using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Engines
{
    public enum ItemTransferState
    {
        Unknown = 0,
        Hashing,
        Metadata,
        Stopped,
        Stopping,
        Downloading,
        Seeding,
        Error
    }
}
