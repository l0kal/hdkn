using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Common.Data
{
    public interface IDataRepositoryFactory
    {
        IDataRepository Create(string connectionString);
    }
}
