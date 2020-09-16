using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace PartitionTableTest.Data
{
    public class MemberTable
    {
        public DbContext DbContext { get; set; }
        public string DataRangeKey { get; set; }
        public Type DataType { get; set; }
    }
}