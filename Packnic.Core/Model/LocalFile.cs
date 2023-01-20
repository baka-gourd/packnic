namespace Packnic.Core.Model;

public record LocalFile
{
    public Guid Id { get; set; }
    public byte[] Sha1 { get; set; } = null!;
    public byte[] Md5 { get; set; } = null!;
    public string Path { get; set; } = null!;
    public string Name { get; set; } = null!;
}