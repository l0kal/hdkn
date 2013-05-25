using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Hadouken.Common.Data.EF
{
    public class EntityFrameworkDataRepository : DbContext, IDataRepository
    {
        public EntityFrameworkDataRepository(DbConnection dbConnection) : base(dbConnection, true)
        {
            Database.SetInitializer<EntityFrameworkDataRepository>(null);
            ConnectionString = dbConnection.ConnectionString;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            this.Configuration.ProxyCreationEnabled = false;
        }

        public string ConnectionString { get; private set; }

        public void Save<TModel>(TModel model) where TModel : Model, new()
        {
            Set<TModel>().Add(model);
        }

        public void SaveOrUpdate<TModel>(TModel model) where TModel : Model, new()
        {
            if (model.Id <= 0)
            {
                Save(model);
            }
            else
            {
                Update(model);
            }
        }

        public void Update<TModel>(TModel model) where TModel : Model, new()
        {
            SaveChanges();
        }

        public void Delete<TModel>(TModel model) where TModel : Model, new()
        {
            Set<TModel>().Remove(model);
        }

        public TModel Single<TModel>(int id) where TModel : Model, new()
        {
            return Set<TModel>().Find(id);
        }

        public TModel Single<TModel>(Expression<Func<TModel, bool>> query) where TModel : Model, new()
        {
            return Set<TModel>().Where(query).SingleOrDefault();
        }

        public IEnumerable<TModel> List<TModel>() where TModel : Model, new()
        {
            return Set<TModel>().ToList();
        }

        public IEnumerable<TModel> List<TModel>(Expression<Func<TModel, bool>> query) where TModel : Model, new()
        {
            return Set<TModel>().Where(query).ToList();
        }
    }
}
