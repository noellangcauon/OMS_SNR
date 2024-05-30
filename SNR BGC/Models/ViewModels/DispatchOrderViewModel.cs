

using System.Collections.Generic;

namespace SNR_BGC.Models.ViewModels
{
    public class DispatchOrderViewModel
    {
        public DispatchOrderModel? DispatchOrders { get; set; }
        public List<DispatchOrderDetailModel>? DispatchOrderDetails { get; set; }
    }
}
