using AspNetCore.ReportingServices.ReportProcessing.ReportObjectModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SNR_BGC.DataAccess;
using SNR_BGC.Models;
using SNR_BGC.Utilities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;

namespace SNR_BGC.Controllers
{
    public class PickerController : Controller
    {
        private readonly UserClass _userInfoConn;
        private readonly IConfiguration _configuration;
        private readonly IDbAccess _dataAccess;

        public PickerController(IConfiguration configuration, UserClass userinfo, IDbAccess dataAccess)
        {

            _configuration = configuration;
            _userInfoConn = userinfo;
            _dataAccess = dataAccess;
        }
        public IActionResult Index()
        {
            return View();
        }

        public JsonResult GetItemPick()
        {

            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var csd = _configuration.GetConnectionString("Myconnection");
            using var connsd = new SqlConnection(csd);
            connsd.Open();


            var user = claims.Claims.ToList()[0].Value;
            string sqld = $"EXEC GetOrderForPicker @User='{user}', @Status='Pick'";
            using var cmdd = new SqlCommand(sqld, connsd);
            SqlDataReader result_clear = cmdd.ExecuteReader();


            List<PickerClass> items = new List<PickerClass>();
            var n = 0;
            while (result_clear.Read())
            {
                PickerClass item = new PickerClass();
                item.skuId = result_clear["skuid"].ToString();
                item.orderId = result_clear["orderId"].ToString();
                item.item_description = result_clear["item_description"].ToString();
                item.item_price = (decimal)result_clear["item_price"];
                item.UPC = (decimal)result_clear["UPC"];
                item.item_image = result_clear["item_image"].ToString();
                item.transferLocation = result_clear["transferLocation"].ToString();
                item.module = result_clear["module"].ToString();
                item.inventoryLocation = result_clear["inventoryLocation"].ToString();
                item.Quantity = (int)result_clear["Quantity"];
                item.PickedQty = (int)result_clear["PickedQty"];
                item.pickingStartTime = result_clear["pickingStartTime"].ToString() == "" ? null : (DateTime?)result_clear["pickingStartTime"];
                item.pickingEndTime = result_clear["pickingEndTime"].ToString() == "" ? null : (DateTime?)result_clear["pickingEndTime"];
                item.isFromNIB = (bool)result_clear["isFromNIB"];
                item.hasPicked = (int)result_clear["hasPicked"];
                items.Add(item);


                n++;
            }
            string[] list = new string[n];
            for (var i = 0; i < n; i++)
            {
                list[i] = items[i].orderId.ToString();
            }
            string[] orderNum = list.Distinct().ToArray();

            var orderList = new List<OrderNumberList>();

            for (var j = 0; j < orderNum.Count(); j++)
            {
                OrderNumberList item = new OrderNumberList();

                var ordersTable = new List<ClearedOrders>();
                ordersTable = _userInfoConn.clearedOrders.Where(e => e.orderId == orderNum[j].ToString() && e.pickerUser == user).ToList();

                var ordersTable2 = new List<ClearedOrders>();
                ordersTable2 = _userInfoConn.clearedOrders.Where(e => e.orderId == orderNum[j].ToString() && e.pickerStatus == "Picked").ToList();


                var ordersTable3 = new List<ClearedOrders>();
                ordersTable3 = _userInfoConn.clearedOrders.Where(e => e.orderId == orderNum[j].ToString() && e.isFromNIB == true).ToList();


                if (ordersTable.Count() == ordersTable2.Count())
                {

                    item.orderId = orderNum[j];
                    item.status = "badge-success";
                    orderList.Add(item);

                }
                else if (ordersTable3.Count > 0)
                {

                    var ordersTable4 = new List<ClearedOrders>();
                    ordersTable4 = _userInfoConn.clearedOrders.Where(e => e.orderId == orderNum[j].ToString() && e.pickerStatus == "Picked").ToList();
                    if (ordersTable4.Count() > 0)
                    {
                        item.orderId = orderNum[j];
                        item.status = "badge-warning";
                        orderList.Add(item);

                    }
                    else
                    {
                        item.orderId = orderNum[j];
                        item.status = "";
                        orderList.Add(item);

                    }

                }
                else if (ordersTable2.Count() == 0)
                {
                    item.orderId = orderNum[j];
                    item.status = "";
                    orderList.Add(item);
                }
                else
                {
                    item.orderId = orderNum[j];
                    item.status = "badge-danger";
                    orderList.Add(item);

                }
            }

            connsd.Close();
            bool isDone;
            var clearedOrders = new List<ClearedOrders>();
            var clearedOrders2 = new List<ClearedOrders>();
            clearedOrders = _userInfoConn.clearedOrders.Where(e => e.pickerUser == user && e.pickerStatus == "Picked").ToList();
            clearedOrders2 = _userInfoConn.clearedOrders.Where(e => e.pickerUser == user).ToList();

            if (clearedOrders.Count() == clearedOrders2.Count())
            {
                isDone = true;
            }
            else
            {
                isDone = false;
            }
            //var items = new List<OrderClass>();
            //items = _userInfoConn.ordersTable.Where(e => e.typeOfexception == "NIB").ToList();


            return Json(new { set = items, status = orderList, isdone = isDone });
        }
        static bool IsNotInteger(string input)
        {
            // Try to parse the string to an integer
            if (double.TryParse(input, out _))
            {
                // If successful, it's an integer
                return false;
            }
            else
            {
                // If not successful, it's not an integer
                return true;
            }
        }

