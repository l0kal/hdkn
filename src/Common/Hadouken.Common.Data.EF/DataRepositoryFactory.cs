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
        private IDataRepository _repository;

        public IDataRepository Create(string connectionString)
        {
            return (_repository ?? (_repository = new EntityFrameworkDataRepository(connectionString)));
        }
    }
}
