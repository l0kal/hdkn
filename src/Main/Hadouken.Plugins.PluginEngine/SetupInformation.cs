using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Hadouken.Plugins.PluginEngine
{
    [Serializable]
    internal sealed class SetupInformation
    {
        public string DatabasePath { get; set; }

        public string HttpBinding { get; set; }

        public string HttpUsername { get; set; }

        public string HttpPassword { get; set; }

        public string HttpRoot { get; set; }

        public string PluginName { get; set; }
    }
}
