using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;
using FirebirdSql.Data.FirebirdClient;

namespace Hadouken.Common.Data.Firebird
{
    [Component(ComponentType.Singleton)]
    public class FirebirdConnectionProvider : IDbConnectionProvider
    {
        private DbConnection _connection;

        public DbConnection GetConnection(string connectionString)
        {
            return (_connection ?? (_connection = new SqlCeConnection(connectionString)));
        }

        public string ProviderInvariantName { get { return "System.Data.SqlServerCe.4.0"; } }
    }
}
