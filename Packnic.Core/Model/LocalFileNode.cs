namespace Packnic.Core.Model;

public record LocalFileNode:LocalFile
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
        Id = Guid.NewGuid();
        Parents = new List<Guid>();
        Children = new List<LocalFileNode>();
    }

    // for JsonSerializer
    public LocalFileNode()
    {
    }

    public void AddChild(LocalFileNode node)
    {
        node.AddParent(this);
        Children.Add(node);
    }
    
    public void AddChild(LocalFile file)
    {
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
}