using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class PickerClass
    {
        public string skuId { get; set; }
        public string orderId { get; set; }
        public string item_description { get; set; }
        public decimal item_price { get; set; }
        public decimal UPC { get; set; }
        public string item_image { get; set; }
        public string transferLocation { get; set; }
        public string module { get; set; }
        public string inventoryLocation { get; set; }
        public int Quantity { get; set; }
        public int PickedQty { get; set; }   
        public DateTime? pickingStartTime { get; set; }
        public DateTime? pickingEndTime { get; set; }
        public bool isFromNIB { get; set; }
        public int hasPicked { get; set; }


    }
    public partial class PickerOrderNumbers
    {
        public List<OrderNumberList> List { get; set; }
    }
    public partial class OrderNumberList
    {
        public string orderId { get; set; }
        public string status { get; set; }
    }

    public class PickerOrderQr
    {
        public List<OrderNumberQr> qrList { get; set; }
    }
    public class OrderNumberQr
    {
        public string orderId { get; set; }
        public string qrCode { get; set; }
    }
}
