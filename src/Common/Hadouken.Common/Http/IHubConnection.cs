using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Common.Http
{
    public interface IHubConnection
    {
        void Load();
        void Unload();

        IHubProxy Torrents { get; }
    }
}
