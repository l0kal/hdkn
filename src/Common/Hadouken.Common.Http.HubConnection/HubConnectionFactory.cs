using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Common.Http.HubConnection
{
    [Component(ComponentType.Singleton)]
    public class HubConnectionFactory : IHubConnectionFactory
    {
        public IHubConnection Connect(string url)
        {
            return new SignalRHubConnection(url);
        }
    }
}
