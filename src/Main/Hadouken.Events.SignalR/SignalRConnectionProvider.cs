﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Hadouken.Configuration;
using Hadouken.Http;

namespace Hadouken.Events.SignalR
{
    [Component(ComponentLifestyle.Singleton)]
    public class SignalRConnectionProvider : ISignalRConnectionProvider
    {
        private readonly string _binding;
        private IHubConnection _connection;

        public SignalRConnectionProvider(IBindingFactory bindingFactory)
        {
            _binding = bindingFactory.GetBinding("superduperhub/hubs");
        }

        public IHubConnection GetConnection()
        {
            return new HubConnection(_binding, false);
        }
    }
}
