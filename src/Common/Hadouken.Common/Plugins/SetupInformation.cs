using System;

namespace Hadouken.Common.Plugins
{
    [Serializable]
    public sealed class SetupInformation
    {
        public string DatabasePath { get; set; }

        public string HttpBinding { get; set; }

        public string HttpUsername { get; set; }

        public string HttpPassword { get; set; }

        public string HttpRoot { get; set; }

        public string PluginName { get; set; }
    }
}
