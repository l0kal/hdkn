using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hadouken.Http;
using Hadouken.Configuration;
using System.Globalization;

namespace Hadouken.Impl.Http
{
    [Component(ComponentLifestyle.Singleton)]
    public class BindingFactory : IBindingFactory
    {
        private static readonly int DefaultPort = 8081;
        private static readonly string DefaultBinding = "http://localhost:{port}/";

        private readonly IRegistryReader _registryReader;

        public BindingFactory(IRegistryReader registryReader)
        {
            _registryReader = registryReader;
        }

        public string GetBinding(string subPath = null)
        {
            var binding = _registryReader.ReadString("webui.binding", DefaultBinding);
            var port = _registryReader.ReadInt("webui.port", DefaultPort);

            // Allow overriding from application configuration file.
            if (HdknConfig.ConfigManager.AllKeys.Contains("WebUI.Binding"))
                binding = HdknConfig.ConfigManager["WebUI.Binding"];

            if (HdknConfig.ConfigManager.AllKeys.Contains("WebUI.Port"))
                port = Convert.ToInt32(HdknConfig.ConfigManager["WebUI.Port"]);

            // Append subPath if it exists
            if (!String.IsNullOrEmpty(subPath))
            {
                if (binding.EndsWith("/"))
                {
                    binding = binding + subPath;
                }
                else
                {
                    binding = binding + "/" + subPath;
                }
            }

            // Make sure the binding ends with an '/'
            if (!binding.EndsWith("/"))
                binding = binding + "/";

            return binding.Replace("{port}", port.ToString(CultureInfo.InvariantCulture));
        }
    }
}
