using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Hadouken.Common;
using Owin;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;

namespace Hadouken.Http.SignalR
{
    internal class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var cfg = new HubConfiguration
                {
                    EnableCrossDomain = true,
                    EnableDetailedErrors = true,
                    Resolver = new CustomDependencyResolver()
                };

            app.MapHubs("/hubs", cfg);
        }
    }

    [Component(ComponentType.Singleton)]
    public class SignalRHubServer : IHttpHubServer
    {
        private static readonly Microsoft.Owin.Host.HttpListener.OwinHttpListener __dd;

        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);

        public void Start(string url)
        {
            Task.Factory.StartNew(() =>
                {
                    using (WebApplication.Start<Startup>(url))
                    {
                        _resetEvent.WaitOne();
                    }
                });
        }

        public void Stop()
        {
            _resetEvent.Set();
        }
    }
}
