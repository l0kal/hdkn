using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Engines
{
    public abstract class ItemTransferManager : IItemTransferManager
    {
        public abstract bool Completed { get; }
        public abstract string Name { get; }
        public abstract long Size { get; }
        public abstract string SavePath { get; }
        public abstract double Progress { get; }
        public abstract string Label { get; }
        public abstract ItemTransferState State { get; }

        public abstract long DownloadSpeed { get; }
        public abstract long DownloadedBytes { get; }
        public abstract long RemainingBytes { get; }

        public abstract DateTime AddedDate { get; }
        public abstract DateTime? CompletedDate { get; }

        public abstract void Start();

        public abstract void Stop();

        public abstract void Pause();

        public abstract void Move(string newLocation);
    }
}
