using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using System.Text.Json;

using Packnic.Core.Model;
using Packnic.Core.Model.Instance;

namespace Packnic.Core;

public class InstanceManager
{
    private readonly string _instancePath;
    private bool Initialized => IsInitialized();
    private string DataPath => Path.Combine(_instancePath, ".packnic");
    private string ConfigPath => Path.Combine(DataPath, "config");
    private string TreePath => Path.Combine(DataPath, "tree");
    private string CurseforgeDependencies => Path.Combine(DataPath, "curseforgeDeps");
    private volatile List<CurseForgeDependenciesNode> _curseForgeDependencies = null!;
    private volatile ModTree _tree = null!;
    private Source _source = Source.None;
    private readonly CacheManager _manager;
    public ConcurrentDictionary<string, bool> DownloadState = new();
    public List<string> Names { get; set; } = new();
    private ConcurrentDictionary<ModPackage, LocalFile> TreeData { get; set; } = new();

    public InstanceManager(string instancePath, CacheManager manager)
    {
        _instancePath = instancePath;
        _manager = manager;
        if (!Initialized)
        {
            Init();
        }

        ReadData();
    }

    private void Init()
    {
        Directory.CreateDirectory(DataPath);
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(
            new InstanceConfig
            {
                Source = Source.Curseforge
            },
            new JsonSerializerOptions { WriteIndented = true }));
    }

    private void ReadData()
    {
        _source = JsonSerializer.Deserialize<InstanceConfig>(File.ReadAllText(ConfigPath))!.Source;
        if (File.Exists(TreePath))
        {
            _tree = JsonSerializer.Deserialize<ModTree>(File.ReadAllText(TreePath)) ?? new ModTree();
        }
        else
        {
            _tree = new ModTree();
        }

        if (File.Exists(CurseforgeDependencies))
        {
            _curseForgeDependencies = JsonSerializer.Deserialize<List<CurseForgeDependenciesNode>>(File.ReadAllText(CurseforgeDependencies)) ?? new List<CurseForgeDependenciesNode>();
        }
    }

    private bool IsInitialized()
    {
        if (!Directory.Exists(DataPath) &&
            !File.Exists(ConfigPath))
        {
            return false;
        }
        return true;
    }

    public void AddMod(ModPackage mod)
    {
        // download mod
        SemaphoreSlim sem = new SemaphoreSlim(8);
        var list = UnfoldPackageRelation(mod);
        foreach (var modPackage in list)
        {
            Names.Add(modPackage.Name);
            DownloadState.TryAdd(modPackage.Name, true);
        }
        var tasks = list.Select(async pkg =>
        {
            try
            {
                await sem.WaitAsync();
                var path = Path.Combine(_instancePath, Path.GetFileName(pkg.DownloadUrl)!);

                var fileInfo = new FileInfo(path);

                if (File.Exists(
                        (fileInfo.Attributes & FileAttributes.ReparsePoint) is FileAttributes.ReparsePoint ?
                            fileInfo.LinkTarget
                            : path))
                {
                    DownloadState.TryUpdate(pkg.Name, false, true);
                    sem.Release();
                    return;
                }

                var state = _manager.TryAddFileToCache(pkg.DownloadUrl!, Path.GetFileName(pkg.DownloadUrl)!,
                    out var file);

                if (state)
                {
                    if (Utils.IsRoot)
                    {
                        if (File.Exists(path) && new FileInfo(path).LinkTarget == file!.Path)
                        {
                            return;
                        }
                        fileInfo.CreateAsSymbolicLink(file!.Path);
                    }
                    else
                    {
                        var fs = File.OpenRead(file!.Path);
                        var dst = fileInfo.Create();
                        await fs.CopyToAsync(dst);
                        dst.Close();
                        fs.Close();
                        //File.Copy(file!.Path, path, true);
                    }

                    StoreExtendData(pkg,file);
                    TreeData.TryAdd(pkg, file);
                }
                else
                {
                    var hash = Convert.FromHexString(pkg.Hash!);
                    var stagedFile = _manager.GetFile(hash, pkg.HashType);
                    if (Utils.IsRoot)
                    {
                        if (File.Exists(path) && new FileInfo(path).LinkTarget == file!.Path)
                        {
                            return;
                        }
                        File.CreateSymbolicLink(path, stagedFile!.Path);
                    }
                    else
                    {
                        File.Copy(stagedFile!.Path, path, true);
                    }

                    StoreExtendData(pkg, stagedFile);
                    TreeData.TryAdd(pkg, stagedFile);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e);
            }
            finally
            {
                DownloadState.TryUpdate(pkg.Name, false, true);
                sem.Release();
            }
        });

        new Thread(() =>
        {
            Task.WaitAll(tasks.ToArray());
            BuildTree();
            _manager.Dispose();
            DownloadState.Clear();
            Names.Clear();
        }).Start();
    }

    public void AddMods(IEnumerable<ModPackage> modPackages)
    {
        foreach (var modPackage in modPackages)
        {
            AddMod(modPackage);
        }
    }

    private List<ModPackage> UnfoldPackageRelation(ModPackage mod)
    {
        var list = new List<ModPackage> { mod };
        if (mod.Children.Count > 0)
        {
            foreach (var dependency in mod.Children)
            {
                list.AddRange(UnfoldPackageRelation(dependency));
            }
        }
        return list.DistinctBy(package => package.DownloadUrl).ToList();
    }

    private void BuildTree()
    {
        var dependenciesList = new List<(string, LocalFile)>();

        // classified mod package.
        foreach (var (modPackage, localFile) in TreeData)
        {
            // if no parents, add to top.
            if (modPackage.Parents.Count is 0)
            {
                _tree.Add(localFile);
                continue;
            }

            // add all dependencies to list.
            modPackage.Parents.ForEach(parent=> dependenciesList.Add((parent.GetFileName(), localFile)));
        }

        var count = dependenciesList.Count;
        // add all dependencies to tree.
        while (count > 0)
        {
            foreach (var (name, file) in dependenciesList)
            {
                var node = _tree.FindByName(name);
                if (node is null) continue;
                node.AddChild(file);
                --count;
            }
        }
        
        TreeData.Clear();
    }

    private void StoreExtendData(ModPackage modPackage, LocalFile file)
    {
        if (modPackage.ExtendData is null)
        {
            return;
        }

        if (modPackage.ExtendData.Source is Source.Curseforge)
        {
            StoreCurseForgeData(modPackage, file);
        }
    }

    private void StoreCurseForgeData(ModPackage modPackage, LocalFile file)
    {
        var node = _curseForgeDependencies.FirstOrDefault(n => n.ModId == modPackage.ExtendData!.ModId);
        if (node is null)
        {
            _curseForgeDependencies.Add(new CurseForgeDependenciesNode
            {
                ModId = modPackage.ExtendData!.ModId,
                Dependencies = modPackage.Children.Select(c => (int)c.ExtendData!.ModId).ToList(),
                Files = new List<CurseForgeFileBind> { new CurseForgeFileBind(modPackage.ExtendData!.FileId, file.Id) }
            });
        }
        else
        {
            var index = _curseForgeDependencies.IndexOf(node);
            var deps = node.Dependencies ?? new List<int>();
            foreach (var child in modPackage.Children)
            {
                if (deps.Contains(child.ExtendData!.ModId))
                {
                    continue;
                }

                deps.Add(child.ExtendData!.ModId);
            }

            node.Dependencies = deps;

            var files = node.Files ?? new List<CurseForgeFileBind>();
            var existed = files.FirstOrDefault(f => f.Id.Equals(file.Id)) is not null;
            if (!existed)
            {
                files.Add(new CurseForgeFileBind(modPackage.ExtendData!.FileId, file.Id));
            }

            node.Files = files;
            _curseForgeDependencies[index] = node;
        }
    }

    private void SaveTree()
    {
        var str = _tree.ToString();
        File.WriteAllText(TreePath, str);
    }

    private void SaveCurseForgeData()
    {
        var str = JsonSerializer.Serialize(_curseForgeDependencies,
            new JsonSerializerOptions() {WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping});
        File.WriteAllText(CurseforgeDependencies,str);
    }

    public bool SaveData()
    {
        SaveTree();
        SaveCurseForgeData();
        return true;
    }
}