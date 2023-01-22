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

    public LocalFileNode? this[byte[] hash, HashType type] => Get(hash, type);

    public LocalFileNode? this[string name] => Get(name);

    public LocalFileNode? this[Guid id] => Get(id);

    public LocalFileNode? Get(string name)
    {
        var obj = Find(node => node.Name == name);
        return obj;
    }

    public LocalFileNode? Get(Guid id)
    {
        var obj = Find(node => node.Id.Equals(id));
        return obj;
    }

    public LocalFileNode? Get(byte[] hash, HashType type)
    {
        var obj = Find(node => node.ExactHash(hash, type));
        return obj;
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this,
            new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
    }

    public LocalFileNode? Find(string name)
    {
        var topNode = this[name];
        if (topNode is not null)
        {
            return topNode;
        }

        foreach (var node in this)
        {
            return node.FindChildByName(name);
        }

        return null;
    }

    public LocalFileNode? Find(Guid id)
    {
        var topNode = this[id];
        if (topNode is not null)
        {
            return topNode;
        }

        foreach (var node in this)
        {
            return node.FindChildById(id);
        }

        return null;
    }

    public LocalFileNode? Find(byte[] hash, HashType type)
    {
        var topNode = this[hash, type];
        if (topNode is not null)
        {
            return topNode;
        }

        foreach (var node in this)
        {
            return node.FindChildByHash(hash, type);
        }

        return null;
    }
}