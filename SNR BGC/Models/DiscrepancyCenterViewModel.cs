using System;

namespace SNR_BGC.Models
{
    public class DiscrepancyCenterViewModel
    {
        public int? reserveId { get; set; }  
        public string? orderId { get; set; }
        public int? boxId { get; set; }

        public string? skuId { get; set; }
        public string? item_description { get; set; }
        public string? item_image { get; set; }
        public string? boxQRCode { get; set; }
        public DateTime created_at { get; set; }
        public string typeOfexception { get; set; }
        public decimal? UPC { get; set; }
        public decimal? item_price { get; set; }

        public string? moduleName { get; set; }
        public string? dateFetch { get; set; }
        public int? clearedTotalItems { get; set; }
        public int? boxTotalItems { get; set; } 
        public int? item_count { get;set; }
    }
}
