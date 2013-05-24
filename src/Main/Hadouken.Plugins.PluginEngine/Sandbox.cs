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
using NLog;
using System.IO;
using Hadouken.Configuration;
using Hadouken.Plugins.PluginEngine.FullTrustHelpers;
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
                    ApplicationBase = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Plugins", manifest.Name),
                    ApplicationName = String.Format("{0}-{1}", manifest.Name, manifest.Version).ToLowerInvariant(),
                    ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile,
                    PrivateBinPath =
                        Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Plugins", manifest.Name)
                };

            var permissions = new PermissionSet(PermissionState.None);

            permissions.AddPermission(new SecurityPermission(PermissionState.Unrestricted));

            // Read assemblies in working directory
            permissions.AddPermission(
                new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery,
                                     HdknConfig.WorkingDirectory));

            permissions.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess));

            // To read application configuration file
            permissions.AddPermission(new ConfigurationPermission(PermissionState.Unrestricted));

            // Web permissions
            permissions.AddPermission(new WebPermission(NetworkAccess.Accept | NetworkAccess.Connect,
                                                        new Regex("http://localhost:8081/.*")));

            permissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.PathDiscovery,
                                                           @"C:\Windows\assembly\GAC_MSIL"));

            var fullTrustHelper = typeof (AssemblyResolver).Assembly.Evidence.GetHostEvidence<StrongName>();

            var domain = AppDomain.CreateDomain(setup.ApplicationName, null, setup, permissions, fullTrustHelper);

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
