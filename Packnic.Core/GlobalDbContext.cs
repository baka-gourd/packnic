using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace Packnic.Core;

public class GlobalDbContext : DbContext
{
    public GlobalDbContext()
    {
        Config.Init();
    }

    public DbSet<VirtualPackage>? VirtualPackages { get; set; }
    public DbSet<ModPackage>? Mods { get; set; }
    public DbSet<Relation>? Relations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={Config.GlobalDatabasePath};");
    }
}

[Table("VirtualPackages")]
public class VirtualPackage
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Version { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTimeOffset UpdateTime { get; set; }
    public virtual List<ModPackage>? Mods { get; set; } = new();
}

[Table("Relations")]
public class Relation
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Version { get; set; } = null!;
    public ModPackage Mod { get; set; } = null!;
    public virtual List<ModPackage> Dependencies { get; set; } = new();
    public virtual List<ModPackage> Related { get; set; } = new();
}

[Table("ModPackages")]
public class ModPackage
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Version { get; set; } = null!;
    public Platform Platform { get; set; }
    public string Name { get; set; } = null!;
    public string? UniqueId { get; set; }
    public bool IsFixed { get; set; }
    public string? DownloadUrl { get; set; }
    public string? Description { get; set; }
}