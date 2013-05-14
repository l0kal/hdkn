using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Hadouken.Http
{
    public interface IHttpFileSystemServer
    {
        string RootDirectory { get; }

        void Start();
        void Stop();
    }
}
