using System.Text.Json;
using System.Text.Json.Serialization;

namespace Packnic.Core;

public class Config
{
    private static string _globalDataPath { get; set; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".packnic");

    private static ConfigTemplate? _configTemplate;
    private static string _curseForgePrefix  = "c";
    private static string _modrinthPrefix  = "m";
    private static string _customPrefix  = "cu";

    public static void LoadConfig()
    {
        if (!File.Exists(Path.Combine(_globalDataPath, "customize.json")))
        {
            return;
        }

        string file = File.ReadAllText(Path.Combine(_globalDataPath, "customize.json"));
        ConfigTemplate? config = JsonSerializer.Deserialize<ConfigTemplate>(file);
        if (config is not null)
        {
            _configTemplate = config;
        }
    }

    static Config()
    {
        LoadConfig();
    }

    public static string GlobalDataPath => _globalDataPath;
    public static string CachePath => Path.Combine(GlobalDataPath, ".cache");
    public static string CurseForgePrefix => _configTemplate?.CurseForgePrefix ?? _curseForgePrefix;
    public static string ModrinthPrefix => _configTemplate?.ModrinthPrefix ?? _modrinthPrefix;
    public static string CustomPrefix => _configTemplate?.CustomPrefix ?? _customPrefix;
    public static string GameVersion = "1.12.2";
}

public class ConfigTemplate
{
    public string? CurseForgePrefix { get; set; }
    public string? ModrinthPrefix { get; set; }
    public string? CustomPrefix { get; set; }
}