using SNR_BGC.Models;
using SNR_BGC.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SNR_BGC.Utilities
{
    public interface IDataRepository
    {
        Task CreateDispatchOrder(DispatchOrders rmodel, List<DispatchOrderDetails> rdmodel);
        Task<DispatchOrderDetailModel> GetBoxOrdersByTrackingNo(string trackingNo);
        Task<IEnumerable<CourierTypes>> GetCourierTypes();
        Task<IEnumerable<DiscrepancyCenterViewModel>> GetDiscrepancyOrders();
        Task<IEnumerable<FleetTypes>> GetFleetTypes();
        Task<IEnumerable<ClearedOrders>> GetInventoryItem();
        Task<OMSDashboardModel> GetOMSDashboard(string condition, string dateFrom, string dateTo);
        Task<GridOrderHeaderClass> GetOrderDetails(string condition, string dateFrom, string dateTo);
        Task<IEnumerable<OMSDashboardModel>> GetPickingTimePerPicker();
        Task UpdateDiscrepancyStatus(string order_id);
    }
}
