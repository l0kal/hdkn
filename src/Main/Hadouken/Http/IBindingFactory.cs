using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Http
{
    public interface IBindingFactory
    {
        string GetBinding(string subPath = null);
    }
}
