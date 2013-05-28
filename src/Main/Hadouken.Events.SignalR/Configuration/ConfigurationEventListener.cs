using Hadouken.Events.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Events.SignalR.Configuration
{
    [Component(ComponentLifestyle.Singleton)]
    public class ConfigurationEventListener : IConfigurationEventListener
    {
        public void OnChanged(Action<string> callback)
        {
        }
    }
}
