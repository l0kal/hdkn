using Hadouken.Events.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace Hadouken.Events.SignalR.Configuration
{
    public class ConfigurationEventListener : IConfigurationEventListener
    {
        private readonly HubConnection _connection;
        private readonly IHubProxy _proxy;

        public ConfigurationEventListener(ISignalRConnectionProvider connectionProvider)
        {
            _connection = (HubConnection) connectionProvider.GetConnection();

            _proxy = _connection.CreateHubProxy("Configuration");
            _proxy.On("Ping", () => { });

            _connection.Start().Wait();
        }

        public void OnChanged(Action callback)
        {
            _proxy.On("Changed", callback);
        }

        public void OnChanged<T>(Action<T> callback)
        {
            _proxy.On("Changed", callback);
        }
    }
}
