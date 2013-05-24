using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Common.Http
{
    public interface IHubProxy
    {
        void On(string eventName, Action callback);
        void On<TData>(string eventName, Action<TData> callback);
    }
}
