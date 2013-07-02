using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Hadouken.Plugins;
using Hadouken.Data.Models;
using Hadouken.Data;
using Hadouken.Reflection;
using System.Reflection;
using NLog;
using Hadouken.Messaging;
using Hadouken.Messages;
using Hadouken.Http.Api;
using Hadouken.Http;

namespace Hadouken.Impl.Plugins
{
    public class DefaultPluginManager : IPluginManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly IPlugin _instance;
        private readonly IHttpApiServer _apiServer;
        private readonly IHttpFileServer _fileServer;

        private readonly PluginAttribute _attribute;

        internal DefaultPluginManager(IPlugin plugin, IHttpApiServerFactory apiServerFactory,
                                      IHttpFileServerFactory fileServerFactory, IBindingBuilder bindingBuilder)
        {
            _instance = plugin;
            _attribute = plugin.GetType().GetAttribute<PluginAttribute>();

            _apiServer = apiServerFactory.Create(bindingBuilder.Build("api", "plugins", Name),
                                                 plugin.GetType().Assembly);

            _fileServer = fileServerFactory.Create(bindingBuilder.Build("plugins", Name),
                                                   new EmbeddedResourceProvider(plugin.GetType().Assembly,
                                                                                "/plugins/" + Name + "/",
                                                                                _attribute.ResourceBase));
        }

        public string Name
        {
            get { return _attribute.Name; }
        }

        public Version Version
        {
            get { return _attribute.Version; }
        }

        public void Load()
        {
            _instance.Load();

            _apiServer.Start();
            _fileServer.Start();
        }

        public void Unload()
        {
            _fileServer.Start();
            _apiServer.Stop();

            _instance.Unload();
        }
    }
}
