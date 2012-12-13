using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Engines
{
    public interface ITransferEngine<TManager> where TManager : IItemTransferManager
    {
        void Load();
        void Unload();

        TManager Add(byte[] data);
        TManager Add(Uri uri);

        void Remove(string id);
        void Remove(TManager manager);

        void StartAll();
        void StopAll();

        void Start(string id);
        void Start(TManager manager);

        void Stop(string id);
        void Stop(TManager manager);

        void Pause(string id);
        void Pause(TManager manager);

        IDictionary<string, TManager> Managers { get; } 
    }
}
