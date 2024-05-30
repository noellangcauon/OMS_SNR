using System;
using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class RunnerClass
    {
        public string sku_id { get; set; }
        public string item_description { get; set; }
        public decimal item_price { get; set; }
        public decimal UPC { get; set; }
        public string departmentDesc { get; set; }
        public string subDepartmentDesc { get; set; }
        public string classDesc { get; set; }
        public string subClassDesc { get; set; }
        public string runnerUser { get; set; }
        public int Quantity { get; set; }
        public string item_image { get; set; }
        public int CollectedQty { get; set; }
        public string inventoryLocation { get; set; }
        public string transferLocation { get; set; }
        public DateTime? collectingStartTime { get; set; }
        public DateTime? collectingEndTime { get; set; }
        public DateTime? transferringStartTime { get; set; }
        public DateTime? transferringEndTime { get; set; }
        public string typeOfexception { get; set; }
        public int onhand { get; set; }
        

    }
}
