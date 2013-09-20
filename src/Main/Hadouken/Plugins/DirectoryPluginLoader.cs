﻿using System;
using Hadouken.IO;

namespace Hadouken.Plugins
{
    public class DirectoryPluginLoader : IPluginLoader
    {
        private readonly IFileSystem _fileSystem;

        public DirectoryPluginLoader(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public bool CanLoad(string path)
        {
            return _fileSystem.IsDirectory(path);
        }

        public IPluginManager Load(string path)
        {
            // Check if path has valid manifest
            return new PluginManager(path, _fileSystem);
        }
    }
}