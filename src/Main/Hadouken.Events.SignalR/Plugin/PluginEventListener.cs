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
        private readonly HubConnection _connection;
        private readonly IHubProxy _proxy;

        public PluginEventListener(ISignalRConnectionProvider connectionProvider)
        {
            _connection = (HubConnection)connectionProvider.GetConnection();

            _proxy = _connection.CreateHubProxy("Plugins");
            _proxy.On("Ping", () => { });

            _connection.Start().Wait();
        }

        public void OnLoading(Action callback)
        {
            _proxy.On("PluginLoading", callback);
        }

        public void OnLoading<T>(Action<T> callback)
        {
            _proxy.On("PluginLoading", callback);
        }
    }
}
