using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentAttribute : Attribute
    {
        public ComponentAttribute()
        {
            Lifestyle = ComponentLifestyle.Singleton;
        }

        public ComponentAttribute(string name, ComponentLifestyle lifestyle)
        {
            Name = name;
            Lifestyle = lifestyle;
        }

        public ComponentLifestyle Lifestyle { get; private set; }

        public string Name { get; private set; }
    }
}
