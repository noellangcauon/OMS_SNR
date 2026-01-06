using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class NIBClass
    {
        public string sku_id { get; set; }
        public string item_description { get; set; }
        public int Quantity { get; set; }
        public int? onhand { get; set; }
        public int? onhand_basement { get; set; }
        public DateTime? dateProcess { get; set; }
    }
    public class NIBViewModel
    {
        public List<NIBClass> NIBClasses { get; set; }
    }
}
