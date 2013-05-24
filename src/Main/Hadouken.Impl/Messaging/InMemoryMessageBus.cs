using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hadouken.Common;
using Hadouken.Messaging;
using Castle.DynamicProxy;
using NLog;

namespace Hadouken.Impl.Messaging
{
    [Component(ComponentType.Singleton)]
    public class InMemoryMessageBus : IMessageBus
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ProxyGenerator _generator = new ProxyGenerator();

        private static class Internal<T> where T : class, new()
        {
            private static readonly List<Action<T>> _subscribers = new List<Action<T>>();

            public static IList<Action<T>> Subscribers { get { return _subscribers; } }
        }

        public Task Publish<TMessage>(TMessage message) where TMessage : class, new()
        {
            return Task.Factory.StartNew(() =>
            {
                foreach (var callback in Internal<TMessage>.Subscribers)
                {
                    try
                    {
                        callback(message);
                    }
                    catch (Exception e)
                    {
                        Logger.ErrorException(String.Format("Could not execute message handler."), e);
                    }
                }
            });
        }

        public void Subscribe<TMessage>(Action<TMessage> callback) where TMessage : class, new()
        {
            if (callback.Method.DeclaringType != null)
                Logger.Trace(String.Format("Adding subscription to message {0} from {1}", typeof(TMessage), callback.Method.DeclaringType.FullName + "." + callback.Method.Name));

            Internal<TMessage>.Subscribers.Add(callback);
        }
    }
}
