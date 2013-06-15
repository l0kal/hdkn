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
    public class TorrentsHub : Hub
    {
    }

    public class TorrentEventPublisher : ITorrentEventPublisher
    {
        private static readonly Lazy<IHubContext> HubInstance =
            new Lazy<IHubContext>(() => GlobalHost.ConnectionManager.GetHubContext<TorrentsHub>());
 
        public void PublishTorrentAdded(object message)
        {
            HubInstance.Value.Clients.All.Added(message);
        }

        public void PublishTorrentRemoved(object message)
        {
            HubInstance.Value.Clients.All.Removed(message);
        }

        public void PublishTorrentError(object message)
        {
            HubInstance.Value.Clients.All.Error(message);
        }

        public void PublishTorrentCompleted(object message)
        {
            HubInstance.Value.Clients.All.Completed(message);
        }

        public void PublishTorrentMoved(object message)
        {
            HubInstance.Value.Clients.All.Moved(message);
        }
    }
}
