namespace SNR_BGC.Models
{
    public class OMSDashboardModel
    {
        public decimal count_result { get; set; }
        public decimal AverageOrderPerHour { get; set; }

        public string username { get; set; }
        public string userFullname { get; set; }
        public string pickingTime { get; set; }
     
        public decimal itemCount { get; set; }

        public decimal orderCount { get; set; }

        public decimal totalPickingTime { get; set; }
        public int row_num { get; set; }

        public string? Description { get; set; }
        public int? Lazada { get; set; }
        public int? Shopee { get; set; }

    }
}
