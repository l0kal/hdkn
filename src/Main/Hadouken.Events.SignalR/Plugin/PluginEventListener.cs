using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hadouken.Events.Plugin;
using Microsoft.AspNet.SignalR.Client.Hubs;
using NLog;

namespace Hadouken.Events.SignalR.Plugin
{
    public class PluginEventListener : IPluginEventListener
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly HubConnection _connection;
        private readonly IHubProxy _proxy;

        public PluginEventListener(ISignalRConnectionProvider connectionProvider)
        {
            Logger.Trace("ctor::PluginEventListener()");

            _connection = (HubConnection)connectionProvider.GetConnection();

            _proxy = _connection.CreateHubProxy("Plugins");
            _proxy.On("Ping", () => Logger.Trace("Pong"));

            _connection.Start().Wait();
        }

        public void OnLoading(Action callback)
        {
            _proxy.On("Loading", callback);
        }

        public void OnLoading<T>(Action<T> callback)
        {
            _proxy.On("Loading", callback);
        }
    }
}
