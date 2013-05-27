using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Events.BitTorrent
{
    public interface ITorrentEventPublisher : IEventPublisher
    {
        void Publish(TorrentAdded message);
        void Publish(TorrentRemoved message);
    }
}
