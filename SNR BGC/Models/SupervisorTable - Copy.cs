using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class ExceptionItems
    {
        [Key]
        public int id { get; set; }
        public string orderId { get; set; }
        public string sku { get; set; }
        public int qty { get; set; }
        public string user { get; set; }
        public string userType { get; set; }
        public string typeOfException { get; set; }
        public DateTime? dateProcess { get; set; }

    }
   
}
