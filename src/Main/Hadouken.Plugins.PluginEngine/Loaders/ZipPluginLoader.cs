using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hadouken.Common.IO;
using Ionic.Zip;
using Hadouken.Common;

namespace Hadouken.Plugins.PluginEngine.Loaders
{
    [Component]
    public class ZipPluginLoader : IPluginLoader
    {
        private readonly IFileSystem _fileSystem;

        public ZipPluginLoader(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public bool CanLoad(string path)
        {
            if (_fileSystem.IsDirectory(path))
                return false;

            byte[] data = _fileSystem.ReadAllBytes(path);
            int header = (data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3];

            return (header == 0x04034b50 && path.EndsWith(".zip"));
        }

        public string[] Load(string path)
        {
            var tmpPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(path));

            using (ZipFile file = ZipFile.Read(_fileSystem.OpenRead(path)))
            {
                if (!_fileSystem.DirectoryExists(tmpPath))
                    _fileSystem.CreateDirectory(tmpPath);

                file.ExtractAll(tmpPath);
            }

            return (from f in _fileSystem.GetFiles(tmpPath, "*.dll")
                    select f).ToArray();
        }
    }
}
