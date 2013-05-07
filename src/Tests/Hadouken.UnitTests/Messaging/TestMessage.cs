using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hadouken.Common.Messaging;

namespace Hadouken.UnitTests.Messaging
{
    public class TestMessage : Message
    {
        public string Text { get; set; }
    }
}
