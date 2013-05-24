using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Common.Http.HubConnection
{
    [Component(ComponentType.Singleton)]
    public class HubConnectionFactory : IHubConnectionFactory
    {
        public IHubConnection Connect(string url, string username, string password)
        {
            return new SignalRHubConnection(url, username, password);
        }
    }
}
