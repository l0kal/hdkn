using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hadouken.Http.Api
{
    public interface IHttpApiServer
    {
        void Start();
        void Stop();
    }
}
