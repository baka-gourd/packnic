using System.Collections.Concurrent;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Runtime.InteropServices;
using System.Security.Principal;

using Packnic.Core;

namespace Packnic.Cli;

class Program
{
    public static async Task Main(string[] args)
    {
        CommandLineBuilder cb = new(CommandProvider.GetRootCommand());
        cb.AddMiddleware(async (context, next) =>
        {
            if (Utils.IsRoot)
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
        var instanceManager = new InstanceManager(Directory.GetCurrentDirectory(), cacheManager);

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
                list.Add(await PackageParser.ParseCurseForgeAsync(strings[1]));
                processing = false;
            }
        }

        instanceManager.AddMods(list);
        ProgressBar.DownloadMany(instanceManager.Names, ref instanceManager.DownloadState);
        Console.WriteLine("info: all mods installed.");
    }
}