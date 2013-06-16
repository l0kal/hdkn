﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hadouken.Plugins;
using Hadouken.Data;
using Hadouken.Configuration;
using Hadouken.IO;
using Hadouken.Reflection;
using NLog;
using System.Reflection;
using Hadouken.Events.Plugin;

namespace Hadouken.Impl.Plugins
{
    [Component(ComponentLifestyle.Singleton)]
    public class DefaultPluginEngine : IPluginEngine
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private Dictionary<string, IPluginManager> _managers = new Dictionary<string, IPluginManager>(StringComparer.InvariantCultureIgnoreCase);

        private readonly IFileSystem _fileSystem;
        private readonly IPluginEventPublisher _pluginPublisher;
        private readonly IMigrationRunner _migrationRunner;
        private readonly IPluginLoader[] _pluginLoaders;

        public DefaultPluginEngine(IFileSystem fileSystem,
                                   IPluginEventPublisher pluginPublisher,
                                   IMigrationRunner migrationRunner,
                                   IPluginLoader[] pluginLoaders)
        {
            _fileSystem = fileSystem;
            _pluginPublisher = pluginPublisher;
            _migrationRunner = migrationRunner;
            _pluginLoaders = pluginLoaders;
        }

        public void Load()
        {
            var path = HdknConfig.GetPath("Paths.Plugins");

            foreach (var info in _fileSystem.GetFileSystemInfos(path)
                                    .Select(i => i.FullName))
            {
                Load(info);
            }
        }

        public void Load(string path)
        {
            // Do we have a IPluginLoader for this path?
            var loader = _pluginLoaders.FirstOrDefault(l => l.CanLoad(path));

            if (loader == null)
            {
                Logger.Info("No IPluginLoader can handle the path {0}", path);
                return;
            }

            var assemblies = loader.Load(path);
            Load(assemblies);
        }

        public void Load(IEnumerable<byte[]> rawAssemblies)
        {
            var assemblies = rawAssemblies.Select(Assembly.Load).ToList();

            // Run migrations
            foreach (var asm in assemblies)
            {
                _migrationRunner.Up(asm);
            }

            var childResolver = Kernel.Resolver.CreateChildResolver(assemblies);
            var plugin = childResolver.Get<IPlugin>();
            var manager = new DefaultPluginManager(plugin);

            // Publish loading
            _pluginPublisher.PublishPluginLoading(new
                {
                    manager.Name,
                    Version = manager.Version.ToString()
                });

            try
            {
                manager.Load();
            }
            catch (Exception e)
            {
                Logger.ErrorException(String.Format("Error when loading plugin '{0}'.", manager.Name), e);
                return;
            }

            _managers.Add(manager.Name, manager);

            // Publish loaded
            _pluginPublisher.PublishPluginLoaded(new
                {
                    manager.Name,
                    Version = manager.Version.ToString()
                });
        }

        public void UnloadAll()
        {
            var managers = _managers.Values;

            foreach (var man in managers)
            {
                try
                {
                    man.Unload();
                }
                catch(Exception e)
                {
                    Logger.ErrorException(String.Format("Could not unload plugin {0}", man.Name), e);
                }
            }
        }

        public IDictionary<string, IPluginManager> Managers
        {
            get { return _managers; }
        }
    }
}
