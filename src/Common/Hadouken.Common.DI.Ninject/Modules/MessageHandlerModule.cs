﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ninject.Modules;
using Hadouken.Common.Messaging;

namespace Hadouken.Common.DI.Ninject.Modules
{
    public class MessageHandlerModule : NinjectModule
    {
        public override void Load()
        {
            var messageTypes = (from asm in AppDomain.CurrentDomain.GetAssemblies()
                                from type in asm.GetTypes()
                                where typeof (IMessage).IsAssignableFrom(type)
                                where type.IsClass && !type.IsAbstract
                                select type).ToList();

            foreach (var messageType in messageTypes)
            {
                var messageHandlerType = typeof (IMessageHandler<>).MakeGenericType(messageType);

                var implTypes = (from asm in AppDomain.CurrentDomain.GetAssemblies()
                                 from type in asm.GetTypes()
                                 where type.BaseType == messageHandlerType
                                 where type.IsClass && !type.IsAbstract
                                 select type);

                foreach (var impl in implTypes)
                {
                    Kernel.Bind(messageHandlerType).To(impl);
                }
            }
        }
    }
}
