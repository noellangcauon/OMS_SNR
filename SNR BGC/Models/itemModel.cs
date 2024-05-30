namespace SNR_BGC.Models
{
    public class itemModel
    {
        public string SKU { get; set; }
        public string UPC { get; set; }
        public int QTY { get; set; }

    }

    public partial class orderModel
    {
        public string SKU { get; set; }
        public string UPC { get; set; }
        public int QTY { get; set; }
        public string OrderId { get; set; }
        public string Answer { get; set; }

    }


}
