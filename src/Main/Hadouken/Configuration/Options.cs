using System;

namespace Hadouken.Configuration
{
    [Flags]
    public enum Options : byte
    {
        None = 0,
        Hashed = 1,
    }
}