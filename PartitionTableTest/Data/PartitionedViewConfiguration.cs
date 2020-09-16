using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace PartitionTableTest.Data
{
    public class PartitionedViewConfiguration<T>
    {
        public Expression<Func<T, object>> PrimaryKeyExpression { get; set; }

        public Expression<Func<T, object>> DataRangeKeyExpression { get; set; }
        public Type DataType
        {
            get { return typeof(T); }
        }

        public string ConnectionName
        {
            get { return $"name={typeof(T).Name}"; }
        }
        public IEnumerable<string> PrimaryKeyPropertyNames
        {
            get { return GetPropertyNamesFromKeyExpression(PrimaryKeyExpression); }
        }

        private IEnumerable<string> GetPropertyNamesFromKeyExpression(Expression<Func<T, Object>> keyProperty)
        {
            var newExpression = keyProperty.Body as NewExpression;

            if (newExpression != null)
            {
                return newExpression.Arguments.Cast<MemberExpression>().Select(e => e.Member.Name);
            }
            var memberExpression = keyProperty.Body as MemberExpression;
            return new string[] { memberExpression.Member.Name };
        }
    }
}