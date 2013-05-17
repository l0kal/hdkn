﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using Hadouken.Common;
using Hadouken.Common.Plugins;
using Hadouken.Common.Messaging;
using Hadouken.Common.DI;
using Hadouken.Common.IO;
using NLog;
using Hadouken.Common.Http;
using System.IO;
using Hadouken.Common.Data;

namespace Hadouken.Plugins.PluginEngine
{
    public class PluginSandbox : MarshalByRefObject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private Plugin _plugin;

        public PluginSandbox()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        public AppDomain AppDomain
        {
            get { return AppDomain.CurrentDomain; }
        }

        System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return (from asm in AppDomain.CurrentDomain.GetAssemblies()
                    where asm.FullName == args.Name
                    select asm).FirstOrDefault();
        }

        public void AddAssemblies(IEnumerable<byte[]> assemblies)
        {
            foreach (var asm in assemblies)
            {
                AppDomain.CurrentDomain.Load(asm);
            }
        }

        internal void ExtractResources(PluginManifest manifest, string webRoot)
        {
            if (manifest == null)
                throw new ArgumentNullException("manifest");

            if (manifest.Resources == null)
                return;

            var fs = Kernel.Get<IFileSystem>();
            var pluginRoot = "plugins/" + manifest.Name;

            foreach (var key in manifest.Resources.Keys)
            {
                if (String.IsNullOrEmpty(manifest.Resources[key]))
                    continue;

                var resource = manifest.Resources[key];
                var assembly =
                    (from asm in AppDomain.CurrentDomain.GetAssemblies()
                     where asm.GetManifestResourceInfo(resource) != null
                     select asm).FirstOrDefault();

                if (assembly == null)
                    continue;

                var jailed = key.StartsWith("~");
                var path = (jailed ? pluginRoot + key.Substring(1) : key).Split(new[] {"/"},
                                                                                StringSplitOptions.RemoveEmptyEntries)
                                                                         .ToList();
                path.Insert(0, webRoot);

                var fullPath = Path.Combine(path.ToArray());

                if (!fs.DirectoryExists(Path.GetDirectoryName(fullPath)))
                    fs.CreateDirectory(Path.GetDirectoryName(fullPath));

                using (var stream = assembly.GetManifestResourceStream(resource))
                using (var ms = new MemoryStream())
                {
                    if (stream == null)
                        return;

                    stream.CopyTo(ms);

                    fs.WriteAllBytes(fullPath, ms.ToArray());
                }
            }
        }

        internal void Load(SetupInformation setup)
        {
            Logger.Debug("Loading plugin '{0}' in the sandbox.", setup.PluginName);

            var resolverType = (from asm in AppDomain.CurrentDomain.GetAssemblies()
                                from type in asm.GetTypes()
                                where typeof (IDependencyResolver).IsAssignableFrom(type)
                                where type.IsClass && !type.IsAbstract
                                select type).First();

            var resolver = (IDependencyResolver) Activator.CreateInstance(resolverType);
            Kernel.SetResolver(resolver);

            Kernel.BindToFunc(() =>
                {
                    var factory = Kernel.Get<IMessageBusFactory>();
                    return factory.Create("hdkn.plugins." + setup.PluginName.ToLowerInvariant());
                });

            Kernel.BindToFunc(() =>
                {
                    var factory = Kernel.Get<IDataRepositoryFactory>();
                    return factory.Create(String.Format("Data Source={0}; Version=3;", setup.DatabasePath));
                });

            Kernel.BindToFunc(() =>
                {
                    var factory = Kernel.Get<IHttpWebApiServerFactory>();

                    return factory.Create(setup.HttpBinding,
                                          new NetworkCredential(setup.HttpUsername, setup.HttpPassword), 
                                          AppDomain.CurrentDomain.GetAssemblies());
                });

            // Resolve the plugin
            _plugin = Kernel.Get<Plugin>();

            // Get a HTTP server
            var httpServer = Kernel.Get<IHttpWebApiServer>();
            httpServer.Start();

            _plugin.Load();
        }

        public void Unload()
        {
            _plugin.Unload();
        }
    }
}
