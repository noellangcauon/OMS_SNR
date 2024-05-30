using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class ReprintWaybillTable
    {
        [Key]
        public int id { get; set; }
        public string supervisorId { get; set; }
        public string? boxerUser { get; set; }
        public string? orderId { get; set; }
        public string? module { get; set; }
        public string? printerName { get; set; }
        public DateTime? transactionDate { get; set; }
        public bool? isSuccess { get; set; }


    }
   
}
