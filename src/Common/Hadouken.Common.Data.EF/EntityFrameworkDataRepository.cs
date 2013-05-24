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
        private readonly DbContext _context;

        public EntityFrameworkDataRepository(string connectionString)
        {
            _context = new DbContext(new SQLiteConnection(connectionString), true);
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; private set; }

        public void Save<TModel>(TModel model) where TModel : Model, new()
        {
            _context.Set<TModel>().Add(model);
            _context.SaveChanges();
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
            _context.SaveChanges();
        }

        public void Delete<TModel>(TModel model) where TModel : Model, new()
        {
            _context.Set<TModel>().Remove(model);
            _context.SaveChanges();
        }

        public TModel Single<TModel>(int id) where TModel : Model, new()
        {
            return _context.Set<TModel>().Find(id);
        }

        public TModel Single<TModel>(Expression<Func<TModel, bool>> query) where TModel : Model, new()
        {
            return _context.Set<TModel>().Where(query).SingleOrDefault();
        }

        public IEnumerable<TModel> List<TModel>() where TModel : Model, new()
        {
            return _context.Set<TModel>().ToList();
        }

        public IEnumerable<TModel> List<TModel>(Expression<Func<TModel, bool>> query) where TModel : Model, new()
        {
            return _context.Set<TModel>().Where(query).ToList();
        }
    }
}
