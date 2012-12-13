using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Engines
{
    public abstract class TransferEngine<TManager> : ITransferEngine<TManager> where TManager : IItemTransferManager
    {
        public abstract void Load();

        public abstract void Unload();

        public abstract TManager Add(byte[] data);

        public virtual TManager Add(Uri uri)
        {
            return Add(new byte[] {});
        }

        public virtual void Remove(string id)
        {
            if (Managers.ContainsKey(id))
                Remove(Managers[id]);
        }

        public abstract void Remove(TManager manager);

        public virtual void StartAll()
        {
            var managers = Managers.Values;

            foreach (var manager in managers)
                Start(manager);
        }

        public virtual void StopAll()
        {
            var managers = Managers.Values;

            foreach (var manager in managers)
                Stop(manager);
        }

        public virtual void Start(string id)
        {
            if(Managers.ContainsKey(id))
                Start(Managers[id]);
        }

        public abstract void Start(TManager manager);

        public virtual void Stop(string id)
        {
            if (Managers.ContainsKey(id))
                Stop(Managers[id]);
        }

        public abstract void Stop(TManager manager);

        public virtual void Pause(string id)
        {
            if (Managers.ContainsKey(id))
                Pause(Managers[id]);
        }

        public abstract void Pause(TManager manager);

        public abstract IDictionary<string, TManager> Managers { get; }
    }
}
