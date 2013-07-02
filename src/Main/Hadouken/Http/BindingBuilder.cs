using Hadouken.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hadouken.Http
{
    [Component]
    public class BindingBuilder : IBindingBuilder
    {
        private static readonly int DefaultPort = 8081;
        private static readonly string DefaultBinding = "http://+:{port}/";

        private readonly IRegistryReader _registryReader;

        private string _root;

        public BindingBuilder(IRegistryReader registryReader)
        {
            _registryReader = registryReader;
            LoadRootUri();
        }

        private void LoadRootUri()
        {
            var binding = DefaultBinding;
            var port = _registryReader.ReadInt("webui.port", DefaultPort);

            // Allow overriding from application configuration file.
            if (HdknConfig.ConfigManager.AllKeys.Contains("WebUI.Binding"))
                binding = HdknConfig.ConfigManager["WebUI.Binding"];

            if (HdknConfig.ConfigManager.AllKeys.Contains("WebUI.Port"))
                port = Convert.ToInt32(HdknConfig.ConfigManager["WebUI.Port"]);

            _root = binding.Replace("{port}", port.ToString());

            if (!_root.EndsWith("/"))
                _root = _root + "/";
        }

        public string Build(params string[] subPaths)
        {
            if (subPaths != null && subPaths.Length > 0)
                return String.Concat(_root, String.Join("/", subPaths), "/");

            return _root;
        }
    }
}
