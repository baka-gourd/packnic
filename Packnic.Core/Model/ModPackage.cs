using System.Text.Json;
using System.Text.Json.Serialization;
using Packnic.Core.Model;

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
    public string? Hash { get; set; }
    public HashType HashType { get; set; }
}