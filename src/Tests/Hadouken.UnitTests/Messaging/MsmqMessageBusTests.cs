using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hadouken.Common.DI.Ninject;
using NUnit.Framework;
using Hadouken.Common.Messaging.Msmq;
using Hadouken.Common;
using System.Threading;

namespace Hadouken.UnitTests.Messaging
{
    public class MsmqMessageBusTests
    {
        [SetUp]
        public void Setup()
        {
            Kernel.SetResolver(new NinjectDependencyResolver());
        }


        [Test]
        public void Can_send_and_receive()
        {
            var bus = new MsmqMessageBus("msmq://localhost/hdkn_test");
            bus.Load();

            var mre = new ManualResetEvent(false);

            bus.Subscribe<TestMessage>(_ => mre.Set());
            bus.Publish(new TestMessage {Text = "Test message"});

            Assert.IsTrue(mre.WaitOne());
        }
    }
}
