using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web;

namespace PartitionTableTest.Data
{
    public class MemberTableFactory<T>
    {
        public PartitionedViewConfiguration<T> Config { get; private set; }

        public MemberTableFactory(PartitionedViewConfiguration<T> config)
        {
            Config = config;
        }

        public MemberTable Create(string dataRangeKey)
        {
            var memberTable = new MemberTable();
            memberTable.DataRangeKey = dataRangeKey;
            memberTable.DataType = CreatePartitionTableType(Config.DataType, dataRangeKey);
            memberTable.DbContext = CreateTableContext(memberTable.DataType, dataRangeKey);
            return memberTable;
        }

        private DbContext CreateTableContext(Type type, string dataRangeKey)
        {
            var contextType = typeof(MemberTableDbContext<>).MakeGenericType(type);
            var context = (DbContext)Activator.CreateInstance(contextType, Config.PrimaryKeyPropertyNames, Config.ConnectionName, dataRangeKey);
            return context;
        }

        private Type CreatePartitionTableType(Type partionedViewType, string suffix)
        {
            var asm = new AssemblyName(string.Concat(partionedViewType.Name, "PartionedMemberTables"));
            var asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asm, System.Reflection.Emit.AssemblyBuilderAccess.Run);
            var moduleBuilder = asmBuilder.DefineDynamicModule("MemberTables");
            var typeName = string.Concat(partionedViewType.Name, suffix);
            var typeBuilder = moduleBuilder.DefineType(typeName);
            typeBuilder.SetParent(partionedViewType);
            return typeBuilder.CreateType();
        }
    }
}