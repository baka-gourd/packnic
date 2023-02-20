namespace Packnic.Core.Model.Instance;

public record CurseForgeDependenciesNode
{
    public int ModId { get; set; }
    public List<CurseForgeFileBind>? Files { get; set; }
    public List<int>? Dependencies { get; set; }
}

public record CurseForgeFileBind(int FileId, Guid Id);