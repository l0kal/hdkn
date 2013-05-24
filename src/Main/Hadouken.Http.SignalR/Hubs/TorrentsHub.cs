using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hadouken.BitTorrent.Messages;
using Microsoft.AspNet.SignalR;
using Hadouken.BitTorrent;
using System.Threading;
using Hadouken.Messaging;

namespace Hadouken.Http.SignalR.Hubs
{
    public class TorrentsHub : Hub
    {
        private readonly IMessageBus _messageBus;

        public TorrentsHub(IMessageBus messageBus)
        {
            _messageBus = messageBus;
            _messageBus.Subscribe<TorrentAddedMessage>(PublishTorrent);
        }

        private void PublishTorrent(TorrentAddedMessage message)
        {
            Clients.All.TorrentAdded(message.Name);
        }
    }
}
