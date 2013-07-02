using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hadouken.Http
{
    public class FileSystemProvider : IFileProvider
    {
        private readonly string _basePath;

        public FileSystemProvider(string basePath)
        {
            _basePath = Path.GetFullPath(basePath);
        }

        public byte[] GetFile(string path)
        {
            path = _basePath + (path == "/" ? "/index.html" : path);
            return !File.Exists(path) ? null : File.ReadAllBytes(path);
        }
    }
}
