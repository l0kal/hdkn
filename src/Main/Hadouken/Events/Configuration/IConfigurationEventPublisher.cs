using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Events.Configuration
{
    public interface IConfigurationEventPublisher : IEventPublisher
    {
        void PublishConfigChanged(object message);
    }
}
