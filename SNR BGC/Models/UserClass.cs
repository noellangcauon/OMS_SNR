using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SNR_BGC.Models;

namespace SNR_BGC.Models
{
    public class UserClass : DbContext
    {
        public UserClass(DbContextOptions<UserClass> options) : base(options)
        {

    
        }
        public DbSet<UserInfoClass> userAccessTable { get; set; }
        public DbSet<TokenClass> tokenTable { get; set; }       
        public DbSet<OrderClass> ordersTable { get; set; }
        public DbSet<Transaction> transaction { get; set; }
        public DbSet<OrderHeaderClass> orderTableHeader { get; set; }
        public DbSet<ClearedOrders> clearedOrders { get; set; }
        public DbSet<UsersTable> usersTable { get; set; }
        public DbSet<AuthClass> authTable { get; set; }
        public DbSet<BoxOrders> boxOrders { get; set; }
        public DbSet<DiscrepancyOrders> DiscrepancyOrders { get; set; }
        public DbSet<CanceledOrders> CanceledOrders { get; set; }
        public DbSet<ShipmentList> ShipmentList { get; set; }
        public DbSet<SupervisorTable> SupervisorTable { get; set; }
        public DbSet<ReprintWaybillTable> ReprintWaybillTable { get; set; }
        public DbSet<ExceptionItems> ExceptionItems { get; set; }
        public DbSet<OrdersCancelledHeader> OrdersCancelledHeader { get; set; }
        public DbSet<OrdersCancelledDetails> OrdersCancelledDetails { get; set; }
        public DbSet<IPaddressForAutoReload> IPaddressForAutoReload { get; set; }
        public DbSet<ErrorLogs> ErrorLogs { get; set; }
        public DbSet<DiscrepancyReportHdrClass> DiscrepancyReportHeaders { get; set; }
        public DbSet<DiscrepancyReportDtlClass> DiscrepancyReportDetails { get; set; }

        public DbSet<DiscrepancyReportHdr> DiscrepancyReportHdr { get; set; }
        public DbSet<SettingsTable> SettingsTable { get; set; }
        public DbSet<RemarksTable> RemarksTable { get; set; }
        public DbSet<printerExeClass> printerExe { get; set; }

        public DbSet<CourierTypes> CourierTypes { get; set; }
        public DbSet<FleetTypes> FleetTypes { get; set; }
        public DbSet<DispatchOrders> DispatchOrders { get; set; }
        public DbSet<DispatchOrderDetails> DispatchOrderDetails { get; set; }

        public DbSet<BoxerLogs> BoxerLogs { get; set; }
        public DbSet<OperationTimeOut> OperationTimeOut { get; set; }
        public DbSet<AutoReloadLogs> AutoReloadLogs { get; set; }
        public DbSet<ItemUPC> ItemUPC { get; set; }
        public DbSet<ECommerceStatus> ECommerceStatus { get; set; }
        public DbSet<PasswordHistory> PasswordHistory { get; set; }
        public DbSet<AuditLogs> AuditLogs { get; set; }








    }

}
