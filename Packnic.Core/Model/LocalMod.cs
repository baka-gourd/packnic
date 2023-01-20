namespace Packnic.Core.Model;

public class LocalMod
{
    public Guid Id { get; set; }
    public byte[] Sha1 { get; set; }
    public byte[] Md5 { get; set; }
    public string Path { get; set; }
}