using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.SelfHost;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Hadouken.Http.Api
{
    [Component]
    public class HttpApiServerFactory : IHttpApiServerFactory
    {
        public IHttpApiServer Create(string binding, params Assembly[] controllerAssemblies)
        {
            var config = new HttpSelfHostConfiguration(binding);

            // Set dependency resolver
            config.DependencyResolver = new WebApiDependencyResolver();

            // Set basic auth
            config.ClientCredentialType = HttpClientCredentialType.Basic;
            config.UserNamePasswordValidator = new IdentityValidator();

            // Replace the AssembliesResolver service
            config.Services.Replace(typeof (IAssembliesResolver), new CustomAssembliesResolver(controllerAssemblies));

            // Setup formatter
            var formatter = new JsonMediaTypeFormatter();
            formatter.SerializerSettings.Converters.Add(new VersionConverter());
            formatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            config.Formatters.Clear();
            config.Formatters.Add(formatter);

            // Map routes
            config.Routes.MapHttpRoute(
                "API Action route",
                "{controller}/{id}/{action}"
            );

            config.Routes.MapHttpRoute(
                "API Default",
                "{controller}/{id}",
                new { controller = "System", id = RouteParameter.Optional }
            );

            return new HttpApiServer(config);
        }
    }
}
