namespace Packnic.Core;

public static class Config
{
    public static string GlobalDatabasePath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),".packnic","global.db");
    public static string VirtualPackagePrefix { get; set; } = "v";
    public static string CurseForgePrefix { get; set; } = "cf";
    public static string ModrinthPrefix { get; set; } = "m";
    public static string CustomPrefix { get; set; } = "c";

    public static void Init()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(GlobalDatabasePath)!);
    }
}