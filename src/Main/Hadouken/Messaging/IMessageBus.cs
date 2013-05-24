using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hadouken.Messaging
{
    public interface IMessageBus
    {
        Task Publish<TMessage>(TMessage message) where TMessage : class, new();

        void Subscribe<TMessage>(Action<TMessage> callback) where TMessage : class, new();
    }
}
