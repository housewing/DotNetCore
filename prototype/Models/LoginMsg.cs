using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace prototype.Models
{
    public class LoginMsg
    {
        [Required(ErrorMessage = "This field is required.")]
        [DisplayName("公司統編")]
        public string Id { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        [EmailAddress(ErrorMessage = "Email format is wrong.")]
        [DisplayName("電子郵件")]
        public string Mail { get; set; }


        public List<string> Index1List { get; set; }

        public List<string> Index2List { get; set; }

        public List<MortgageInfo> MortgageList { get; set; }

        public List<StockInfo> StockList { get; set; }
    }
}
