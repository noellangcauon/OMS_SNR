using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SNR_BGC.Models
{
    public class BoxerLogs
    {
        [Key]
        public int id { get; set; }
        public string orderId { get; set; }
        public string logs { get; set; }
        public DateTime? dateProcess { get; set; }
        public string printername { get; set; }
        public string module { get; set; }
        public string response { get; set; }
        public string partnerId { get; set; }
        public string shopId { get; set; }
        public string access_token { get; set; }
        public string partnerKey { get; set; }
        public string filepath { get; set; }
    }
}
