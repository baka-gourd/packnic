namespace Packnic.Core.Model;

public record LocalFileNode : LocalFile
{
    /// <summary>
    /// DO NOT ADD Parents BY Add().
    /// </summary>
    public List<Guid> Parents { get; set; }

    /// <summary>
    /// DO NOT ADD Children BY Add().
    /// </summary>
    public List<LocalFileNode> Children { get; set; }

    public LocalFileNode(LocalFile file)
    {
        Md5 = file.Md5;
        Sha1 = file.Sha1;
        Path = file.Path;
        Name = file.Name;
        Id = file.Id;
        Parents = new List<Guid>();
        Children = new List<LocalFileNode>();
    }

    // for JsonSerializer
#pragma warning disable CS8618
    public LocalFileNode()
#pragma warning restore CS8618
    {
    }

    public void AddChild(LocalFileNode node)
    {
        if (Children.FindById(node.Id) is not null)
        {
            return;
        }
        node.AddParent(this);
        Children.Add(node);
    }

    public void AddChild(LocalFile file)
    {
        // good QoL.
        if (Children.FindById(file.Id) is not null)
        {
            return;
        }
        var node = new LocalFileNode(file);
        node.AddParent(this);
        Children.Add(node);
    }

    public void AddParent(LocalFileNode node)
    {
        Parents.Add(node.Id);
    }

    public void AddParent(LocalFile file)
    {
        Parents.Add(new LocalFileNode(file).Id);
    }

    public void AddChildren(IEnumerable<LocalFileNode> nodes)
    {
        foreach (var node in nodes)
        {
            AddChild(node);
        }
    }

    public void AddChildren(IEnumerable<LocalFile> files)
    {
        foreach (var file in files)
        {
            AddChild(file);
        }
    }

    public LocalFileNode? FindChildByName(string name)
    {
        var child = Children.Find(node => node.Name == name);
        if (child is not null)
        {
            return child;
        }

        foreach (var node in Children)
        {
            return node.FindChildByName(name);
        }

        return null;
    }

    public LocalFileNode? FindChildById(Guid id)
    {
        var child = Children.Find(node => node.Id.Equals(id));
        if (child is not null)
        {
            return child;
        }

        foreach (var node in Children)
        {
            return node.FindChildById(id);
        }

        return null;
    }

    public LocalFileNode? FindChildByHash(byte[] hash, HashType type)
    {
        var child = Children.Find(node => node.ExactHash(hash, type));
        if (child is not null)
        {
            return child;
        }

        foreach (var node in Children)
        {
            return node.FindChildByHash(hash, type);
        }

        return null;
    }
}