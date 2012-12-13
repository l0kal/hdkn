using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Engines
{
    public interface IItemTransferManager
    {
        string Name { get; }
        long Size { get; }
        string SavePath { get; }
        ItemTransferState State { get; }
    }
}
