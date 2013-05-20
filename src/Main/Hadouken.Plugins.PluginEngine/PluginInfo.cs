using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Hadouken.Plugins.PluginEngine
{
    public sealed class PluginInfo : IPluginInfo
    {
        private readonly PluginManifest _manifest;
        private readonly IEnumerable<byte[]> _assemblies;

        private AppDomain _appDomain;

        public PluginInfo(PluginManifest manifest, IEnumerable<byte[]> assemblies)
        {
            _assemblies = assemblies.ToList();
            _manifest = manifest;

            State = PluginState.Unloaded;
        }

        public string Name { get { return _manifest.Name; } }

        public Version Version { get { return _manifest.Version; } }

        public PluginState State { get; private set; }

        internal void Load(SetupInformation setupInformation)
        {
            if (State != PluginState.Unloaded)
                return;

            if (_appDomain != null)
                return;

            var sandbox = Sandbox.CreatePluginSandbox(_manifest, _assemblies);
            sandbox.Load(setupInformation);
            sandbox.ExtractResources(_manifest, setupInformation.HttpRoot);

            _appDomain = sandbox.GetAppDomain();

            State = PluginState.Loaded;
        }

        internal void Unload()
        {
            AppDomain.Unload(_appDomain);
            _appDomain = null;

            State = PluginState.Unloaded;
        }
    }
}
