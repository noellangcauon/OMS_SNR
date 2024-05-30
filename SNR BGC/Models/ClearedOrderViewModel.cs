using System;

namespace SNR_BGC.Models
{
    public class ClearedOrderViewModel
    {
        public int reserveId { get; set; }
        public decimal? deductedStock2017 { get; set; }
        public decimal? deductedStockEcom { get; set; }
        public string? skuId { get; set; }
        public string? orderId { get; set; }
        public string? module { get; set; }
        public string? processBy { get; set; }
        public DateTime? dateProcess { get; set; }
        public Boolean isFreeItem { get; set; }
        public string? pickerStatus { get; set; }
        public string? pickerUser { get; set; }
        public DateTime? pickingStartTime { get; set; }
        public DateTime? pickingEndTime { get; set; }
        public bool isNIB { get; set; }
        public string? boxQRCode { get; set; }
    }
}
