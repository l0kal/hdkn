using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Events.BitTorrent
{
    public interface ITorrentEventPublisher : IEventPublisher
    {
        void PublishTorrentAdded(object message);
        void PublishTorrentRemoved(object message);
        void PublishTorrentError(object message);
        void PublishTorrentCompleted(object message);
        void PublishTorrentMoved(object message);
    }
}
