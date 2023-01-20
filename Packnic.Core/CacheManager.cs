using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;

using Packnic.Core.Model;

namespace Packnic.Core;

public class CacheManager : IDisposable
{
    private string _path;
    private string _indexFile => Path.Combine(_path, ".index");
    public ConcurrentBag<LocalFile>? LocalFiles { get; set; }

    public CacheManager(string path)
    {
        _path = path;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        CreateIndex();
        ReadIndex();
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
                Path = file.FullName.Replace("\\", "/"),
                Sha1 = sha1
            });
        });

        if (!result.IsCompleted)
        {
            throw new Exception();
        }

        File.WriteAllText(_indexFile, JsonSerializer.Serialize(files, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
        LocalFiles = files;
    }

    void ReadIndex()
    {
        LocalFiles = new ConcurrentBag<LocalFile>(JsonSerializer.Deserialize<List<LocalFile>>(File.ReadAllText(_indexFile)) ?? new List<LocalFile>());
    }

    public void RefreshIndex()
    {
        GenerateIndex();
    }

    public bool TryAddFileToCache(string remoteUrl, string name, out LocalFile? file)
    {
        LocalFile? tmp = null;
        try
        {
            tmp = AddFileToCache(remoteUrl, name).Result;
            file = tmp;
            return true;
        }
        catch
        {
            file = tmp;
            return false;
        }
    }

    private async Task<LocalFile> AddFileToCache(string remoteUrl, string name)
    {
        if (File.Exists(Path.Combine(_path, name)))
        {
            throw new Exception();
        }
        var bytes = await Utils.NormalClient.GetByteArrayAsync(remoteUrl);
        var path = Path.Combine(_path, name);
        var md5 = MD5.HashData(bytes);
        var sha1 = SHA1.HashData(bytes);

        if (LocalFiles!.FirstOrDefault(file => file.Md5.ExactEqual(md5) && file.Sha1.ExactEqual(sha1)) is not null)
        {
            throw new Exception();
        }

        var file = new LocalFile
        {
            Id = Guid.NewGuid(),
            Md5 = md5,
            Sha1 = sha1,
            Name = name,
            Path = path
        };

        await File.WriteAllBytesAsync(path, bytes);

        LocalFiles!.Add(file);
        return file;
    }

    public LocalFile GetFile(byte[] hash, HashType type)
    {
        return LocalFiles!.FirstOrDefault(file => type is HashType.MD5 ? file.Md5.ExactEqual(hash) : file.Sha1.ExactEqual(hash))!;
    }

    public void Dispose()
    {
        RefreshIndex();
    }
}