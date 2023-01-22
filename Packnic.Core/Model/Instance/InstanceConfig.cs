using System.Text.Json.Serialization;

namespace Packnic.Core.Model.Instance;

public class InstanceConfig
{
    public Source Source { get; set; }

    [JsonExtensionData]
    public Dictionary<string,object>? Other { get; set; }
}