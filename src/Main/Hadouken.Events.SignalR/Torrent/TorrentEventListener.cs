using Hadouken.Events.BitTorrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Events.SignalR.Torrent
{
    public class TorrentEventListener : ITorrentEventListener
    {
        public void OnAdded(Action callback)
        {
            throw new NotImplementedException();
        }

        public void OnAdded<T>(Action<T> callback)
        {
            throw new NotImplementedException();
        }

        public void OnRemoved(Action callback)
        {
            throw new NotImplementedException();
        }

        public void OnRemoved<T>(Action<T> callback)
        {
            throw new NotImplementedException();
        }

        public void OnError(Action callback)
        {
            throw new NotImplementedException();
        }

        public void OnError<T>(Action<T> callback)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted(Action callback)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted<T>(Action<T> callback)
        {
            throw new NotImplementedException();
        }

        public void OnMoved(Action callback)
        {
            throw new NotImplementedException();
        }

        public void OnMoved<T>(Action<T> callback)
        {
            throw new NotImplementedException();
        }
    }
}
