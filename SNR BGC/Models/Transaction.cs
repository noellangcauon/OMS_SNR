using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SNR_BGC.Models
{
    public class Transaction
    {
        [Key]
        public int trn_number { get; set; }
        public string order_number { get; set; }
        public string item_id { get; set; }
        public string item_description { get; set; }
        public DateTime ? trn_date { get; set; }
        public decimal? item_price { get; set; }
        public decimal? trn_grand_total { get; set; }
        public string trn_user { get; set; }
        public string submodule { get; set; }
        public string status { get; set; }

    }
}
