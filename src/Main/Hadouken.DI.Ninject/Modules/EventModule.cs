using Hadouken.Events.Plugin;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.DI.Ninject.Modules
{
    public class EventModule : NinjectModule
    {
        public override void Load()
        {
            var pep = (from asm in AppDomain.CurrentDomain.GetAssemblies()
                       from type in asm.GetTypes()
                       where typeof (IPluginEventPublisher).IsAssignableFrom(type)
                       where type.IsClass && !type.IsAbstract
                       select type).FirstOrDefault();

            Kernel.Bind(typeof (IPluginEventPublisher), pep).To(pep).InSingletonScope();
        }
    }
}
