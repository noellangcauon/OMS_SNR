using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SNR_BGC.Models
{
    public class NobNofSkuClass
    {
        [Key]
        public int entryNum { get; set; }
        public string Sku { get; set; }
        public string order_id { get; set; }
        public string module { get; set; }
        public string category { get; set; }
    }
}
