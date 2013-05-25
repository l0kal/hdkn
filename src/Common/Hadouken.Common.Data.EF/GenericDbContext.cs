using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;

namespace Hadouken.Common.Data.EF
{
    public class GenericDbContext<TModel> : DbContext where TModel : Model, new()
    {
        public GenericDbContext(DbConnection dbConnection) : base(dbConnection, true)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            
            modelBuilder.Entity<TModel>();
        }

        public DbSet<TModel> DbSet { get; set; }
    }
}
