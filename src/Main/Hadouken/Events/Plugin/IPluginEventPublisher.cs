using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Events.Plugin
{
    public interface IPluginEventPublisher : IEventPublisher
    {
        void Publish(PluginLoaded message);
    }
}
