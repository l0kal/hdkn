using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hadouken.Events.Plugin;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace Hadouken.Events.SignalR.Plugin
{
    public class PluginEventListener : IPluginEventListener
    {
        private readonly IHubConnection _connection;
        private readonly IHubProxy _proxy;

        public PluginEventListener(ISignalRConnectionProvider connectionProvider)
        {
            _connection = connectionProvider.GetConnection();
            _proxy = new HubProxy(_connection, "Plugin");
            _proxy.On("Ping", () => { });
        }

        public void OnLoaded(Action<PluginLoaded> callback)
        {
            _proxy.On("PluginLoaded", callback);
        }
    }
}