        public JsonResult ScannedUPC(string upc, int sku)
        {
            var status = string.Empty;
            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            //var cs = "Data Source=199.84.0.151;Initial Catalog=SNR_ECOMMERCE;User ID=apps;Password=546@Apps#88";
            //using var conns = new SqlConnection(cs);
            //conns.Open();

            if (IsNotInteger(upc))
            {
                return Json(new { set = upc, status = "Invalid" });
            }

            decimal? upcresult = null;
            var itemUPC = _userInfoConn.ItemUPC.Where(w => w.SKU == sku && w.UPC == decimal.Parse(upc)).FirstOrDefault();

            //string Upc = $"SELECT UPC FROM [SNR_ECOMMERCE].[dbo].[ItemUPC] Where SKU={sku} AND UPC = {upc} ";
            //using var cmddd = new SqlCommand(Upc, conns);
            //var upcresult = cmddd.ExecuteScalar();
            //conns.Close();

            string convertedUPC = string.Empty;
            if (itemUPC == null)
            {
                BarcodeConverter barcodeConverter = new BarcodeConverter();
                convertedUPC = barcodeConverter.UPCConverter(upc);
                if (convertedUPC != "")
                {
                    var claims2 = (System.Security.Claims.ClaimsIdentity)User.Identity;
                    //var cs2 = "Data Source=199.84.0.151;Initial Catalog=SNR_ECOMMERCE;User ID=apps;Password=546@Apps#88";
                    //using var conns2 = new SqlConnection(cs2);
                    //conns2.Open();


                    //string Upc2 = $"SELECT UPC FROM [SNR_ECOMMERCE].[dbo].[ItemUPC] Where SKU={sku} AND UPC = {convertedUPC} ";
                    //using var cmddd2 = new SqlCommand(Upc2, conns2);
                    //upcresult = cmddd2.ExecuteScalar();

                    //conns2.Close();

                    itemUPC = _userInfoConn.ItemUPC.Where(w => w.SKU == sku && w.UPC == decimal.Parse(convertedUPC)).FirstOrDefault();
                }
            }

            if (itemUPC == null)
            {
                status = "Wrong";
            }
            else
            {
                upcresult = itemUPC.UPC;
                status = "Correct";
            }


            //var items = new List<OrderClass>();
            //items = _userInfoConn.ordersTable.Where(e => e.typeOfexception == "NIB").ToList();

            return Json(new { set = upcresult, status = status });
        }

