using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Events.BitTorrent
{
    public interface ITorrentEventListener : IEventListener
    {
        void OnAdded(Action<TorrentAdded> callback);
        void OnRemoved(Action<TorrentRemoved> callback);
    }
}
