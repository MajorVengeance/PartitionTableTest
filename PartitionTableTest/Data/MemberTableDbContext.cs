using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Security.Permissions;
using System.Web;

namespace PartitionTableTest.Data
{
    public class MemberTableDbContext<T> : DbContext where T : class
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.RegisterEntityType(DataType);
            modelBuilder.Types()
                .Where(t => t == DataType)
                .Configure(c => c.HasKey(PrimaryKeyPropertyNames));
            base.OnModelCreating(modelBuilder);
        }

        public Type DataType
        {
            get { return typeof(T); }
        }

        public IEnumerable<string> PrimaryKeyPropertyNames { get; private set; }

        public string PartionDataRange { get; private set; }
        public DbContext EmptyContext { get; private set; }

        public MemberTableDbContext(IEnumerable<string> primaryKeyPropertyNames, string connectionString, string suffix) : base(connectionString)
        {
            PartionDataRange = suffix;
            PrimaryKeyPropertyNames = primaryKeyPropertyNames;
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<MemberTableDbContext<T>, Configuration<T>>(true));
        }
    }
}