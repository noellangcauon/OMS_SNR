using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class DiscrepancyReportDtlClass
    {
        [Key]
        public int id { get; set; }
        public int discrepancyId { get; set; }
        public string referenceNo { get; set; }
        public string orderId { get; set; }
        public string sku_id { get; set; }

        public DiscrepancyReportHdrClass Header { get; set; }

    }

}
