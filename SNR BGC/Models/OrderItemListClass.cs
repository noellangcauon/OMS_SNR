using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SNR_BGC.Models
{
    public class OrderItemListClass
    {
        public string? order_item_id { get; set; }
        public string? msg { get; set; }
        public string? item_err_code { get; set; }
        public string? tracking_number { get; set; }
        public string? shipment_provider { get; set; }
        public string? package_id { get; set; }
        public bool retry { get; set; }
    }
}
