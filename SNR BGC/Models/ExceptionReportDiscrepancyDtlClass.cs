using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class ExceptionReportDiscrepancyDtlClass
    {
        public string skuId { get; set; }
        public string item_image { get; set; }
        public string item_description { get; set; }
        public DateTime? dateProcess { get; set; }
        public decimal item_price { get; set; }
        public decimal total_item_price { get; set; }
    }
   
}
