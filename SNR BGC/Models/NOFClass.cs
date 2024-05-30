using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class NOFClass
    {
        public string sku_id { get; set; }
        public string item_description { get; set; }
        public int Quantity { get; set; }
        public int? onhand { get; set; }
        public DateTime? dateProcess { get; set; }
        public string Remarks { get; set; }
        public string RemarksDesc { get; set; }
    }
    public class NOFViewModel
    {
        public List<NOFClass> NOFClasses { get; set; }
    }
}
