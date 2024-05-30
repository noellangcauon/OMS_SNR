using Microsoft.AspNetCore.Mvc;
using SNR_BGC.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using SNR_BGC.Utilities;
using SNR_BGC.DataAccess;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration;
using Polly;
using Microsoft.Extensions.Configuration;

namespace SNR_BGC.Controllers
{
    public class DiscrepancyCenterController : Controller
    {

        private readonly UserClass _userInfoConn;
        private readonly IDataRepository _dataRepository;
        private readonly IDbAccess _dataAccess;
        private readonly IConfiguration _configuration;


        public DiscrepancyCenterController(UserClass userinfo, IDataRepository dataRepository, IDbAccess dataAccess,IConfiguration configuration)
        {
            _userInfoConn = userinfo;
            _dataRepository = dataRepository;
            _dataAccess = dataAccess;
            _configuration = configuration;

        }

        public IActionResult DiscrepancyCenter()
        {
            return View();
        }



        public JsonResult GetDiscrepancyOrders()
        {
            IEnumerable<DiscrepancyCenterViewModel> items = new List<DiscrepancyCenterViewModel>();
            items = _dataAccess.ExecuteSP2<DiscrepancyCenterViewModel, dynamic>("GetDiscrepancyOrders", new { });
            return Json(new { set = items });

        }

        public JsonResult GetBoxOrderItem(string order_id)
        {
            IEnumerable<DiscrepancyCenterViewModel> items = new List<DiscrepancyCenterViewModel>();
            items = _dataAccess.ExecuteSP2<DiscrepancyCenterViewModel, dynamic>("sp_GetBoxOrderItem", new { order_id });
            return Json(new { set = items });

        }

        public JsonResult GetOrdersByOrderId(string order_id)
        {
            IEnumerable<OrdersTableViewModel> items = new List<OrdersTableViewModel>();
            items = _dataAccess.ExecuteSP2<OrdersTableViewModel, dynamic>("GetOrdersByOrderId", new { order_id });
            return Json(new { set = items });

        }

        public async Task<IActionResult> GetDiscrepancyCount()
        {
            IEnumerable<DiscrepancyCenterViewModel> items = new List<DiscrepancyCenterViewModel>();
            items = _dataAccess.ExecuteSP2<DiscrepancyCenterViewModel, dynamic>("GetDiscrepancyCount", new { });
            return Ok(items);
        }


