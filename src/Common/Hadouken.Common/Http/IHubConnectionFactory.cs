using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Common.Http
{
    public interface IHubConnectionFactory
    {
        IHubConnection Connect(string url, string username, string password);
    }
}