        public JsonResult CheckTub(string result, string orderId)
        {

            //var clearedOrder = new List<ClearedOrders>();
            //clearedOrder = _userInfoConn.clearedOrders.Where(e => e.boxQRCode == result).ToList();

            //if (clearedOrder.Count() < 1)
            //{
            //    return Json(new { set = "Good" });
            //}
            //else
            //{


            //    for (int i = 0; i < clearedOrder.Count(); i++)
            //    {
            //        var boxOrder = new List<BoxOrders>();
            //        boxOrder = _userInfoConn.boxOrders.Where(e => e.orderId == clearedOrder[i].orderId && e.boxerStatus == "Discrepancy").ToList();
            //        if (boxOrder.Count() > 0)
            //        {
            //            return Json(new { set = "Discrepancy" });
            //        }
            //        var boxOrder2 = new List<BoxOrders>();
            //        boxOrder2 = _userInfoConn.boxOrders.Where(e => e.orderId == clearedOrder[i].orderId).ToList();
            //        if (boxOrder2.Count() < 1)
            //        {
            //            return Json(new { set = "InUse" });
            //        }
            //        else
            //        {
            //            var boxOrder3 = new List<BoxOrders>();
            //            boxOrder3 = _userInfoConn.boxOrders.Where(e => e.orderId == clearedOrder[i].orderId && e.boxerStatus != "Done").ToList();
            //            if (boxOrder3.Count() > 0)
            //            {
            //                return Json(new { set = "InUse" });
            //            }
            //        }

            //    }

            //    return Json(new { set = "Good" });

            //}

            var csd = _configuration.GetConnectionString("Myconnection");
            using var connsd = new SqlConnection(csd);
            connsd.Open();

            string tubresult = $"EXEC [GetTubForPicker] '{result}'";
            using var tub_result = new SqlCommand(tubresult, connsd);
            var resultTub = tub_result.ExecuteScalar();

            connsd.Close();

            return Json(new { set = resultTub });
            //var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            //var cs = "Data Source=199.84.0.151;Initial Catalog=SNR_ECOMMERCE;User ID=apps;Password=546@Apps#88";
            //using var conns = new SqlConnection(cs);
            //conns.Open();


            //string Upc = $"SELECT UPC FROM [SNR_ECOMMERCE].[dbo].[ItemUPC] Where SKU={sku} AND UPC = {upc} ";
            //using var cmddd = new SqlCommand(Upc, conns);
            //var upcresult = cmddd.ExecuteScalar();

            ////var items = new List<OrderClass>();
            ////items = _userInfoConn.ordersTable.Where(e => e.typeOfexception == "NIB").ToList();

            //conns.Close();
            //return Json(new { set = result });
        }

