﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class CanceledOrders
    {
        [Key]
        public int id { get; set; }
        public string orderId { get; set; }
        public DateTime? dateFetch { get; set; }
        public DateTime? dateProcess { get; set; }
        public DateTime? dateCreatedAt { get; set; }
        public string? module { get; set; }
        public string? status { get; set; }
        public string? customerID { get; set; }
        public string? employee { get; set; }
        public decimal? item_count { get; set; }
        public decimal? total_amount { get; set; }

    }
   
}
