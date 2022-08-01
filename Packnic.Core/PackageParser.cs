using Microsoft.EntityFrameworkCore;

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

        if (strings[0] == Config.VirtualPackagePrefix)
        {
            return Platform.VirtualPackage;
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

        if (strings[0] == Config.VirtualPackagePrefix)
        {
            return Platform.VirtualPackage;
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

    public static IEnumerable<ModPackage> ParseModPackage(string mod, bool pass = false)
    {
        var strings = mod.Split("/", 2);
        if (strings.Length < 2)
        {
            throw new ArgumentException("Invalid mod");
        }

        var platform = ParsePlatform(strings);

        var result = platform switch
        {
            Platform.CurseForge => ParseCurseForge(strings[1]),
            Platform.Modrinth => ParseModrinth(strings[1]),
            Platform.Custom => ParseCustom(strings[1]),
            Platform.Unknown => throw new ArgumentException("Unknown platform."),
            _ => throw new ArgumentOutOfRangeException()
        };

        return result;
    }

    public static VirtualPackage[] ParseVirtualPackages(string name) =>
        Resources.GlobalDb.VirtualPackages!
            .Include(p => p.Mods)
            .Where(_ => _.Name.Contains(name))
            .ToArray();

    public static IEnumerable<ModPackage> ParseCurseForge(string name)
    {
        throw new NotImplementedException();
    }

    public static IEnumerable<ModPackage> ParseModrinth(string name)
    {
        throw new NotImplementedException();
    }

    public static IEnumerable<ModPackage> ParseCustom(string name)
    {
        throw new NotImplementedException();
    }
}