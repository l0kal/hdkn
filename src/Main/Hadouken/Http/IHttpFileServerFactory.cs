using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hadouken.Http
{
    public interface IHttpFileServerFactory
    {
        IHttpFileServer Create(string binding, IFileProvider fileProvider);
    }
}
