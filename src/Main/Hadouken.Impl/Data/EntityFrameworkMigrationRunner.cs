using Hadouken.Common;
using Hadouken.Common.Data;
using Hadouken.Configuration;
using Hadouken.Data;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;

namespace Hadouken.Impl.Data
{
    public class CustomDb : DbContext
    {
    }

    [Component(ComponentType.Singleton)]
    public class EntityFrameworkMigrationRunner : IMigrationRunner
    {
        private readonly IDbConnectionProvider _connectionProvider;

        public EntityFrameworkMigrationRunner(IDbConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public void Up(System.Reflection.Assembly target)
        {
            string dataPath = HdknConfig.GetPath("Paths.Data");
            string connectionString = HdknConfig.ConnectionString.Replace("$Paths.Data$", dataPath);

            var conf = new DbMigrationsConfiguration
                {
                    MigrationsAssembly = target,
                    MigrationsNamespace = "Hadouken.Impl.Data",
                    ContextType = typeof(CustomDb),
                    TargetDatabase = new DbConnectionInfo(connectionString, _connectionProvider.ProviderInvariantName)
                };

            var migrator = new DbMigrator(conf);
            migrator.Update();
        }

        public void Down(System.Reflection.Assembly target)
        {
            throw new NotImplementedException();
        }
    }
}
