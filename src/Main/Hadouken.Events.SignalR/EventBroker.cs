using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Owin;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Host.HttpListener;
using Hadouken.Http;

namespace Hadouken.Events.SignalR
{
    internal class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var cfg = new HubConfiguration
                {
                    EnableCrossDomain = false,
                    EnableDetailedErrors = true,
                    EnableJavaScriptProxies = true,
                    Resolver = new SignalRDependencyResolver()
                };

            app.MapHubs("/hubs", cfg);
        }
    }

    [Component(ComponentLifestyle.Singleton)]
    public class EventBroker : IEventBroker
    {
#pragma warning disable 0169
        private static readonly Microsoft.Owin.Host.HttpListener.OwinHttpListener __owl;
#pragma warning restore 0169

        private readonly string _binding;
        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);

        public EventBroker(IBindingFactory bindingFactory)
        {
            _binding = bindingFactory.GetBinding("superduperhub");
        }

        public void Start()
        {
            Task.Factory.StartNew(() =>
                {
                    using (WebApplication.Start<Startup>(_binding))
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
