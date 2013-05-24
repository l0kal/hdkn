using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Http
{
    public interface IHttpHubServer
    {
        void Start(string url);
        void Stop();
    }
}
