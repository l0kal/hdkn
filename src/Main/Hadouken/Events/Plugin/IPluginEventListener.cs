using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Events.Plugin
{
    public interface IPluginEventListener : IEventListener
    {
        void OnLoading(Action callback);
        void OnLoading(Action<dynamic> callback);
        void OnLoading<T>(Action<T> callback);
    }
}
