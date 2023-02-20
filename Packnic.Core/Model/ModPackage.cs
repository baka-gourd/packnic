using System.Text.Json;
using System.Text.Json.Serialization;
using Packnic.Core.Model;

namespace Packnic.Core;

public class ModPackage
{
    public string Version { get; set; } = null!;
    public Platform Platform { get; set; }
    public string Name { get; set; } = null!;
    public string? DownloadUrl { get; set; }
    public List<ModPackage> Children { get; set; } = new();
    public List<ModPackage> Parents { get; set; } = new();
    public string? Hash { get; set; }
    public HashType HashType { get; set; }
    public dynamic? ExtendData { get; set; }
}