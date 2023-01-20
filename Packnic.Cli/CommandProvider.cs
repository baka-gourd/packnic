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
    public static Option<string> CfToken { get; set; } = new("--cfToken", "CFToken.");

    static CommandProvider()
    {
        CfToken.AddAlias("-cf");

        #region InstallCommand

        InstallCommand.AddAlias("i");
        InstallCommand.AddAlias("add");
        var installArgument = new Argument<string[]>("items", Array.Empty<string>, "Will be installed mods.");
        InstallCommand.Add(installArgument);
        InstallCommand.SetHandler(Program.InstallHandler,installArgument,CfToken);

        #endregion

        #region UninstallCommand

        UninstallCommand.AddAlias("r");
        UninstallCommand.AddAlias("rm");

        #endregion
    }

    public static RootCommand GetRootCommand()
    {
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
        rootCommand.AddGlobalOption(CfToken);
        
        return rootCommand;
    }
}