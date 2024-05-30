using System.ComponentModel.DataAnnotations;
using System;

namespace SNR_BGC.Models
{
    public class DiscrepancyReportHdr
    {
        [Key]
        public int discrepancyId { get; set; }
        public string? referenceNo { get; set; }
        public string orderId { get; set; }
        public DateTime? dateProcess { get; set; }
        public string boxerUser { get; set; }
        public string pickerUser { get; set; }
        public string status { get; set; }
    }
}
