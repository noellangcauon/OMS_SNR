using System;

namespace SNR_BGC.Models
{
    public class DispatchOrders
    {
        public int Id { get; set; }

        public int CourierTypeId { get; set; }

        public int FleetTypeId { get; set; }

        public string PlateNo { get; set; }

        public DateTime DateCreated { get; set; }

        public string UserId { get; set; }

    }
}
