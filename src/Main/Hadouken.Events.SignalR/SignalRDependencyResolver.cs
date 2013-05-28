using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.AspNet.SignalR;

namespace Hadouken.Events.SignalR
{
    public class SignalRDependencyResolver : DefaultDependencyResolver
    {
        public override object GetService(Type serviceType)
        {
            var service = Kernel.Resolver.Get(serviceType);
            return (service ?? base.GetService(serviceType));
        }

        public override IEnumerable<object> GetServices(Type serviceType)
        {
            var services = Kernel.Resolver.GetAll(serviceType).ToList();
            return (!services.Any() ? base.GetServices(serviceType) : services);
        }
    }
}
