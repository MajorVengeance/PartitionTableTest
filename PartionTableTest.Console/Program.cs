using PartitionTableTest.Data;
using PartitionTableTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartionTableTest.Console
{
    public class Program
    {
        static void Main(string[] args)
        {
            var config = new PartitionedViewConfiguration<PokerHand>
            {
                DataRangeKeyExpression = ph => new { ph.Year, ph.Month },
                PrimaryKeyExpression = ph => new { ph.Id, ph.Year, ph.Month },
            };

            var adapter = new PartitionedViewAdapterFactory().Create(config);
            adapter.View.Add(new PokerHand
            {
                Action = "Fold",
                Amount = 100.00,
                Id = 1,
                Month = 2,
                PlayerName = "Joe Bloggs",
                PokerSiteHandId = "12",
                Year = 2020
            });
            adapter.SaveChanges();
        }
    }
}
