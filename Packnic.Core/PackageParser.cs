using CurseForge.APIClient.Models.Files;
using CurseForge.APIClient.Models.Mods;

namespace Packnic.Core;

public static class PackageParser
{
    public static Platform ParsePlatform(string mod)
    {
        var strings = mod.Split("/", 2);
        if (strings.Length < 2)
        {
            throw new ArgumentException("Invalid mod");
        }

        if (strings[0] == Config.CurseForgePrefix)
        {
            return Platform.CurseForge;
        }

        if (strings[0] == Config.ModrinthPrefix)
        {
            return Platform.Modrinth;
        }

        if (strings[0] == Config.CustomPrefix)
        {
            return Platform.Custom;
        }

        return Platform.Unknown;
    }

    public static Platform ParsePlatform(string[] strings)
    {
        if (strings.Length < 2)
        {
            throw new ArgumentException("Invalid mod");
        }

        if (strings[0] == Config.CurseForgePrefix)
        {
            return Platform.CurseForge;
        }

        if (strings[0] == Config.ModrinthPrefix)
        {
            return Platform.Modrinth;
        }

        if (strings[0] == Config.CustomPrefix)
        {
            return Platform.Custom;
        }

        return Platform.Unknown;
    }

    public static async Task<List<ModPackage>> ParseModPackageAsync(string mod, bool pass = false)
    {
        var strings = mod.Split("/", 2);
        if (strings.Length < 2)
        {
            throw new ArgumentException("Invalid mod");
        }

        var platform = ParsePlatform(strings);

        var result = platform switch
        {
            Platform.CurseForge => await ParseCurseForgeAsync(strings[1]),
            Platform.Modrinth => await ParseModrinthAsync(strings[1]),
            Platform.Custom => await ParseCustomAsync(strings[1]),
            Platform.Unknown => throw new ArgumentException("Unknown platform."),
            _ => throw new ArgumentOutOfRangeException()
        };

        return result;
    }

    public static async Task<List<ModPackage>> ParseCurseForgeAsync(string name)
    {
        string? id;
        List<ModPackage> result = new();
        if (name.StartsWith("~"))
        {
            id = name[1..];
        }
        else
        {
            throw new ArgumentException();
        }

        var info = await Utils.CfClient.GetModAsync(int.Parse(id));
        if (info is not null)
        {
            var index = info.Data.LatestFilesIndexes.FirstOrDefault(f => f.GameVersion!.Contains(Config.GameVersion));
            //var downloadUrl = $"https://edge.forgecdn.net/files/{index!.FileId.ToString()[..4]}/{index.FileId.ToString()[4..]}/{index.Filename}";
            if (index is null)
            {
                Console.WriteLine("error: cannot fetch mod information.");
                throw new NullReferenceException();
            }

            if (!info.Data.IsAvailable)
            {
                
            }
            var modFile = await Utils.CfClient.GetModFileAsync(info.Data.Id, index.FileId);
            if (modFile is not null)
            {
                ModPackage main = new()
                {
                    Description = info.Data.Summary,
                    DownloadUrl = modFile.Data.DownloadUrl,
                    IsFixed = false,
                    Name = info.Data.Name,
                    Platform = Platform.CurseForge,
                    UniqueId = info.Data.Id.ToString(),
                    Version = Config.GameVersion
                };
                result.Add(main);
                if (modFile.Data.Dependencies is { Count: > 0 })
                {
                    var mods = new List<int>();
                    foreach (var dependency in modFile.Data.Dependencies)
                    {
                        if (dependency.RelationType is not FileRelationType.RequiredDependency)
                        {
                            continue;
                        }
                        mods.Add(dependency.ModId);
                    }

                    var dependencies = await ParseCurseForgeInBatchAsync(mods);
                    result.AddRange(dependencies);
                }
            }
        }

        return result.DistinctBy(m => m.DownloadUrl).ToList();
    }

    public static async Task<List<ModPackage>> ParseCurseForgeInBatchAsync(List<int> ids)
    {
        List<ModPackage> result = new();
        var infos = await Utils.CfClient.GetModsByIdListAsync(new GetModsByIdsListRequestBody() { ModIds = ids });

        if (infos is not null)
        {
            foreach (var mod in infos.Data)
            {
                var index = mod.LatestFilesIndexes.FirstOrDefault(f => f.GameVersion!.Contains(Config.GameVersion));
                //var downloadUrl = $"https://edge.forgecdn.net/files/{index!.FileId.ToString()[..4]}/{index.FileId.ToString()[4..]}/{index.Filename}";
                if (index is null)
                {
                    Console.WriteLine("error: cannot fetch mod information.");
                    throw new NullReferenceException();
                }
                var modFile = await Utils.CfClient.GetModFileAsync(mod.Id, index.FileId);
                if (modFile is not null)
                {
                    ModPackage main = new()
                    {
                        Description = mod.Summary,
                        DownloadUrl = modFile.Data.DownloadUrl,
                        IsFixed = false,
                        Name = mod.Name,
                        Platform = Platform.CurseForge,
                        UniqueId = mod.Id.ToString(),
                        Version = Config.GameVersion
                    };
                    result.Add(main);
                    if (modFile.Data.Dependencies is { Count: > 0 })
                    {
                        var mods = new List<int>();
                        foreach (var dependency in modFile.Data.Dependencies)
                        {
                            if (dependency.RelationType is not FileRelationType.RequiredDependency)
                            {
                                continue;
                            }
                            mods.Add(dependency.ModId);
                        }

                        var dependencies = await ParseCurseForgeInBatchAsync(mods);
                        result.AddRange(dependencies);
                    }
                }
            }
        }

        return result.DistinctBy(m => m.DownloadUrl).ToList();
    }

    public static async Task<List<ModPackage>> ParseModrinthAsync(string name)
    {
        throw new NotImplementedException();
    }

    public static async Task<List<ModPackage>> ParseModrinthInBatchAsync(string[] ids)
    {
        throw new NotImplementedException();
    }

    public static async Task<List<ModPackage>> ParseCustomAsync(string name)
    {
        throw new NotImplementedException();
    }
}