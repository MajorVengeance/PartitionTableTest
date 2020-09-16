﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web;

namespace PartitionTableTest.Data
{
    public class Configuration<T> : DbMigrationsConfiguration<MemberTableDbContext<T>> where T : class
    {
        public Configuration()
        {
            SetSqlGenerator("System.Data.SqlClient", new SqlServerMigrationSqlGenerator());
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }
    }
}