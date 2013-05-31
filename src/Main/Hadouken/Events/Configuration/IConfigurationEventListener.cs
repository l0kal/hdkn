using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Events.Configuration
{
    public interface IConfigurationEventListener : IEventListener
    {
        void OnChanged(Action callback);
        void OnChanged<T>(Action<T> callback);
    }
}
