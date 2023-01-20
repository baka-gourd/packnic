using System.CommandLine;

namespace Packnic.Cli;

public class CommandProvider
{
    public static Command InstallCommand { get; set; } = new("install", "Install a mod.");
    public static Command UninstallCommand { get; set; } = new("uninstall", "Uninstall a mod");
    public static Command ImportCommand { get; set; } = new("import", "Import a modpack.");
    public static Command ExportCommand { get; set; } = new("export", "Export a modpack.");
    public static Command UpdateCommand { get; set; } = new("update", "Update mod information.");
    public static Command UpgradeCommand { get; set; } = new("upgrade", "Upgrade mod(s).");
    public static Command SearchCommand { get; set; } = new("search", "Search a mod.");
    public static Command InitCommand { get; set; } = new("init", "Init a project.");

    static CommandProvider()
    {
        #region InstallCommand

        InstallCommand.AddAlias("i");
        InstallCommand.AddAlias("add");
        var installArgument = new Argument<string[]>("items", Array.Empty<string>, "Will be installed mods.");
        InstallCommand.Add(installArgument);

        #endregion

        #region UninstallCommand

        UninstallCommand.AddAlias("r");
        UninstallCommand.AddAlias("rm");

        #endregion
    }

    public static RootCommand GetRootCommand()
    {
        Option<string?> cfToken = new("--cfToken", "CFToken.");
        cfToken.AddAlias("-cf");
        RootCommand rootCommand = new("Packnic! RUA!")
        {
            InstallCommand,
            UninstallCommand,
            ImportCommand,
            ExportCommand,
            UpdateCommand,
            UpgradeCommand,
            SearchCommand,
            InitCommand
        };
        rootCommand.AddGlobalOption(cfToken);
        
        return rootCommand;
    }
}