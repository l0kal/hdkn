using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SQLite;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Hadouken.Common.Data.EF
{
    public class EntityFrameworkDataRepository : IDataRepository
    {
        private static string c;

        private static class Internal<TModel> where TModel : Model, new()
        {
            private static readonly GenericDbContext<TModel> DbContext = new GenericDbContext<TModel>(c);

            public static GenericDbContext<TModel> Context
            {
                get { return DbContext; }
            }
        }

        public EntityFrameworkDataRepository(string connectionString)
        {
            c = connectionString;
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; private set; }

        public void Save<TModel>(TModel model) where TModel : Model, new()
        {
            Internal<TModel>.Context.DbSet.Add(model);
            Internal<TModel>.Context.SaveChanges();
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
            Internal<TModel>.Context.SaveChanges();
        }

        public void Delete<TModel>(TModel model) where TModel : Model, new()
        {
            Internal<TModel>.Context.DbSet.Remove(model);
            Internal<TModel>.Context.SaveChanges();
        }

        public TModel Single<TModel>(int id) where TModel : Model, new()
        {
            return Internal<TModel>.Context.DbSet.Find(id);
        }

        public TModel Single<TModel>(Expression<Func<TModel, bool>> query) where TModel : Model, new()
        {
            return Internal<TModel>.Context.DbSet.Where(query).SingleOrDefault();
        }

        public IEnumerable<TModel> List<TModel>() where TModel : Model, new()
        {
            return Internal<TModel>.Context.DbSet.ToList();
        }

        public IEnumerable<TModel> List<TModel>(Expression<Func<TModel, bool>> query) where TModel : Model, new()
        {
            return Internal<TModel>.Context.DbSet.Where(query).ToList();
        }
    }
}
