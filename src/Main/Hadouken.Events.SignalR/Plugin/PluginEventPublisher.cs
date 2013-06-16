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
    public class PluginsHub : Hub
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public override System.Threading.Tasks.Task OnConnected()
        {
            Logger.Trace("OnConnected: {0}", Context.ConnectionId);
            Clients.Client(Context.ConnectionId).Ping();

            return base.OnConnected();
        }
    }

    public class PluginEventPublisher : IPluginEventPublisher
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Lazy<IHubContext> _hub = new Lazy<IHubContext>(() => GlobalHost.ConnectionManager.GetHubContext<PluginsHub>()); 

        public void PublishPluginLoading(object message)
        {
            _hub.Value.Clients.All.Loading(message);
        }

        public void PublishPluginLoaded(object message)
        {
            _hub.Value.Clients.All.Loaded(message);
        }
    }
}
