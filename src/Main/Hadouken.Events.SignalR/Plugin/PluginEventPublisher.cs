using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hadouken.Events.Plugin;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using NLog;
using Hadouken.Configuration;

namespace Hadouken.Events.SignalR.Plugin
{
    [HubName("Plugins")]
    public class PluginEventPublisher : Hub, IPluginEventPublisher
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void PublishPluginLoading(object message)
        {
            Clients.All.Loading(message);
        }

        public override System.Threading.Tasks.Task OnConnected()
        {
            Logger.Trace("Connection: " + Context.ConnectionId);
            return base.OnConnected();
        }
    }
}
