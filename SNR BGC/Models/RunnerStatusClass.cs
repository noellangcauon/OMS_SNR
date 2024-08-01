using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class RunnerStatusClass
    {
        public string IsTooLong { get; set; }
        public string orderId { get; set; }
        public string module { get; set; }
        public string sku_id { get; set; }
        public string typeOfexception { get; set; }
        public string item_description { get; set; }
        public DateTime? collectingStartTime { get; set; }
        public string runnerUser { get; set; }
        public string runnerStatus { get; set; }
    }
   
}
