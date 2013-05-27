using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Events.BitTorrent
{
    public interface ITorrentEventPublisher : IEventPublisher
    {
        void PublishTorrentAdded(Torrent torrent);
        void PublishTorrentRemoved(string infoHash);
        void PublishTorrentError(Torrent torrent);
        void PublishTorrentCompleted(Torrent torrent);
        void PublishTorrentMoved(Torrent torrent);
    }
}
