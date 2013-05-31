using Hadouken.Events.BitTorrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace Hadouken.Events.SignalR.Torrent
{
    public class TorrentEventListener : ITorrentEventListener
    {
        private readonly HubConnection _connection;
        private readonly IHubProxy _proxy;

        public TorrentEventListener(ISignalRConnectionProvider connectionProvider)
        {
            _connection = (HubConnection) connectionProvider.GetConnection();

            _proxy = _connection.CreateHubProxy("Torrents");
            _proxy.On("Ping", () => { });

            _connection.Start().Wait();
        }

        public void OnAdded(Action callback)
        {
            _proxy.On("TorrentAdded", callback);
        }

        public void OnAdded<T>(Action<T> callback)
        {
            _proxy.On("TorrentAdded", callback);
        }

        public void OnRemoved(Action callback)
        {
            _proxy.On("TorrentRemoved", callback);
        }

        public void OnRemoved<T>(Action<T> callback)
        {
            _proxy.On("TorrentRemoved", callback);
        }

        public void OnError(Action callback)
        {
            _proxy.On("TorrentError", callback);
        }

        public void OnError<T>(Action<T> callback)
        {
            _proxy.On("TorrentError", callback);
        }

        public void OnCompleted(Action callback)
        {
            _proxy.On("TorrentCompleted", callback);
        }

        public void OnCompleted<T>(Action<T> callback)
        {
            _proxy.On("TorrentCompleted", callback);
        }

        public void OnMoved(Action callback)
        {
            _proxy.On("TorrentMoved", callback);
        }

        public void OnMoved<T>(Action<T> callback)
        {
            _proxy.On("TorrentMoved", callback);
        }
    }
}
