using System.Text.Encodings.Web;
using System.Text.Json;

namespace Packnic.Core.Model;

public class ModTree : List<LocalFileNode>
{
    public static ModTree BuildModTreeByDirectories()
    {
        throw new NotImplementedException();
    }

    public void Add(LocalFile file)
    {
        base.Add(new LocalFileNode(file));
    }

    public LocalFileNode? this[byte[] hash, HashType type]
    {
        get
        {
            var obj = Find(node => type is HashType.MD5 ? node.Md5.ExactEqual(hash) : node.Sha1.ExactEqual(hash));
            return obj;
        }
    }

    public LocalFileNode? this[string name]
    {
        get
        {
            var obj = Find(node => node.Name == name);
            return obj;
        }
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this,
            new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
    }
}