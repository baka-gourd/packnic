using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

using Packnic.Core.Model;
using Packnic.Core.Model.Instance;

namespace Packnic.Core;

public class InstanceManager
{
    public readonly string InstancePath;
    private bool Initialized => IsInitialized();
    private string DataPath => System.IO.Path.Combine(InstancePath, ".packnic");
    private string ConfigPath => System.IO.Path.Combine(DataPath, "config");
    private string TreePath => System.IO.Path.Combine(DataPath, "tree");
    private volatile ModTree _tree = null!;
    private Source _source = Source.None;
    private CacheManager _manager;
    public ConcurrentDictionary<string, bool> DownloadState = new();
    public List<string> Names { get; set; } = new();
    public ConcurrentDictionary<ModPackage, LocalFile> TreeData { get; set; } = new();

    public InstanceManager(string instancePath, CacheManager manager)
    {
        InstancePath = instancePath;
        _manager = manager;
        if (!Initialized)
        {
            Init();
        }

        ReadData();
    }

    ~InstanceManager()
    {
        SaveTree();
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
        var list = UnfoldModPackages(mod);
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
                var path = Path.Combine(InstancePath, Path.GetFileName(pkg.DownloadUrl)!);

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

        Task.WaitAll(tasks.ToArray());
        BuildTree();
        _manager.Dispose();
        DownloadState.Clear();
        Names.Clear();
    }

    public void AddMods(IEnumerable<ModPackage> modPackages)
    {
        foreach (var modPackage in modPackages)
        {
            AddMod(modPackage);
        }
    }

    private List<ModPackage> UnfoldModPackages(ModPackage mod)
    {
        var list = new List<ModPackage> { mod };
        if (mod.Children.Count > 0)
        {
            foreach (var dependency in mod.Children)
            {
                list.AddRange(UnfoldModPackages(dependency));
            }
        }
        return list.DistinctBy(package => package.DownloadUrl).ToList();
    }

    private void BuildTree()
    {
        var depDict = new List<(string, LocalFile)>();
        var added = 1;
        var addedName = new List<string>();

        foreach (var (modPackage, localFile) in TreeData)
        {
            if (modPackage.Parents.Count is 0)
            {
                _tree.Add(localFile);
                continue;
            }

            foreach (var parent in modPackage.Parents)
            {
                depDict.Add((parent.GetFileName(), localFile));
            }
        }

        while (added < depDict.Count)
        {
            foreach (var (name, file) in depDict)
            {
                if (addedName.Contains(name))
                {
                    continue;
                }
                var node = _tree.FindByName(name);
                if (node is null)
                {
                    continue;
                }
                node.AddChild(file);
                added++;
                addedName.Add(name);
            }
        }

        TreeData.Clear();
        SaveTree();
    }

    private void SaveTree()
    {
        var str = _tree.ToString();
        File.WriteAllText(TreePath, str);
    }
}