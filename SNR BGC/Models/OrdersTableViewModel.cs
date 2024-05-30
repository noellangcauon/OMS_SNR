using System;

namespace SNR_BGC.Models
{
    public class OrdersTableViewModel
    {
        public int entryNum { get; set; }
        public string? orderId { get; set; }
        public string? order_item_id { get; set; }

        public DateTime? created_at { get; set; }
        public string? sku_id { get; set; }
        public decimal? item_price { get; set; }
        public decimal? pos_price { get; set; }
        public decimal? total_item_price { get; set; }
        public string? item_description { get; set; }
        public decimal? item_quantity { get; set; }
        public string? warehouseQuantity { get; set; }
        public DateTime? dateProcess { get; set; }
        public string? clubID { get; set; }
        public string? customerID { get; set; }
        public string? item_image { get; set; }
        public int? item_count { get; set; }

        public string? module { get; set; }
        public int? exception { get; set; }
        public string? typeOfexception { get; set; }
        public decimal? UPC { get; set; }
        public string? runnerUser { get; set; }
        public string? runnerStatus { get; set; }
        public string? transferLocation { get; set; }
        public DateTime? collectingStartTime { get; set; }
        public DateTime? collectingEndTime { get; set; }
        public DateTime? transferringStartTime { get; set; }
        public DateTime? transferringEndTime { get; set; }
        public string? platform_status { get; set; }
    }
}
