using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Hadouken.Common.Plugins;
using Ninject.Extensions.Conventions;
using Ninject.Modules;

namespace Hadouken.Common.DI.Ninject.Modules
{
    public class PluginModule : NinjectModule
    {
        public override void Load()
        {
            var pluginType = (from asm in AppDomain.CurrentDomain.GetAssemblies()
                              let types = TryGetTypes(asm)
                              where types != null
                              from type in types
                              where typeof (Plugin).IsAssignableFrom(type)
                              where type.IsClass && !type.IsAbstract
                              select type).FirstOrDefault();

            if (pluginType != null)
                Kernel.Bind<Plugin>().To(pluginType);
        }

        private Type[] TryGetTypes(Assembly asm)
        {
            try
            {
                return asm.GetTypes();
            }
            catch
            {
            }

            return null;
        }
    }
}
