using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace Hadouken.Events.SignalR
{
    public interface ISignalRConnectionProvider
    {
        IHubConnection GetConnection();
    }
}
