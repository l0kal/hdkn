using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Security;
using NLog;
using System.IO;

namespace Hadouken.Plugins.PluginEngine
{
    public static class Sandbox
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static PluginManifest ReadManifest(string[] assemblies)
        {
            var domain = AppDomain.CreateDomain("temp");

            var manifestReader =
                (PluginManifestReader)
                domain.CreateInstanceFromAndUnwrap(typeof (PluginManifestReader).Assembly.Location,
                                                   typeof (PluginManifestReader).FullName,
                                                   false,
                                                   BindingFlags.Default,
                                                   null,
                                                   new object[] {assemblies},
                                                   null,
                                                   null);

            var manifest = manifestReader.ReadManifest();

            AppDomain.Unload(domain);

            return manifest;
        }

        internal static PluginSandbox CreatePluginSandbox(PluginManifest manifest, string[] assemblies)
        {
            if (manifest == null)
                throw new ArgumentNullException("manifest");

            if (assemblies == null)
                throw new ArgumentNullException("assemblies");

            var setup = new AppDomainSetup
                {
                    ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                    ApplicationName = String.Format("{0}-{1}", manifest.Name, manifest.Version).ToLowerInvariant(),
                    ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile,
                    PrivateBinPath =
                        Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Plugins", manifest.Name)
                };

            var domain = AppDomain.CreateDomain(setup.ApplicationName, null, setup);


            var ps = (PluginSandbox) domain.CreateInstanceFromAndUnwrap(typeof (PluginSandbox).Assembly.Location,
                                                                        typeof (PluginSandbox).FullName,
                                                                        false,
                                                                        BindingFlags.Default,
                                                                        null,
                                                                        new object[] {assemblies},
                                                                        null,
                                                                        null);
            
            Logger.Debug("Created a new AppDomain named '{0}'", domain.FriendlyName);

            return ps;
        }
    }
}
