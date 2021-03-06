﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Hadouken.Framework
{
    public interface IBootConfig
    {
        string DataPath { get; }

        int Port { get; }

        string HttpVirtualPath { get; }

        string RpcGatewayUri { get; }

        string RpcPluginUri { get; }

        string HostBinding { get; }

        string UserName { get; }
        
        string Password { get; }
    }
}
