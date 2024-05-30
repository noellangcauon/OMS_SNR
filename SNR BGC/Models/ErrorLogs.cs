using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class ErrorLogs
    {
        [Key]
        public int id { get; set; }
        public string orderId { get; set; }
        public string Logs { get; set; }
        public DateTime? date { get; set; }
    }
    
}
