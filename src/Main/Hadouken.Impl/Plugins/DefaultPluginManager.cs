using System;
using System.Collections.Generic;
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
using System.IO;

namespace Hadouken.Impl.Plugins
{
    public class DefaultPluginManager : IPluginManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly IPlugin _instance;
        private readonly PluginAttribute _attribute;

        internal DefaultPluginManager(IPlugin plugin)
        {
            _instance = plugin;
            _attribute = plugin.GetType().GetAttribute<PluginAttribute>();
        }

        public string Name
        {
            get { return _attribute.Name; }
        }

        public Version Version
        {
            get { return _attribute.Version; }
        }

        public byte[] GetResource(string name)
        {
            var resource = _attribute.ResourceBase + "." + name;

            using (var stream = _instance.GetType().Assembly.GetManifestResourceStream(resource))
            {
                if (stream != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        return ms.ToArray();
                    }
                }

                return null;
            }
        }

        public void Load()
        {
            _instance.Load();
        }

        public void Unload()
        {
            _instance.Unload();
        }
    }
}
