using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Engines
{
    public abstract class ItemTransferManager : IItemTransferManager
    {
        public abstract string Name { get; }

        public abstract long Size { get; }

        public abstract string SavePath { get; }

        public abstract string Label { get; }

        public abstract ItemTransferState State { get; }
    }
}
