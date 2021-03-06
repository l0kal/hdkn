﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hadouken.Framework.Rpc
{
    public interface IMethodInvoker
    {
        ParameterInfo[] Parameters { get; }

        object Invoke(params object[] args);
    }
}
