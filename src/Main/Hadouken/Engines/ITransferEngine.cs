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

        IDictionary<string, TManager> Managers { get; } 
    }
}
