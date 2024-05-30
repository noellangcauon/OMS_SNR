using System;

namespace SNR_BGC.Models
{
    public class DispatchOrderDetails
    {
        public int Id { get; set; }
        public int DispatchOrderId { get; set; }

        public string TrackingNo { get; set; }

        public string OrderId { get; set; }

        public string PlatForm { get; set; }

        public DateTime DateScanned { get; set; }

        public string Status { get; set; }

        public string Remarks { get; set; }

    }
}
