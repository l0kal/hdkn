﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using NLog;
using Hadouken.Common;
using Hadouken.Configuration;
using Hadouken.Common.IO;
using Hadouken.Common.Messaging;
using Hadouken.Common.Plugins;
using Hadouken.Http;

namespace Hadouken.Plugins.PluginEngine
{
    [Component(ComponentType.Singleton)]
    public class DefaultPluginEngine : IPluginEngine
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IEnvironment _environment;
        private readonly IFileSystem _fileSystem;
        private readonly IKeyValueStore _keyValueStore;
        private readonly IMessageBus _messageBus;
        private readonly IHttpFileSystemServer _httpServer;
        private readonly IPluginLoader[] _pluginLoaders;

        private readonly IDictionary<string, PluginInfo> _plugins =
            new Dictionary<string, PluginInfo>(StringComparer.InvariantCultureIgnoreCase); 

        public DefaultPluginEngine(IEnvironment environment,
                                   IFileSystem fileSystem,
                                   IKeyValueStore keyValueStore,
                                   IMessageBusFactory messageBusFactory,
                                   IHttpFileSystemServer httpServer,
                                   IPluginLoader[] pluginLoaders)
        {
            _environment = environment;
            _fileSystem = fileSystem;
            _keyValueStore = keyValueStore;
            _messageBus = messageBusFactory.Create("hdkn");
            _httpServer = httpServer;
            _pluginLoaders = pluginLoaders;

            _messageBus.Subscribe<KeyValueChangedMessage>(OnBlacklistChanged);
        }

        private void OnBlacklistChanged(KeyValueChangedMessage message)
        {
            if (!String.Equals("plugins.blacklist", message.Key))
                return;

            var blacklist = _keyValueStore.Get(message.Key, new string[] {});
            var unblacklistedItems = _plugins.Keys.Except(blacklist);

            foreach (var item in blacklist)
            {
                if (_plugins.ContainsKey(item) && _plugins[item].State == PluginState.Loaded)
                    Unload(item);
            }

            foreach (var item in unblacklistedItems)
            {
                if (_plugins.ContainsKey(item) && _plugins[item].State == PluginState.Unloaded)
                    LoadSandbox(item);
            }
        }

        public IEnumerable<IPluginInfo> Plugins
        {
            get { return _plugins.Values; }
        } 

        public void Load()
        {
            var path = HdknConfig.GetPath("Paths.Plugins");

            foreach (var file in _fileSystem.GetFileSystemInfos(path))
            {
                Load(file.FullName);
            }
        }

        public void Load(string path)
        {
            Logger.Trace("Load(\"{0}\");", path);

            var pluginLoader = (from pl in _pluginLoaders
                                where pl.CanLoad(path)
                                select pl).FirstOrDefault();

            if (pluginLoader == null)
            {
                Logger.Warn("No plugin loader available for path '{0}'.", path);
                return;
            }

            var assemblies = pluginLoader.Load(path);
            // Add common assemblies to list
            foreach (var file in _fileSystem.GetFiles(HdknConfig.WorkingDirectory,
                                                      "Hadouken.Common.**.dll"))
            {
                assemblies.Add(_fileSystem.ReadAllBytes(file));
            }

            var manifest = Sandbox.ReadManifest(assemblies);

            if (manifest == null)
            {
                Logger.Error("Found no plugin manifest.");
                return;
            }

            if (_plugins.ContainsKey(manifest.Name))
            {
                Logger.Error("Plugin {0} already loaded. Ignoring.", manifest.Name);
                return;
            }

            var info = new PluginInfo(manifest.Name, manifest.Version);
            info.Assemblies.AddRange(assemblies);
            info.Manifest = manifest;

            _plugins.Add(info.Name, info);

            var blacklist = _keyValueStore.Get("plugins.blacklist", new string[] { });
            if (blacklist.Contains(manifest.Name))
            {
                Logger.Error("Plugin {0} is blacklisted. Skipping.", manifest.Name);
                return;
            }

            LoadSandbox(info.Name);
        }

        /// <summary>
        /// Loads the sandbox for a plugin present in the _plugin dictionary.
        /// </summary>
        /// <param name="name">The name of the plugin to load the sandbox for.</param>
        internal void LoadSandbox(string name)
        {
            var info = _plugins[name];

            if (info.Sandbox != null)
            {
                Logger.Error("Sandbox already exists for plugin {0}", info.Name);
            }

            try
            {
                Logger.Debug("Creating plugin sandbox");

                var httpUser = _keyValueStore.Get<string>("auth.username");
                var httpPass = _keyValueStore.Get<string>("auth.password");
                var binding = (_environment.HttpBinding.EndsWith("/")
                                   ? _environment.HttpBinding + "api/plugins/" + info.Name
                                   : _environment.HttpBinding + "/api/plugins/" + info.Name);

                var sandbox = Sandbox.CreatePluginSandbox(info.Manifest, info.Assemblies);
                sandbox.Load(new SetupInformation
                    {
                        DatabasePath = Path.Combine(HdknConfig.GetPath("Paths.Data"),
                                                    String.Format("hdkn.plugins.{0}.db", info.Name)),
                        HttpBinding = binding,
                        HttpPassword = httpPass,
                        HttpUsername = httpUser,
                        PluginName = info.Name
                    });
                sandbox.ExtractResources(info.Manifest, _httpServer.RootDirectory);

                info.Sandbox = sandbox;
            }
            catch (Exception e)
            {
                Logger.ErrorException(String.Format("Could not load plugin {0}.", info.Name), e);

                return;
            }

            _plugins[name].State = PluginState.Loaded;
            _messageBus.Publish(new PluginLoadedMessage { Name = info.Name, Version = info.Version });
        }

        public void Unload(string name)
        {
            if (!_plugins.ContainsKey(name))
                return;

            Logger.Info("Unloading plugin {0}", name);

            var sandbox = _plugins[name].Sandbox;

            if (sandbox != null)
            {
                try
                {
                    sandbox.Unload();
                    AppDomain.Unload(sandbox.AppDomain);
                }
                catch (Exception e)
                {
                    Logger.ErrorException("Error when unloading plugin sandbox", e);
                }
            }

            _plugins[name].Sandbox = null;
            _plugins[name].State = PluginState.Unloaded;
        }

        public void UnloadAll()
        {
            foreach (var key in _plugins.Keys)
            {
                Unload(key);
            }
        }
    }
}
