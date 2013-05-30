using Hadouken.Events.BitTorrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Events.SignalR.Torrent
{
    public class TorrentEventPublisher : ITorrentEventPublisher
    {
        public void PublishTorrentAdded(object message)
        {
            throw new NotImplementedException();
        }

        public void PublishTorrentRemoved(object message)
        {
            throw new NotImplementedException();
        }

        public void PublishTorrentError(object message)
        {
            throw new NotImplementedException();
        }

        public void PublishTorrentCompleted(object message)
        {
            throw new NotImplementedException();
        }

        public void PublishTorrentMoved(object message)
        {
            throw new NotImplementedException();
        }
    }
}
