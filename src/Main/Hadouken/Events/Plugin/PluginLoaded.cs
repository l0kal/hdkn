using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Events.Plugin
{
    public class PluginLoaded
    {
        public string Name { get; set; }

        public Version Version { get; set; }
    }
}
