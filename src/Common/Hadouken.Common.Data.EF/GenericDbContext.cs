using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace Hadouken.Common.Data.EF
{
    public class GenericDbContext<TModel> : DbContext where TModel : Model, new()
    {
        public GenericDbContext(string connectionString) : base(new SQLiteConnection(connectionString), true)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            
            modelBuilder.Entity<TModel>();
        }

        public DbSet<TModel> DbSet { get; set; }
    }
}
