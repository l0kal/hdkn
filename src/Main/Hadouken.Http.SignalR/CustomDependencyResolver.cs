using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hadouken.Common;
using Microsoft.AspNet.SignalR;
using System.Diagnostics;

namespace Hadouken.Http.SignalR
{
    public class CustomDependencyResolver : DefaultDependencyResolver
    {
        public override object GetService(Type serviceType)
        {
            var service = Kernel.Get(serviceType);

            return service ?? base.GetService(serviceType);
        }

        public override IEnumerable<object> GetServices(Type serviceType)
        {
            var services = Kernel.GetAll(serviceType).ToList();

            return (!services.Any() ? base.GetServices(serviceType) : services);
        }
    }
}
