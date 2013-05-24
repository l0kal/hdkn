using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Owin.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Owin;
using Microsoft.AspNet.SignalR;

namespace Hadouken.Common.Http.SignalR
{
    public class SignalRHubConnection : IHubConnection
    {
        public SignalRHubConnection(string url)
        {
        }

        public void Load()
        {
        }

        public void Unload()
        {
        }
    }
}
