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
            var adapterFactory = new PartitionedViewAdapterFactory();
            var adapter = adapterFactory.Create(config);

            adapter.View.Add(new PokerHand
            {
                Action = "Fold",
                Amount = 2000.00,
                Id = 2,
                Month = 10,
                PlayerName = "Kevin Smith",
                PokerSiteHandId = "142",
                Year = 2019
            });
            adapter.SaveChanges();

            // Recreate the adapter (view) so we get the new DB
            adapter = adapterFactory.Create(config);

            var viewList = adapter.View.ToList();

            viewList.ForEach(a => System.Console.WriteLine($"PlayerName: {a.PlayerName}; Action: {a.Action}; Id: {a.Id}; Year: {a.Year}; Month: {a.Month}; Amount: {a.Month}"));
            System.Console.Read();
        }
    }
}
