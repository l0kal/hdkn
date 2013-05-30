using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Events.BitTorrent
{
    public interface ITorrentEventListener : IEventListener
    {
        void OnAdded(Action callback);
        void OnAdded<T>(Action<T> callback);

        void OnRemoved(Action callback);
        void OnRemoved<T>(Action<T> callback);

        void OnError(Action callback);
        void OnError<T>(Action<T> callback);

        void OnCompleted(Action callback);
        void OnCompleted<T>(Action<T> callback);

        void OnMoved(Action callback);
        void OnMoved<T>(Action<T> callback);
    }
}
