using Hadouken.Events.BitTorrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Events.SignalR.Torrent
{
    [Component(ComponentLifestyle.Singleton)]
    public class TorrentEventListener : ITorrentEventListener
    {
        public void OnAdded(Action<BitTorrent.Torrent> callback)
        {
        }

        public void OnRemoved(Action<string> callback)
        {
        }

        public void OnError(Action<BitTorrent.Torrent> callback)
        {
        }

        public void OnCompleted(Action<BitTorrent.Torrent> callback)
        {
        }

        public void OnMoved(Action<BitTorrent.Torrent> callback)
        {
        }
    }
}
