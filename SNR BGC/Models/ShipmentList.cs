using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class ShipmentList
    {
        [Key]
        public string orderId { get; set; }
        public string? package_number { get; set; }

    }
   
}
