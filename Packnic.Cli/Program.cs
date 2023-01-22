using System.Collections.Concurrent;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Packnic.Core;

namespace Packnic.Cli;

class Program
{
    public static bool IsRoot => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
                          new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    public static async Task Main(string[] args)
    {
        CommandLineBuilder cb = new(CommandProvider.GetRootCommand());
        cb.AddMiddleware(async (context, next) =>
        {
            if (IsRoot)
            {
                Console.WriteLine("info: Cli will create \"SymbolicLink\" instead of copy file.");
            }
            if (!CheckEnvironment())
            {
                Console.WriteLine("error: not a packnic project");
                return;
            }

            await next(context);
        });
        cb.UseDefaults();
        var parser = cb.Build();
        await parser.InvokeAsync(args);
    }

    private static bool CheckEnvironment()
    {
        return true;
    }

    public static async Task InstallHandler(string[] items, string? token)
    {
        Utils.InitCfApi("$2a$10$bL4bIL5pUWqfcO7KQtnMReakwtfHbNKh6v1uTpKlzhwoueEJQnPnm");
        var cache = new CacheFileProvider(Config.CachePath);
        var cacheManager = cache.GetModCacheByVersion(Config.GameVersion);
        cacheManager.RefreshIndex();

        var list = new List<ModPackage>();
        if (token is not null)
        {
            Console.WriteLine(token);
        }
        foreach (var s in items)
        {
            var strings = s.Split("/", 2);
            if (strings.Length < 2)
            {
                throw new ArgumentException("Invalid mod");
            }
            var platform = PackageParser.ParsePlatform(strings);

            if (platform is Platform.CurseForge)
            {
                bool processing = true;
                new Thread(() => ProgressBar.ResloveDependencies(strings[1], ref processing)).Start();
                list.AddRange(await PackageParser.ParseCurseForgeAsync(strings[1]));
                processing = false;
            }
        }

        list = list.DistinctBy(m => m.DownloadUrl).ToList();

        SemaphoreSlim sem = new SemaphoreSlim(8);
        List<string> names = new();
        ConcurrentDictionary<string, bool> downloadState = new();
        foreach (var modPackage in list)
        {
            names.Add(modPackage.Name);
            downloadState.TryAdd(modPackage.Name, true);
        }

        var tasks = list.Select(async pkg =>
        {
            try
            {
                await sem.WaitAsync();
                var path = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileName(pkg.DownloadUrl)!);

                var fileInfo = new FileInfo(path);

                if (File.Exists(
                        (fileInfo.Attributes & FileAttributes.ReparsePoint) is FileAttributes.ReparsePoint ?
                            fileInfo.LinkTarget
                            : path))
                {
                    downloadState.TryUpdate(pkg.Name, false, true);
                    sem.Release();
                    return;
                }

                var state = cacheManager.TryAddFileToCache(pkg.DownloadUrl!, Path.GetFileName(pkg.DownloadUrl)!,
                    out var file);

                if (state)
                {
                    if (IsRoot)
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
                }
                else
                {
                    var hash = Convert.FromHexString(pkg.Hash!);
                    if (IsRoot)
                    {
                        if (File.Exists(path) && new FileInfo(path).LinkTarget == file!.Path)
                        {
                            return;
                        }
                        File.CreateSymbolicLink(path, cacheManager.GetFile(hash, pkg.HashType)!.Path);
                    }
                    else
                    {
                        File.Copy(cacheManager.GetFile(hash, pkg.HashType)!.Path, path, true);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e);
            }
            finally
            {
                downloadState.TryUpdate(pkg.Name, false, true);
                sem.Release();
            }
        });

        new Thread(() =>
        {
            Task.WaitAll(tasks.ToArray());
            cacheManager.Dispose();
        }).Start();
        ProgressBar.DownloadMany(names, ref downloadState);
        Console.WriteLine("info: all mods installed.");
    }
}