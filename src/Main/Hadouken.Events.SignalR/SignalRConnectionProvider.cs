using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Hadouken.Configuration;

namespace Hadouken.Events.SignalR
{
    [Component(ComponentLifestyle.Singleton)]
    public class SignalRConnectionProvider : ISignalRConnectionProvider
    {
        private IHubConnection _connection;

        private IHubConnection Create()
        {
            return new HubConnection("");
        }

        public IHubConnection GetConnection()
        {
            return (_connection ?? (_connection = Create()));
        }
    }
}
