using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Automapping;
using NHibernate;
using NHibernate.Linq;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NLog;

namespace Hadouken.Common.Data.FluentNHibernate
{
    public class FluentNHibernateDataRepository : IDataRepository
    {
#pragma warning disable 0169
        private static System.Data.SQLite.SQLiteConnection __conn__;
#pragma warning restore 0169

        private readonly object _lock = new object();

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private ISessionFactory _sessionFactory;
        private ISession _session;

        private readonly string _connectionString;

        public FluentNHibernateDataRepository(string connectionString)
        {
            _connectionString = connectionString;

            BuildSessionFactory();
        }

        public string ConnectionString
        {
            get { return _connectionString; }
        }

        private void BuildSessionFactory()
        {
            Logger.Debug("Creating the ISessionFactory with connection string {0}.", _connectionString);

            _sessionFactory = Fluently.Configure()
                                      .Database(SQLiteConfiguration.Standard.ConnectionString(_connectionString))
                                      .Mappings(
                                          m => m.AutoMappings.Add(AutoMap.Assemblies(new CustomAutomappingConfig(),
                                                                                     AppDomain.CurrentDomain.GetAssemblies())
                                                                         .Conventions.Add(new EnumMappingConvention())))
                                      .BuildSessionFactory();

            Logger.Debug("Opening an ISession to use.");

            lock (_lock)
            {
                _session = _sessionFactory.OpenSession();                
            }
        }

        public void Save<TModel>(TModel model) where TModel : Model, new()
        {
            lock (_lock)
            {
                _session.Save(model);
            }
        }

        public void SaveOrUpdate<TModel>(TModel model) where TModel : Model, new()
        {
            lock (_lock)
            {
                _session.SaveOrUpdate(model);
            }
        }

        public void Update<TModel>(TModel model) where TModel : Model, new()
        {
            lock (_lock)
            {
                _session.Update(model);
            }
        }

        public void Delete<TModel>(TModel model) where TModel : Model, new()
        {
            lock (_lock)
            {
                _session.Delete(model);
            }
        }

        public TModel Single<TModel>(int id) where TModel : Model, new()
        {
            lock (_lock)
            {
                return _session.Get<TModel>(id);
            }
        }

        public TModel Single<TModel>(System.Linq.Expressions.Expression<Func<TModel, bool>> query) where TModel : Model, new()
        {
            lock (_lock)
            {
                return _session.Query<TModel>().Where(query).SingleOrDefault();
            }
        }

        public IEnumerable<TModel> List<TModel>() where TModel : Model, new()
        {
            lock (_lock)
            {
                return _session.Query<TModel>().ToList();
            }
        }

        public IEnumerable<TModel> List<TModel>(System.Linq.Expressions.Expression<Func<TModel, bool>> query) where TModel : Model, new()
        {
            lock (_lock)
            {
                return _session.Query<TModel>().Where(query).ToList();
            }
        }
    }
}
