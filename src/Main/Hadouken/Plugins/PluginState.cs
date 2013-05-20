using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Plugins
{
    public enum PluginState
    {
        Unknown = 0,
        Loading,
        Loaded,
        Unloading,
        Unloaded
    }
}
