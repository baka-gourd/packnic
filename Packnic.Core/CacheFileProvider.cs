using Packnic.Core.Model;

namespace Packnic.Core;

public class CacheFileProvider
{
    private readonly string _cacheDirectory;
    private string _modCache => Path.Combine(_cacheDirectory, "mods");

    public CacheFileProvider(string cachePath)
    {
        _cacheDirectory = cachePath;
        if (!Directory.Exists(cachePath))
        {
            Directory.CreateDirectory(cachePath);
        }

        if (!Directory.Exists(_modCache))
        {
            Directory.CreateDirectory(_modCache);
        }
    }

    public CacheManager GetModCacheByVersion(string version)
    {
        var path = Path.Combine(_modCache, version);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return new CacheManager(path);
    }

    public List<LocalFile> GetAllFiles()
    {
        throw new NotImplementedException();
    }
}