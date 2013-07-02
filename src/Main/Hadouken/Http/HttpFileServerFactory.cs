using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hadouken.Http
{
    [Component]
    public class HttpFileServerFactory : IHttpFileServerFactory
    {
        public IHttpFileServer Create(string binding, IFileProvider fileProvider)
        {
            if (binding == null)
                throw new ArgumentNullException("binding");

            if (fileProvider == null)
                throw new ArgumentNullException("fileProvider");

            return new HttpFileServer(binding, fileProvider);
        }
    }
}
