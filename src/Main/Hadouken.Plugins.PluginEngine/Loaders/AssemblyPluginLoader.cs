﻿using System.Collections.Generic;
using Hadouken.Common.IO;
using Hadouken.Common;

namespace Hadouken.Plugins.PluginEngine.Loaders
{
    [Component]
    public class AssemblyPluginLoader : IPluginLoader
    {
        private readonly IFileSystem _fileSystem;

        public AssemblyPluginLoader(IFileSystem fs)
        {
            _fileSystem = fs;
        }

        public bool CanLoad(string path)
        {
            return path.EndsWith(".dll");
        }

        public string[] Load(string path)
        {
            return new [] { path };
        }
    }
}
