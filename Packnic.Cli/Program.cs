using System.Collections.Concurrent;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

using Packnic.Core;

namespace Packnic.Cli;

class Program
{
    public static async Task Main(string[] args)
    {
        CommandLineBuilder cb = new(CommandProvider.GetRootCommand());
        cb.AddMiddleware(async (context, next) =>
        {
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

    private static async Task InstallHandler(string[] items, string? token)
    {
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
                if (File.Exists(path))
                {
                    downloadState.TryUpdate(pkg.Name, false, true);
                    sem.Release();
                    return;
                }

                var bytes = await Utils.NormalClient.GetByteArrayAsync(pkg.DownloadUrl);
                await File.WriteAllBytesAsync(path, bytes);
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

        new Thread(() => Task.WhenAll(tasks)).Start();
        ProgressBar.DownloadMany(names, ref downloadState);
        Console.WriteLine("info: all mods installed.");
    }
}