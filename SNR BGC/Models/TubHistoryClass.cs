using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class TubHistoryClass
    {
        public string orderId { get; set; }
        public DateTime? dateProcess { get; set; }
        public string pickerStatus { get; set; }
    }
   
}
