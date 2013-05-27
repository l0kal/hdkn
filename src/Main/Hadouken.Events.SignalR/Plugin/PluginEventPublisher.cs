using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hadouken.Events.Plugin;
using Microsoft.AspNet.SignalR;

namespace Hadouken.Events.SignalR.Plugin
{
    public class PluginEventPublisher : Hub, IPluginEventPublisher
    {
        public void Publish(PluginLoaded message)
        {
            Clients.Group("authenticated").PluginLoaded(message);
        }
    }
}
