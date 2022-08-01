using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

using Packnic.Core;

namespace Packnic.Cli;

class Program
{
    public static async Task Main(string[] args)
    {
        RootCommand rootCommand = new("Packnic! RUA!");
        Option<string?> cfToken = new("--cfToken", "CFToken.");
        cfToken.AddAlias("-cf");
        rootCommand.AddGlobalOption(cfToken);
        Command install = new("install", "Install a mod.");
        install.AddAlias("i");
        install.AddAlias("add");
        Command uninstall = new("uninstall", "Uninstall a mod");
        uninstall.AddAlias("r");
        uninstall.AddAlias("rm");
        Command import = new("import", "Import a modpack.");
        Command export = new("export", "Export a modpack.");
        Command update = new("update", "Update mod information.");
        Command upgrade = new("upgrade", "Upgrade mod(s).");
        Command search = new("search", "Search a mod.");
        Command init = new("init", "Init a project.");
        Argument<string[]> installArgument = new("items", Array.Empty<string>, "Will be installed mods.");
        install.AddArgument(installArgument);
        install.SetHandler(InstallHandler, installArgument, cfToken);
        search.AddAlias("s");
        rootCommand.Add(install);
        rootCommand.Add(uninstall);
        rootCommand.Add(import);
        rootCommand.Add(export);
        rootCommand.Add(update);
        rootCommand.Add(upgrade);
        rootCommand.Add(search);
        rootCommand.Add(init);
        CommandLineBuilder cb = new(rootCommand);
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

    private static void InstallHandler(string[] items, string? token)
    {
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
            Console.WriteLine("info: load database...");
            if (platform is Platform.VirtualPackage)
            {
                VirtualPackage? selected = null;
                var packages = PackageParser.ParseVirtualPackages(strings[1]);
                if (packages.Length == 0)
                {
                    Console.WriteLine("warn: cannot find any virtual package");
                    return;
                }
                if (packages.Length > 1)
                {
                    selected = packages[ProgressBar.BuilderSelector(packages.Select(p => p.Name).ToArray()!)];
                }
                else
                {
                    selected = packages[0];
                }

                foreach (var mod in selected.Mods!)
                {
                    Console.WriteLine(mod.DownloadUrl);
                }
            }
        }
    }
}