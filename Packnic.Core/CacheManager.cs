using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;

using Packnic.Core.Model;

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
            Directory.CreateDirectory(path);
        }

        CreateIndex();
    }

    private void CreateIndex()
    {
        if (!File.Exists(_indexFile))
        {
            File.WriteAllText(_indexFile, JsonSerializer.Serialize(Array.Empty<LocalFile>()));
            GenerateIndex();
        }

        if (DateTime.UtcNow - File.GetLastWriteTimeUtc(_indexFile) > TimeSpan.FromDays(3))
        {
            GenerateIndex();
        }
    }

    private void GenerateIndex()
    {
        var directory = new DirectoryInfo(_path);

        var files = new ConcurrentBag<LocalFile>();
        var result = Parallel.ForEach(directory.GetFiles(), file =>
        {
            if (file.Name is ".index")
            {
                return;
            }

            var bytes = File.ReadAllBytes(file.FullName);
            var sha1 = SHA1.HashData(bytes);
            var md5 = MD5.HashData(bytes);

            files.Add(new LocalFile
            {
                Id = Guid.NewGuid(),
                Md5 = md5,
                Name = file.Name,
                Path = file.FullName.Replace("\\","/"),
                Sha1 = sha1
            });
        });

        if (!result.IsCompleted)
        {
            throw new Exception();
        }

        File.WriteAllText(_indexFile, JsonSerializer.Serialize(files, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
    }

    public void RefreshIndex()
    {
        GenerateIndex();
    }
}