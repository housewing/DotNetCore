using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace prototype.Models
{
    public class MortgageInfo
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [DisplayName("縣市")]
        public string City { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [DisplayName("鄉鎮市區")]
        public string Area { get; set; }

        [DisplayName("等級")]
        public string Type { get; set; }

        [DisplayName("價格")]
        public string Price { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [DisplayName("坪數")]
        public string Num { get; set; }
    }
}