        public JsonResult NIB(string orderId, string sku)
        {

            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var user = claims.Claims.ToList()[0].Value;
            var orderTableHeader = new OrderHeaderClass();
            var orderTable = new List<OrderClass>();

            var clearedOrder = new List<ClearedOrders>();


            orderTable = _userInfoConn.ordersTable.Where(e => e.orderId == orderId && e.sku_id == sku).ToList();
            for (int i = 0; i < orderTable.Count; i++)
            {
                orderTable[i].typeOfexception = "NIB";
                orderTable[i].exception = 1;
                orderTable[i].runnerUser = null;
                orderTable[i].runnerStatus = null;
                orderTable[i].transferLocation = null;
                orderTable[i].collectingStartTime = null;
                orderTable[i].collectingEndTime = null;
                orderTable[i].transferringStartTime = null;
                orderTable[i].transferringEndTime = null;
                orderTable[i].exception = 1;
                _userInfoConn.Update(orderTable[i]);
                _userInfoConn.SaveChanges();
            }

            orderTableHeader = _userInfoConn.orderTableHeader.Where(e => e.orderId == orderId).FirstOrDefault();
            orderTableHeader.status = "exception";
            orderTableHeader.exception = 1;
            _userInfoConn.Update(orderTableHeader);


            clearedOrder = _userInfoConn.clearedOrders.Where(e => e.orderId == orderId && e.skuId == sku).ToList();
            for (int k = 0; k < clearedOrder.Count; k++)
            {
                clearedOrder[k].isNIB = true;
                clearedOrder[k].NIBUser = user;
                _userInfoConn.Update(orderTableHeader);
            }

            _userInfoConn.SaveChanges();

            var clearedOrder2 = new List<ClearedOrders>();
            clearedOrder2 = _userInfoConn.clearedOrders.Where(e => e.orderId == orderId).ToList();
            for (int j = 0; j < clearedOrder2.Count; j++)
            {
                clearedOrder2[j].pickerUser = null;
                _userInfoConn.Update(orderTableHeader);
            }

            _userInfoConn.SaveChanges();




            var exceptionItems = new ExceptionItems();
            exceptionItems.orderId = orderId;
            exceptionItems.sku = sku;
            exceptionItems.qty = clearedOrder.Count;
            exceptionItems.user = user;
            exceptionItems.userType = "Picker";
            exceptionItems.typeOfException = "NIB";
            exceptionItems.dateProcess = DateTime.Now;
            _userInfoConn.Add(exceptionItems);
            _userInfoConn.SaveChanges();
            //exceptionItems = _userInfoConn.ExceptionItems.Where(e => e.orderId == orderId).ToList();

            return Json(new { set = clearedOrder });
        }
        public JsonResult ScannedQRContainer(string upc, int sku)
        {

            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            //var cs = "Data Source=199.84.0.151;Initial Catalog=SNR_ECOMMERCE;User ID=apps;Password=546@Apps#88";
            //using var conns = new SqlConnection(cs);
            //conns.Open();


            //string Upc = $"SELECT UPC FROM [SNR_ECOMMERCE].[dbo].[ItemUPC] Where SKU={sku} AND UPC = {upc} ";
            //using var cmddd = new SqlCommand(Upc, conns);
            //var upcresult = cmddd.ExecuteScalar();
            decimal? upcresult = null;
            var itemUPC = _userInfoConn.ItemUPC.Where(w => w.SKU == sku && w.UPC == decimal.Parse(upc)).FirstOrDefault();

            if (itemUPC != null)
                upcresult = itemUPC.UPC;

            //var items = new List<OrderClass>();
            //items = _userInfoConn.ordersTable.Where(e => e.typeOfexception == "NIB").ToList();

                //conns.Close();
            return Json(new { set = upcresult });
        }
        public JsonResult DonePicker(PickerOrderQr pickerQrClass)
        {

            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var user = claims.Claims.ToList()[0].Value;
            var clearedOrders = new List<ClearedOrders>();
            clearedOrders = _userInfoConn.clearedOrders.Where(e => e.pickerUser == user && e.pickerStatus == "Picked").ToList();
            for (var i = 0; i < clearedOrders.Count(); i++)
            {

                var canceledOrders = new List<CanceledOrders>();
                canceledOrders = _userInfoConn.CanceledOrders.Where(e => e.orderId == clearedOrders[i].orderId).ToList();
                if (canceledOrders.Count() > 0)
                {
                    var cs = _configuration.GetConnectionString("Myconnection");
                    using var conns = new SqlConnection(cs);
                    conns.Open();


                    string Upc = $"EXEC InsertCancelledOrders @orderId='{clearedOrders[i].orderId}'";
                    using var cmddd = new SqlCommand(Upc, conns);
                    var upcresult = cmddd.ExecuteScalar();

                    conns.Close();



                    return Json(new { set = clearedOrders, message = "CancelledOrders" });
                }


                var boxOrders = new BoxOrders();

                for (var j = 0; j < pickerQrClass.qrList.Count(); j++)
                {
                    if (clearedOrders[i].orderId == pickerQrClass.qrList[j].orderId)
                    {
                        clearedOrders[i].pickerStatus = "Done";
                        clearedOrders[i].pickingEndTime = DateTime.Now;

                        var clearedOrders2 = new ClearedOrders();
                        clearedOrders2 = _userInfoConn.clearedOrders.Where(e => e.boxQRCode == pickerQrClass.qrList[j].qrCode && e.orderId != clearedOrders[i].orderId).FirstOrDefault();

                        if (clearedOrders2 == null)
                        {

                            clearedOrders[i].boxQRCode = pickerQrClass.qrList[j].qrCode;
                        }
                        else
                        {

                            var boxOrders2 = new BoxOrders();
                            boxOrders2 = _userInfoConn.boxOrders.Where(e => e.orderId == clearedOrders2.orderId).FirstOrDefault();
                            if (boxOrders2.boxerStatus == "Done")
                            {
                                clearedOrders[i].boxQRCode = pickerQrClass.qrList[j].qrCode;
                            }
                            else
                            {
                                return Json(new { set = clearedOrders, message = "QrExist" });
                            }
                        }

                    }
                }

                _userInfoConn.Update(clearedOrders[i]);

                //boxOrders.reserveId = clearedOrders[i].reserveId;
                //boxOrders.skuId = clearedOrders[i].skuId;
                //boxOrders.orderId = clearedOrders[i].orderId;
                //boxOrders.module = clearedOrders[i].module;
                //boxOrders.processBy = user;
                //boxOrders.dateProcess = DateTime.Now;
                //_userInfoConn.Add(boxOrders);

                _userInfoConn.SaveChanges();

            }


            //var pickOrders = new List<ClearedOrders>();
            //pickOrders = _userInfoConn.clearedOrders.Where(e => e.pickerUser == user && e.pickerStatus == "Pick" && e.isNIB == true).ToList();

            //var pickedOrders = new List<ClearedOrders>();
            //pickedOrders = _userInfoConn.clearedOrders.Where(e => e.pickerUser == user && e.pickerStatus == "Picked" && e.isNIB == true).ToList();


            //if (pickOrders.Count > 0 || pickedOrders.Count > 0)
            //{

            //    return Json(new { set = clearedOrders, message = "SuccessNotComplete" });
            //}
            //else
            //{

                return Json(new { set = clearedOrders, message = "SuccessComplete" });
            //}



        }


