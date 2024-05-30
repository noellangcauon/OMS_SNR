using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class DiscrepancyReportHdrClass
    {
        [Key]
        public int discrepancyId { get; set; }
        public string referenceNo { get; set; }
        public string orderId { get; set; }
        public DateTime? dateProcess { get; set; }
        public string boxerUser { get; set; }
        public string pickerUser { get; set; }
        public string status { get; set; }

        public List<DiscrepancyReportDtlClass> Details { get; set; }
    }
   
}
