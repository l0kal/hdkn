using Hadouken.Events.BitTorrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Events.SignalR.Torrent
{
    [Component(ComponentLifestyle.Singleton)]
    public class TorrentEventPublisher : ITorrentEventPublisher
    {
        public void PublishTorrentAdded(BitTorrent.Torrent torrent)
        {
        }

        public void PublishTorrentRemoved(string infoHash)
        {
        }

        public void PublishTorrentError(BitTorrent.Torrent torrent)
        {
        }

        public void PublishTorrentCompleted(BitTorrent.Torrent torrent)
        {
        }

        public void PublishTorrentMoved(BitTorrent.Torrent torrent)
        {
        }
    }
}
