using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace prototype.Models
{
    public class StockInfo
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [DisplayName("股號")]
        public string Stock { get; set; }

        [DisplayName("股票名稱")]
        public string Name { get; set; }

        [DisplayName("上市/上櫃")]
        public string StockType { get; set; }

        [DisplayName("股價")]
        public string Price { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [DisplayName("張數")]
        public string Num { get; set; }
    }
}
