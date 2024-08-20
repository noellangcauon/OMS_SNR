using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class OIDInquiriesClass
    {
        public int entryNum { get; set; }
        public string orderId { get; set; }
        public DateTime? dateFetch { get; set; }
        public string module { get; set; }
        public string oidstatus { get; set; }
        public string tub { get; set; }
        public decimal item_count { get; set; }
        public decimal total_amount { get; set; }
        public DateTime? pickingStartTime { get; set; }
        public DateTime? pickingEndTime { get; set; }
        public DateTime? boxerStartTime { get; set; }
        public DateTime? boxerEndTime { get; set; }
    }

}
