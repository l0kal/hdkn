using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.AspNet.SignalR.Client.Hubs;

namespace Hadouken.Common.Http.HubConnection
{
    public class SignalRHubProxy : IHubProxy
    {
        private readonly Microsoft.AspNet.SignalR.Client.Hubs.IHubProxy _proxy;

        public SignalRHubProxy(Microsoft.AspNet.SignalR.Client.Hubs.IHubProxy proxy)
        {
            _proxy = proxy;
        }

        public void On(string eventName, Action callback)
        {
            _proxy.On(eventName, callback);
        }

        public void On<TData>(string eventName, Action<TData> callback)
        {
            _proxy.On(eventName, callback);
        }
    }
}
