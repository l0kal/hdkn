using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hadouken.Http.Api
{
    public interface IHttpApiServerFactory
    {
        IHttpApiServer Create(string binding, params Assembly[] controllerAssemblies);
    }
}
