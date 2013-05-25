using System;
using System.Collections.Generic;

namespace Hadouken.Common.Plugins
{
    [Serializable]
    public class PluginManifest
    {
        public string Name { get; set; }

        public Version Version { get; set; }

        public Dictionary<string,string> Resources { get; set; } 
    }
}
