using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Reflection;

namespace Hadouken.Plugins.PluginEngine
{
    public class PluginManifestReader : MarshalByRefObject
    {
        private static readonly JsonSerializer Serializer = new JsonSerializer();

        static PluginManifestReader()
        {
            Serializer.Converters.Add(new VersionConverter());
        }

        public PluginManifestReader(IEnumerable<string> assemblies)
        {
            foreach (var asm in assemblies)
            {
                Assembly.LoadFile(asm);
            }
        }

        public PluginManifest ReadManifest()
        {
            var pluginAssembly = (from asm in AppDomain.CurrentDomain.GetAssemblies()
                                  from name in asm.GetManifestResourceNames()
                                  where name.EndsWith("manifest.json")
                                  select new
                                      {
                                          Assembly = asm,
                                          Name = name
                                      }).FirstOrDefault();

            if (pluginAssembly == null)
                return null;

            using (var stream = pluginAssembly.Assembly.GetManifestResourceStream(pluginAssembly.Name))
            {
                if (stream == null)
                    throw new Exception();

                using (var reader = new StreamReader(stream))
                {
                    return Serializer.Deserialize<PluginManifest>(new JsonTextReader(reader));
                }
            }
        }
    }
}
