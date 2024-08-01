using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class FailedHandoverClass
    {
        public string orderId { get; set; }
        public DateTime? boxerEndTime { get; set; }
    }
   
}