        public async Task<IActionResult> SaveDiscrepancy(string order_id, string referenceNo)
        {
            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var user = claims.Claims.ToList()[0].Value;
            //using var transaction = await _userInfoConn.Database.BeginTransactionAsync();
            string status = "";
            try
            {
                
                var clearedOrders = _userInfoConn.clearedOrders.Where(i => i.orderId == order_id).FirstOrDefault();
                if (clearedOrders != null)
                {
                    if (clearedOrders.boxQRCode.ToUpper() != referenceNo.ToUpper())
                    {
                        var existingTub = _userInfoConn.clearedOrders.Where(i => i.boxQRCode == referenceNo).ToList();
                        if (existingTub.Count >= 1)
                        {

                            foreach (var itemTub in existingTub)
                            {
                                var existsInBoxOrders = _userInfoConn.boxOrders.Where(i => i.orderId == itemTub.orderId).FirstOrDefault();
                                if (existsInBoxOrders != null)
                                {
                                    if (existsInBoxOrders.boxerStatus != "Done")
                                    {
                                        status = "bad";
                                    }
                                    else
                                    {
                                        status = "good";

                                    }
                                }
                            }
                            if (status == "good")
                            {
                                IEnumerable<DiscrepancyCenterViewModel> items = new List<DiscrepancyCenterViewModel>();
                                items = _dataAccess.ExecuteSP2<DiscrepancyCenterViewModel, dynamic>("GetBoxOrderByOrderID", new { order_id });
                                DiscrepancyOrders model = new DiscrepancyOrders();
                                foreach (var item in items)
                                {
                                    var boxOrders = _userInfoConn.boxOrders.Where(i => i.boxId == item.boxId).FirstOrDefault();

                                    //boxOrders.boxerStatus = "Cleared";

                                    //_userInfoConn.boxOrders.Update(boxOrders);

                                    model.boxId = boxOrders.boxId;
                                    model.orderId = boxOrders.orderId;
                                    model.skuId = boxOrders.skuId;
                                    model.module = boxOrders.module;
                                    model.referenceNo = referenceNo;
                                    model.dateCreated = DateTime.Now;
                                    model.createdBy = user;

                                    _userInfoConn.DiscrepancyOrders.Add(model);
                                    _userInfoConn.boxOrders.Remove(boxOrders);



                                }

                                IEnumerable<ClearedOrders> clearedItem = new List<ClearedOrders>();
                                clearedItem = _userInfoConn.clearedOrders.Where(i => i.orderId == order_id).ToList();

                                foreach (var item in clearedItem)
                                {
                                    item.boxQRCode = referenceNo;
                                }
                                _userInfoConn.clearedOrders.UpdateRange(clearedItem);

                               


                            }
                            else
                            {
                                return Json("ExistingTub");
                            }

                        }
                        else
                        {
                            IEnumerable<DiscrepancyCenterViewModel> items = new List<DiscrepancyCenterViewModel>();
                            items = _dataAccess.ExecuteSP2<DiscrepancyCenterViewModel, dynamic>("GetBoxOrderByOrderID", new { order_id });
                            DiscrepancyOrders model = new DiscrepancyOrders();
                            foreach (var item in items)
                            {
                                var boxOrders = _userInfoConn.boxOrders.Where(i => i.boxId == item.boxId).FirstOrDefault();

                               

                                model.boxId = boxOrders.boxId;
                                model.orderId = boxOrders.orderId;
                                model.skuId = boxOrders.skuId;
                                model.module = boxOrders.module;
                                model.referenceNo = referenceNo;
                                model.dateCreated = DateTime.Now;
                                model.createdBy = user;

                                _userInfoConn.DiscrepancyOrders.Add(model);
                                _userInfoConn.boxOrders.Remove(boxOrders);

                               



                            }

                            IEnumerable<ClearedOrders> clearedItem = new List<ClearedOrders>();
                            clearedItem = _userInfoConn.clearedOrders.Where(i => i.orderId == order_id).ToList();

                            foreach (var item in clearedItem)
                            {
                                item.boxQRCode = referenceNo;
                            }
                            _userInfoConn.clearedOrders.UpdateRange(clearedItem);

                           

                        }

                    }
                    else
                    {
                        IEnumerable<DiscrepancyCenterViewModel> items = new List<DiscrepancyCenterViewModel>();
                        items = _dataAccess.ExecuteSP2<DiscrepancyCenterViewModel, dynamic>("GetBoxOrderByOrderID", new { order_id });
                        DiscrepancyOrders model = new DiscrepancyOrders();
                        foreach (var item in items)
                        {
                            var boxOrders = _userInfoConn.boxOrders.Where(i => i.boxId == item.boxId).FirstOrDefault();

                           

                            model.boxId = boxOrders.boxId;
                            model.orderId = boxOrders.orderId;
                            model.skuId = boxOrders.skuId;
                            model.module = boxOrders.module;
                            model.referenceNo = referenceNo;
                            model.dateCreated = DateTime.Now;
                            model.createdBy = user;

                            //_userInfoConn.DiscrepancyOrders.Add(model);
                            _userInfoConn.boxOrders.Remove(boxOrders);

                        }
                       

                    }
                }

                var result_clear = string.Empty;
                var csd = _configuration.GetConnectionString("Myconnection");
                using var connsd = new SqlConnection(csd);
                connsd.Open();

                string sqld = $"EXEC UpdateDiscrepancyStatus @orderId='{order_id}'";
                using var cmdd = new SqlCommand(sqld, connsd);
                result_clear = (cmdd.ExecuteScalar()).ToString();
                connsd.Close();

                if(result_clear != "Success"){
                    throw new Exception(result_clear);
                }


                 _userInfoConn.SaveChanges();
                //await _userInfoConn.SaveChanges();
                //await transaction.CommitAsync();



                return Json("Ok");
            }
            catch (Exception ex)
            {
                //await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }



        }

    }
}
