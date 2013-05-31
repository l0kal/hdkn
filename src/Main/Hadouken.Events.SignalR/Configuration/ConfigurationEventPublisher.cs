using Hadouken.Events.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Hadouken.Events.SignalR.Configuration
{
    [HubName("Configuration")]
    public class ConfigurationEventPublisher : Hub, IConfigurationEventPublisher
    {
        public void PublishConfigChanged(object message)
        {
            Clients.All.ConfigChanged(message);
        }
    }
}
