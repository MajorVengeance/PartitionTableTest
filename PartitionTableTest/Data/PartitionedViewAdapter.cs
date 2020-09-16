using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.UI;

namespace PartitionTableTest.Data
{
    public class PartitionedViewAdapter<T> : DbContext where T: class
    {
        private List<MemberTable> memberTables;
        const string EmptyPartitionSuffix = "_Empty";
        public PartitionedViewConfiguration<T> Config { get; set; }
        public DatabaseAdapter Adapter { get; private set; }
        public MemberTableFactory<T> MemberTableFactory { get; private set; }
        public IDbSet<T> View { get; set; }

        internal PartitionedViewAdapter(PartitionedViewConfiguration<T> config, MemberTableFactory<T> memberTableFactory, DatabaseAdapter adapter) : base(config.ConnectionName)
        {
            Config = config;
            Adapter = adapter;
            MemberTableFactory = memberTableFactory;
            memberTables = GetDataRangeKeys().Select(MemberTableFactory.Create).ToList();

        }
                
        private string PartitionTablePrefix
        {
            get { return Config.DataType.Name; }
        }

        private string ViewName
        {
            get { return string.Concat("View", Config.DataType.Name); }
        }

        public override int SaveChanges()
        {
            var objectsWritten = 0;
            foreach(var o in ChangeTracker.Entries<T>().Where(e => e.State != EntityState.Unchanged))
            {
                var dataRangeKey = GetDataRangeKey(o.Entity);

                // Check to see if the table "exists" in our model
                if (!memberTables.Any(mt => mt.DataRangeKey == dataRangeKey))
                {
                    var newTable = MemberTableFactory.Create(dataRangeKey);
                    newTable.DbContext.Database.Initialize(true);
                    // Add the table
                    this.memberTables.Add(newTable);

                    //Recreate View (I hope)
                    CreateView();
                }

                var memberTable = memberTables.Single(mt => mt.DataRangeKey == dataRangeKey);

                var copy = CloneTo(o.Entity, memberTable.DataType);
                    memberTable.DbContext.Entry(copy).State = o.State;
                    objectsWritten += memberTable.DbContext.SaveChanges();
                    Copy(memberTable.DbContext.Entry(copy).Entity as T, o.Entity);

                    if (o.State == EntityState.Deleted)
                        o.State = EntityState.Detached;
                    else
                        o.State = EntityState.Unchanged;
                
            }

            return objectsWritten;
        }

        private Func<T, Type, object> CloneTo = (T from, Type type) =>
        {
            var to = Activator.CreateInstance(type);
            foreach (PropertyInfo sourcePropertyInfo in from.GetType().GetProperties())
            {
                PropertyInfo destPropertyInfo = type.GetProperty(sourcePropertyInfo.Name);
                destPropertyInfo.SetValue(to, sourcePropertyInfo.GetValue(from, null), null);
            }

            return to;
        };

        public Action<T, T> Copy = (T from, T to) =>
        {
            foreach (PropertyInfo sourcePropertyInfo in from.GetType().GetProperties())
            {
                PropertyInfo destPropertyInfo = from.GetType().GetProperty(sourcePropertyInfo.Name);
                destPropertyInfo.SetValue(to, sourcePropertyInfo.GetValue(from, null), null);
            }
        };

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            foreach(var tableContext in memberTables.Select(mt => mt.DbContext))
            {
                tableContext.Database.Initialize(true);
            }

            CreateView();
            modelBuilder.Entity<T>().ToTable(ViewName).HasKey(Config.PrimaryKeyExpression);
            base.OnModelCreating(modelBuilder);
        }

        private void CreateView()
        {
            var keys = GetDataRangeKeys();
            if(keys.Count() > 1)
            {
                keys = keys.Where(p => p != EmptyPartitionSuffix);
            }

            var memberTableNames = keys.Select(dataRangeKey => PartitionTablePrefix + dataRangeKey);
            Adapter.CreateOrAlterPartitionedView(ViewName, memberTableNames);
        }

        private IEnumerable<string> GetDataRangeKeys()
        {
            return Adapter
                .GetObjectNamesStartingWith(PartitionTablePrefix)
                .Select(p => p.Replace(PartitionTablePrefix, ""))
                .Concat(new[] { EmptyPartitionSuffix })
                .Distinct();
        }

        private string GetDataRangeKey(T o)
        {
            var dataRangeKey = Config.DataRangeKeyExpression.Compile()(o);
            var dataRangeKeyValues = dataRangeKey.GetType().GetProperties().Select(p => p.GetValue(dataRangeKey).ToString());
            return string.Concat(dataRangeKeyValues);
        }
    }
}