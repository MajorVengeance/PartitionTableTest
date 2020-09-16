using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;

namespace PartitionTableTest.Models
{
    public class PokerHand
    {
        public long Id { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string Action { get; set; }
        public double Amount { get; set; }
        public string PlayerName { get; set; }
        public string PokerSiteHandId { get; set; }

    }
}