using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hadouken.BitTorrent.Messages;
using Microsoft.AspNet.SignalR;
using Hadouken.BitTorrent;
using System.Threading;
using Hadouken.Messaging;
using Hadouken.Configuration;

namespace Hadouken.Http.SignalR.Hubs
{
    public class TorrentsHub : Hub
    {
        private readonly IMessageBus _messageBus;
        private readonly IKeyValueStore _keyValueStore;

        public TorrentsHub(IMessageBus messageBus, IKeyValueStore keyValueStore)
        {
            _messageBus = messageBus;
            _keyValueStore = keyValueStore;

            _messageBus.Subscribe<TorrentAddedMessage>(PublishTorrent);
        }

        public override System.Threading.Tasks.Task OnConnected()
        {
            var usr = _keyValueStore.Get<string>("auth.username");
            var pwd = _keyValueStore.Get<string>("auth.password");

            if (usr.Equals(Context.QueryString["usr"]) && pwd.Equals(Context.QueryString["pwd"]))
            {
                Groups.Add(Context.ConnectionId, "authenticated");
            }

            return base.OnConnected();
        }

        private void PublishTorrent(TorrentAddedMessage message)
        {
            Clients.Group("authenticated").TorrentAdded(message.Name);
        }
    }
}
