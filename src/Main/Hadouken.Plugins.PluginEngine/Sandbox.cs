using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Security;
using System.Text.RegularExpressions;
using Hadouken.Common.Plugins;
using NLog;
using System.IO;
using Hadouken.Configuration;

using System.Security.Policy;

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
                    PrivateBinPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase
                };

            var domain = AppDomain.CreateDomain(setup.ApplicationName, null, setup); //, permissions, fullTrustHelper);

            var ps = (PluginSandbox) domain.CreateInstanceFromAndUnwrap(typeof (PluginSandbox).Assembly.Location,
                                                                        typeof (PluginSandbox).FullName,
                                                                        false,
                                                                        BindingFlags.Default,
                                                                        null,
                                                                        new object[] {HdknConfig.WorkingDirectory, assemblies},
                                                                        null,
                                                                        null);
            
            Logger.Debug("Created a new AppDomain named '{0}'", domain.FriendlyName);

            return ps;
        }
    }
}