        public JsonResult SaveItem(orderModel item)
        {

            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var user = claims.Claims.ToList()[0].Value;
            var clearedOrders = new List<ClearedOrders>();
            clearedOrders = _userInfoConn.clearedOrders.Where(e => e.skuId == item.SKU && e.pickerUser == user && e.orderId == item.OrderId).ToList();
            for (int i = 0; i < clearedOrders.Count(); i++)
            {
                if (item.Answer == "yes")
                {
                    if (i < item.QTY)
                    {
                        clearedOrders[i].pickerStatus = "Picked";
                        _userInfoConn.Update(clearedOrders[i]);
                    }
                    else
                    {

                        clearedOrders[i].isNIB = true;
                        _userInfoConn.Update(clearedOrders[i]);

                        var ordersTable = new List<OrderClass>();
                        ordersTable = _userInfoConn.ordersTable.Where(e => e.orderId == clearedOrders[i].orderId && e.sku_id == clearedOrders[i].skuId).ToList();
                        var ordersTableHeader = new OrderHeaderClass();
                        ordersTableHeader = _userInfoConn.orderTableHeader.Where(e => e.orderId == clearedOrders[i].orderId).FirstOrDefault();
                        for (int j = 0; j < ordersTable.Count(); j++)
                        {
                            ordersTable[j].exception = 1;
                            ordersTable[j].typeOfexception = "NIB";
                            _userInfoConn.Update(ordersTable[j]);

                        }
                        ordersTableHeader.exception = 1;
                        _userInfoConn.Update(ordersTableHeader);


                    }
                }
                else
                {
                    if (i < item.QTY)
                    {
                        clearedOrders[i].pickerStatus = "Picked";
                        _userInfoConn.Update(clearedOrders[i]);
                    }
                }


            }

            _userInfoConn.SaveChanges();



            return Json(new { set = clearedOrders });
        }
        public JsonResult SaveItemLocation(itemLocationModel itemLoc)
        {

            var cs_ecom = "Data Source=199.84.17.95;Initial Catalog=EcommerceHub;User ID=apps;Password=546@Apps#88";
            using var conns_ecom = new SqlConnection(cs_ecom);
            conns_ecom.Open();

            string sql_ecom = $"SELECT count(*) FROM [EcommerceHub].[dbo].[InventoryLocations] Where Description= '{itemLoc.Location}'";
            using var cmd_ecom = new SqlCommand(sql_ecom, conns_ecom);
            var result_ecom = cmd_ecom.ExecuteScalar();

            var ordersTable = new List<OrderClass>();

            if ((int)result_ecom > 0)
            {

                var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
                var user = claims.Claims.ToList()[0].Value;
                ordersTable = _userInfoConn.ordersTable.Where(e => e.sku_id == itemLoc.SKU && e.runnerUser == user).ToList();
                for (int i = 0; i < ordersTable.Count; i++)
                {

                    ordersTable[i].transferLocation = itemLoc.Location;
                    _userInfoConn.Update(ordersTable[i]);
                }



                _userInfoConn.SaveChanges();
            }




            return Json(new { set = ordersTable });
        }
        public JsonResult OutOfStockItem(itemModel item)
        {

            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var user = claims.Claims.ToList()[0].Value;
            var ordersTable = new List<OrderClass>();
            ordersTable = _userInfoConn.ordersTable.Where(e => e.sku_id == item.SKU && e.runnerUser == user).ToList();
            for (int i = 0; i < ordersTable.Count; i++)
            {
                //UPDATE INVENTORY TO OOS
                ordersTable[i].runnerStatus = "";
                ordersTable[i].runnerUser = "";
                ordersTable[i].typeOfexception = "OOS";
                _userInfoConn.Update(ordersTable[i]);
            }

            _userInfoConn.SaveChanges();



            return Json(new { set = ordersTable });
        }

