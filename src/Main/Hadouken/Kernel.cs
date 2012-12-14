using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hadouken.DI;
using Hadouken.Reflection;
using System.Reflection;

namespace Hadouken
{
    public static class Kernel
    {
        private static IDependencyResolver _resolver;

        public static void Register(params Assembly[] assemblies)
        {
            // get all interface types which are assignable from IComponent (but not IComponent itself)

            var componentTypes = (from asm in assemblies
                                  from type in asm.GetTypes()
                                  where type.HasAttribute<ComponentAttribute>()
                                  where type.IsClass && !type.IsAbstract
                                  select type);

            foreach (var component in componentTypes)
            {
                var componentAttribute = component.GetAttribute<ComponentAttribute>();
                var types = component.GetInterfaces().Union(component.GetAbstractParents());

                foreach (var type in types)
                {
                    if (String.IsNullOrEmpty(componentAttribute.Name))
                    {
                        _resolver.Register(type, component, componentAttribute.Lifestyle);
                    }
                    else
                    {
                        _resolver.Register(type, component, componentAttribute.Lifestyle, componentAttribute.Name);                        
                    }
                }
            }
        }

        public static void SetResolver(IDependencyResolver resolver)
        {
            if(resolver == null)
                throw new ArgumentNullException("resolver");

            _resolver = resolver;
        }

        public static IDependencyResolver Resolver
        {
            get
            {
                if(_resolver == null)
                    throw new ArgumentNullException();

                return _resolver;
            }
        }
    }
}
