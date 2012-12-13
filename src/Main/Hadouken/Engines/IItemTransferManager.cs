using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Engines
{
    public interface IItemTransferManager
    {
        bool Completed { get; }
        string Name { get; }
        long Size { get; }
        string SavePath { get; }
        string Label { get; }
        double Progress { get; }
        ItemTransferState State { get; }

        long DownloadSpeed { get; }
        long DownloadedBytes { get; }
        long RemainingBytes { get; }

        DateTime AddedDate { get; }
        DateTime? CompletedDate { get; }

        void Start();
        void Stop();
        void Pause();
        void Move(string newLocation);
    }
}
