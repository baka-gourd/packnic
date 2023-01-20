using System.Text.Json;
using System.Text.Json.Serialization;

namespace Packnic.Core;

public class ModPackage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Version { get; set; } = null!;
    public Platform Platform { get; set; }
    public string Name { get; set; } = null!;
    public string? UniqueId { get; set; }
    public bool IsFixed { get; set; }
    public string? DownloadUrl { get; set; }
    public string? Description { get; set; }
}