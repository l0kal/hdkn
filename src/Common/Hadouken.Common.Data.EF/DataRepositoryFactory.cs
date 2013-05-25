using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hadouken.Common.Data.EF
{
    [Component(ComponentType.Singleton)]
    public class DataRepositoryFactory : IDataRepositoryFactory
    {
        private readonly IDbConnectionProvider _connectionProvider;
        private IDataRepository _repository;

        public DataRepositoryFactory(IDbConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public IDataRepository Create(string connectionString)
        {
            return (_repository ??
                    (_repository =
                     new EntityFrameworkDataRepository(_connectionProvider.GetConnection(connectionString))));
        }
    }
}
