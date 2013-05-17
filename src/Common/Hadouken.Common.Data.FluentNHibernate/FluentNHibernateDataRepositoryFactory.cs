using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hadouken.Common.Data.FluentNHibernate
{
    [Component(ComponentType.Singleton)]
    public class FluentNHibernateDataRepositoryFactory : IDataRepositoryFactory
    {
        private IDataRepository _repository;

        public IDataRepository Create(string connectionString)
        {
            return _repository ?? (_repository = new FluentNHibernateDataRepository(connectionString));
        }
    }
}
