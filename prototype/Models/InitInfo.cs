using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace prototype.Models
{
    public class InitInfo
    {
        public string Id { get; set; }

        public string Mail { get; set; }

        public List<string> Index1 { get; set; }

        public List<string> Index2 { get; set; }

        public List<MortgageInfo> MortgageList { get; set; }

        public List<StockInfo> StockList { get; set; }
    }
}
