using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace Hadouken.Common.Http.HubConnection
{
    public class SignalRHubConnection : IHubConnection
    {
        private readonly Microsoft.AspNet.SignalR.Client.Hubs.HubConnection _hubConnection;

        public SignalRHubConnection(string url, string username, string password)
        {
            _hubConnection = new Microsoft.AspNet.SignalR.Client.Hubs.HubConnection(
                url,
                new Dictionary<string, string>() {{"usr", username}, {"pwd", password}},
                false);
        }

        public IHubProxy Torrents { get; private set; }

        public void Load()
        {
            Torrents = new SignalRHubProxy(_hubConnection.CreateHubProxy("TorrentsHub"));
            Torrents.On("onPing", () => { });

            _hubConnection.Start().Wait();
        }

        public void Unload()
        {
            _hubConnection.Stop();
        }
    }
}