        public JsonResult CheckItem()
        {
            var status = string.Empty;
            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var user = claims.Claims.ToList()[0].Value;
            var ordersTable = new List<OrderClass>();
            ordersTable = _userInfoConn.ordersTable.Where(e => e.runnerUser == user && e.runnerStatus == "Transfer").ToList();


            if (ordersTable.Count > 0)
            {
                status = "Transfer";
            }
            else
            {
                status = "Collect";
            }




            return Json(new { set = status });
        }

        public JsonResult ItemCollected(itemModel item)
        {

            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var user = claims.Claims.ToList()[0].Value;
            var ordersTable = new List<OrderClass>();
            //ordersTable = _userInfoConn.ordersTable.Where(e => e.runnerStatus == "Collect" && e.runnerUser == user).ToList();
            //for (int i = 0; i < ordersTable.Count; i++)
            //{
            //    //UPDATE INVENTORY TO OOS
            //    ordersTable[i].runnerStatus = "";
            //    ordersTable[i].runnerUser = "";
            //    _userInfoConn.Update(ordersTable[i]);
            //}

            var ordersTable2 = new List<OrderClass>();
            ordersTable2 = _userInfoConn.ordersTable.Where(e => e.runnerStatus == "Collected" && e.runnerUser == user).ToList();
            for (int x = 0; x < ordersTable2.Count; x++)
            {
                //UPDATE INVENTORY TO OOS
                ordersTable2[x].runnerStatus = "Transfer";
                ordersTable2[x].collectingEndTime = DateTime.Now;
                ordersTable2[x].transferringStartTime = DateTime.Now;
                _userInfoConn.Update(ordersTable2[x]);
            }
            _userInfoConn.SaveChanges();



            return Json(new { set = ordersTable2 });
        }

        public JsonResult ItemTransferred(itemModel item)
        {

            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var user = claims.Claims.ToList()[0].Value;
            var ordersTable = new List<OrderClass>();
            var ordersTableHeader = new OrderHeaderClass();
            ordersTable = _userInfoConn.ordersTable.Where(e => e.runnerStatus == "Transfer" && e.runnerUser == user).ToList();

            var clearedOrders = new ClearedOrders();
            for (int i = 0; i < ordersTable.Count; i++)
            {
                //UPDATE INVENTORY TO OOS
                ordersTable[i].runnerStatus = "Transferred";
                ordersTable[i].typeOfexception = "";
                ordersTable[i].exception = 0;
                ordersTable[i].transferringEndTime = DateTime.Now;
                _userInfoConn.Update(ordersTable[i]);


                clearedOrders.deductedStockEcom = 1;
                clearedOrders.deductedStock2017 = 1;
                clearedOrders.dateProcess = DateTime.Now;
                clearedOrders.skuId = ordersTable[i].sku_id;
                clearedOrders.orderId = ordersTable[i].orderId;
                clearedOrders.module = ordersTable[i].module;
                clearedOrders.processBy = "System";
                clearedOrders.isFreeItem = false;
                clearedOrders.isFromNIB = false;
                clearedOrders.isNIB = false;
                _userInfoConn.Add(clearedOrders);


                ordersTableHeader = _userInfoConn.orderTableHeader.Where(e => e.orderId == ordersTable[i].orderId).FirstOrDefault();
                ordersTableHeader.exception = 0;
                ordersTableHeader.status = "";
                _userInfoConn.Update(ordersTableHeader);
            }


            _userInfoConn.SaveChanges();

            return Json(new { set = ordersTable });
        }



    }






}
