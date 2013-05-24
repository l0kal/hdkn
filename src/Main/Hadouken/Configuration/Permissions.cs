using System;

namespace Hadouken.Configuration
{
    [Flags]
    public enum Permissions : byte
    {
        None = 0,
        Read = 1,
        Write = 2
    }
}