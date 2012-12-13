using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Engines
{
    public abstract class TransferEngine<TManager> : ITransferEngine<TManager> where TManager : IItemTransferManager
    {
        private readonly IDictionary<string, TManager> _managers = new Dictionary<string, TManager>(StringComparer.InvariantCultureIgnoreCase); 

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
                manager.Start();
        }

        public virtual void StopAll()
        {
            var managers = Managers.Values;

            foreach (var manager in managers)
                manager.Stop();
        }

        public virtual IDictionary<string, TManager> Managers
        {
            get { return _managers; }
        }
    }
}
