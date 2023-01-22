namespace Packnic.Core.Model;

public record LocalFile
{
    public Guid Id { get; init; }
    public byte[] Sha1 { get; init; } = null!;
    public byte[] Md5 { get; init; } = null!;
    public string Path { get; init; } = null!;
    public string Name { get; init; } = null!;
}