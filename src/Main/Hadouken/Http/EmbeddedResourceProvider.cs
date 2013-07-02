using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hadouken.Http
{
    public class EmbeddedResourceProvider : IFileProvider
    {
        private readonly Assembly _targetAssembly;
        private readonly string _rootUrl;
        private readonly string _baseNamespace;

        public EmbeddedResourceProvider(Assembly targetAssembly, string rootUrl, string baseNamespace)
        {
            _targetAssembly = targetAssembly;
            _rootUrl = rootUrl;
            _baseNamespace = baseNamespace;
        }

        public byte[] GetFile(string path)
        {
            if (path.Length >= _rootUrl.Length)
            {
                if (path.Substring(0, _rootUrl.Length) == _rootUrl)
                    path = path.Substring(_rootUrl.Length);
            }

            path = path.Replace("/", ".");
            path = String.Concat(_baseNamespace, ".", path);

            using (var resourceStream = _targetAssembly.GetManifestResourceStream(path))
            {
                if (resourceStream == null)
                    return null;

                using (var ms = new MemoryStream())
                {
                    resourceStream.CopyTo(ms);

                    return ms.ToArray();
                }
            }
        }
    }
}
