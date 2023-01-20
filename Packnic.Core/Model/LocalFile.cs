namespace Packnic.Core.Model;

public class LocalFile
{
    public Guid Id { get; set; }
    public byte[] Sha1 { get; set; }
    public byte[] Md5 { get; set; }
    public string Path { get; set; }
    public string Name { get; set; }
}