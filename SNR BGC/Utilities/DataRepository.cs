using SNR_BGC.DataAccess;
using SNR_BGC.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using SNR_BGC.Models.ViewModels;

namespace SNR_BGC.Utilities
{
    public class DataRepository : IDataRepository
    {
        private readonly IDbAccess _db;
        private readonly UserClass _context;

        public DataRepository(IDbAccess db, UserClass context)
        {
            _db = db;
            _context = context;
        }

        public async Task<IEnumerable<DiscrepancyCenterViewModel>> GetDiscrepancyOrders() =>
            await _db.ExecuteSP<DiscrepancyCenterViewModel, dynamic>("GetDiscrepancyOrders", new { });

        public async Task<OMSDashboardModel> GetOMSDashboard(string condition,string dateFrom, string dateTo) =>
           await _db.ExecuteSingleSP<OMSDashboardModel, dynamic>("OMSDashboard", new { condition, dateFrom, dateTo });

        public async Task<IEnumerable<OMSDashboardModel>> GetPickingTimePerPicker() =>
          await _db.ExecuteSP<OMSDashboardModel, dynamic>("GetPickingTimePerPicker", new { });

        public async Task<IEnumerable<ClearedOrders>> GetInventoryItem() =>
            await _db.ExecuteSP<ClearedOrders, dynamic>("GetInventoryItem", new { });

        public async Task<GridOrderHeaderClass> GetOrderDetails(string condition, string dateFrom, string dateTo) =>
          await _db.ExecuteSingleSP<GridOrderHeaderClass, dynamic>("GetOrderDetails", new { condition, dateFrom, dateTo });

        public async Task UpdateDiscrepancyStatus(string order_id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                DiscrepancyReportHdrClass modelItem = new DiscrepancyReportHdrClass();
                modelItem = _context.DiscrepancyReportHeaders.Where(i => i.orderId == order_id).FirstOrDefault();
                modelItem.status = "Cleared";

                _context.DiscrepancyReportHeaders.Update(modelItem);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch(Exception ex)
            {
                transaction.Rollback();
            }
           
        }

        public async Task<IEnumerable<CourierTypes>> GetCourierTypes() =>
             await _context.CourierTypes.ToListAsync();
        public async Task<IEnumerable<FleetTypes>> GetFleetTypes() =>
            await _context.FleetTypes.ToListAsync();

        public async Task<DispatchOrderDetailModel> GetBoxOrdersByTrackingNo(string trackingNo) =>
          await _db.ExecuteSingleSP<DispatchOrderDetailModel, dynamic>("GetBoxOrdersByTrackingNo", new { trackingNo });

        public async Task CreateDispatchOrder(DispatchOrders rmodel, List<DispatchOrderDetails> rdmodel)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                _context.Add(rmodel);
                await _context.SaveChangesAsync();

                foreach (var item in rdmodel)
                {
                    item.DispatchOrderId = rmodel.Id;
                    _context.AddRange(rdmodel);

                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}
