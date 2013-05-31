using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using Hadouken.Events;

namespace Hadouken.DI.Ninject.Modules
{
    public class EventListenerModule : NinjectModule
    {
        public override void Load()
        {
            var types = (from asm in AppDomain.CurrentDomain.GetAssemblies()
                         from type in asm.GetTypes()
                         where typeof (IEventListener).IsAssignableFrom(type)
                         where type.IsClass && !type.IsAbstract
                         select type);

            foreach (var type in types)
            {
                var registerTypes = type.GetInterfaces().ToList();
                registerTypes.Add(type);

                Kernel.Bind(registerTypes.ToArray()).To(type).InSingletonScope();
            }
        }
    }
}
