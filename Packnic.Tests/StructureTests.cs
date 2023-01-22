using System.Text.Json;
using Packnic.Core;
using Packnic.Core.Model;

namespace Packnic.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ModTreeTest()
        {
            var tree = new ModTree();
            tree.Add(new LocalFile
            {
                Id = Guid.NewGuid(),
                Md5 = Array.Empty<byte>(),
                Name = "test",
                Path = "./1",
                Sha1 = Array.Empty<byte>()
            });

            tree["test"]?.AddChild(new LocalFile
            {
                Id = Guid.NewGuid(),
                Md5 = Array.Empty<byte>(),
                Name = "test2",
                Path = "./2",
                Sha1 = Array.Empty<byte>()
            });
            
            tree["test"]?.AddChild(new LocalFile
            {
                Id = Guid.NewGuid(),
                Md5 = Array.Empty<byte>(),
                Name = "test3",
                Path = "./3",
                Sha1 = Array.Empty<byte>()
            });

            tree["test"]?.Children.FindByName("test2")?.AddChild(new LocalFile
            {
                Id = Guid.NewGuid(),
                Md5 = Array.Empty<byte>(),
                Name = "test4",
                Path = "./4",
                Sha1 = Array.Empty<byte>()
            });

            var str = tree.ToString();
            var obj = JsonSerializer.Deserialize<ModTree>(str);
            var str2 = obj!.ToString();
            Assert.AreEqual(str2,str);
        }
    }
}