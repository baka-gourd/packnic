namespace Packnic.Core;

public class CacheManager
{
    private string _path;
    private string _indexFile => Path.Combine(_path, ".index");

    public CacheManager(string path)
    {
        _path = path;
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException();
        }
    }
}