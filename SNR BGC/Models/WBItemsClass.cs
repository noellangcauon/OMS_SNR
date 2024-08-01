using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SNR_BGC.Models
{
    public class WBItemsClass
    {
        public int boxId { get; set; }
        public int? reserveId { get; set; }
        public string? skuId { get; set; }
        public string? orderId { get; set; }
        public string? module { get; set; }
        public string? processBy { get; set; }
        public DateTime? dateProcess { get; set; }
        public string? boxerStatus { get; set; }
        public string? boxerUser { get; set; }
        public DateTime? boxerStartTime { get; set; }
        public DateTime? boxerEndTime { get; set; }
        public bool? isScanned { get; set; }
        public decimal? UPC { get; set; }
        public string? printerName { get; set; }
        public string? platformStatus { get; set; }
        public string? trackingNo { get; set; }
        public string? package_id { get; set; }
        public string? order_item_id { get; set; }
        public string? item_description { get; set; }


    }
}
