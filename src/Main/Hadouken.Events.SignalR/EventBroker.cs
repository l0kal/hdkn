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

namespace Hadouken.Events.SignalR
{
    internal class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var cfg = new HubConfiguration
                {

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

        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);

        public void Start()
        {
            Task.Factory.StartNew(() =>
                {
                    using (WebApplication.Start<Startup>("url"))
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
