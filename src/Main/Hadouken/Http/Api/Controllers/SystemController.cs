using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Hadouken.Http.Api.Controllers
{
    public class SystemController : ApiController
    {
        public class SystemInfo
        {
            public Version Version { get; set; }
        }

        public SystemInfo Get()
        {
            return new SystemInfo()
                {
                    Version = typeof(Kernel).Assembly.GetName().Version
                };
        }
    }
}
