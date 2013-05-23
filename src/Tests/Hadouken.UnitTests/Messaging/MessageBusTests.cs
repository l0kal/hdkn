using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Hadouken.Impl.Messaging;
using Hadouken.Messaging;
using System.Threading;

namespace Hadouken.UnitTests.Messaging
{
    public interface ITestMessage : IMessage
    {
        string Test { get; set; }
    }

    [TestFixture]
    public class MessageBusTests
    {
        [Test]
        public void Can_send_and_receive_messages()
        {
            var mre = new ManualResetEvent(false);

            var mb = new DefaultMessageBus();
            mb.Subscribe<ITestMessage>(msg =>
                {
                    if (msg.Test == "1")
                        mre.Set();
                });
            mb.Send<ITestMessage>(msgBuilder => { msgBuilder.Test = "1"; });

            Assert.IsTrue(mre.WaitOne());
        }
    }
}
