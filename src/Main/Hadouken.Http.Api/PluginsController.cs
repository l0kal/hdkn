using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Http;
using Hadouken.Plugins;
using System.Net.Http;

namespace Hadouken.Http.Api
{
    public class PluginsController : ApiController
    {
        private readonly IPluginEngine _pluginEngine;

        public PluginsController(IPluginEngine pluginEngine)
        {
            _pluginEngine = pluginEngine;
        }

        public IEnumerable<object> Get()
        {
            return (from plugin in _pluginEngine.Plugins
                    select new
                        {
                            plugin.Name,
                            plugin.Version,
                            plugin.State
                        });
        } 

        public HttpResponseMessage Put([FromUri] string id, [FromBody] Dictionary<string, object> data)
        {
            foreach (var key in data.Keys)
            {
                switch (key)
                {
                    case "action":
                        var state = data[key].ToString().ToLowerInvariant();

                        switch (state)
                        {
                            case "load":
                                _pluginEngine.Load(id);
                                break;
                            case "unload":
                                _pluginEngine.Unload(id);
                                break;
                        }

                        break;
                }
            }

            using (var response = Request.CreateResponse(HttpStatusCode.NoContent))
            {
                return response;
            }
        }
    }
}
