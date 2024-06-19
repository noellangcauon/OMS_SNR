using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SNR_BGC.Models
{
    public class ItemUPC
    {
        public decimal SKU { get; set; }
        [Key]
        public decimal UPC { get; set; }
        public DateTime DataAsOf { get; set; }

    }
}
