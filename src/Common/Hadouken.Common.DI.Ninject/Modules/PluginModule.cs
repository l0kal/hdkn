using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hadouken.Common.Plugins;
using Ninject.Modules;

namespace Hadouken.Common.DI.Ninject.Modules
{
    public class PluginModule : NinjectModule
    {
        public override void Load()
        {
            var pluginType = (from asm in AppDomain.CurrentDomain.GetAssemblies()
                              from type in asm.GetTypes()
                              where typeof (Plugin).IsAssignableFrom(type)
                              where type.IsClass && !type.IsAbstract
                              select type).FirstOrDefault();

            if (pluginType != null)
                Kernel.Bind(typeof (Plugin)).To(pluginType);
        }
    }
}
