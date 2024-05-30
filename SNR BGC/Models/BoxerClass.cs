using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class BoxerClass
    {
        public string? skuId { get; set; }
        public string? orderId { get; set; }
        public string? item_description { get; set; }
        public decimal? item_price { get; set; }
        public decimal? UPC { get; set; }
        public string? item_image { get; set; }
        public string? transferLocation { get; set; }
        public string? module { get; set; }
        public string? inventoryLocation { get; set; }
        public int? ScannedQty { get; set; }
        public int? Quantity { get; set; }
        public int? OrdersQuantity { get; set; }
        public int? TotalOrdersQuantity { get; set; }
        public DateTime? boxerStartTime { get; set; }
        public DateTime? boxerEndTime { get; set; }
        
    }
   
}
