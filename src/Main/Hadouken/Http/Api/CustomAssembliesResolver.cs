using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Dispatcher;

namespace Hadouken.Http.Api
{
    public class CustomAssembliesResolver : IAssembliesResolver
    {
        private readonly ICollection<Assembly> _assemblies;

        public CustomAssembliesResolver(ICollection<Assembly> assemblies)
        {
            _assemblies = assemblies;
        }

        public ICollection<Assembly> GetAssemblies()
        {
            return _assemblies;
        }
    }
}
