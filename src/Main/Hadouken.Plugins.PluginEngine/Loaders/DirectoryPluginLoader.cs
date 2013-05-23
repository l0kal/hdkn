﻿using System;
using System.Collections.Generic;
using System.Linq;
using Hadouken.Common.IO;
using Hadouken.Common;

namespace Hadouken.Plugins.PluginEngine.Loaders
{
    [Component]
    public class DirectoryPluginLoader : IPluginLoader
    {
        private readonly IFileSystem _fileSystem;

        public DirectoryPluginLoader(IFileSystem fs)
        {
            _fileSystem = fs;
        }

        public bool CanLoad(string path)
        {
            return _fileSystem.IsDirectory(path);
        }

        public string[] Load(string path)
        {
            return (from f in _fileSystem.GetFiles(path, "*.dll")
                    select f).ToArray();
        }
    }
}
