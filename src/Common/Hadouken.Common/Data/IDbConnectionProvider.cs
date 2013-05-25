using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Hadouken.Common.Data
{
    public interface IDbConnectionProvider
    {
        DbConnection GetConnection(string connectionString);

        string ProviderInvariantName { get; }
    }
}
