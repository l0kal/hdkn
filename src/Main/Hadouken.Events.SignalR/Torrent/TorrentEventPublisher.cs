using Hadouken.Events.BitTorrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Hadouken.Events.SignalR.Torrent
{
    [HubName("Torrents")]
    public class TorrentEventPublisher : Hub, ITorrentEventPublisher
    {
        public void PublishTorrentAdded(object message)
        {
            Clients.All.TorrentAdded(message);
        }

        public void PublishTorrentRemoved(object message)
        {
            Clients.All.TorrentRemoved(message);
        }

        public void PublishTorrentError(object message)
        {
            Clients.All.TorrentError(message);
        }

        public void PublishTorrentCompleted(object message)
        {
            Clients.All.TorrentCompleted(message);
        }

        public void PublishTorrentMoved(object message)
        {
            Clients.All.TOrrentMoved(message);
        }
    }
}
