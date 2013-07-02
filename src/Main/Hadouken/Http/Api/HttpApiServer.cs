using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.SelfHost;

namespace Hadouken.Http.Api
{
    public class HttpApiServer : IHttpApiServer
    {
        private readonly HttpSelfHostServer _server;

        public HttpApiServer(HttpSelfHostConfiguration configuration)
        {
            _server = new HttpSelfHostServer(configuration);
        }

        public void Start()
        {
            _server.OpenAsync().Wait();
        }

        public void Stop()
        {
            _server.CloseAsync().Wait();
            _server.Dispose();
        }
    }
}
