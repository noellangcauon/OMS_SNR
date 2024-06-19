using AspNetCore.ReportingServices.ReportProcessing.ReportObjectModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SNR_BGC.Models;
using SNR_BGC.Utilities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;

namespace SNR_BGC.Controllers
{
    public class RunnerController : Controller
    {
        private readonly UserClass _userInfoConn;
        private readonly IConfiguration _configuration;

        public RunnerController(IConfiguration configuration, UserClass userinfo)
        {
            _configuration = configuration;
            _userInfoConn = userinfo;
        }
        public IActionResult Index()
        {
            return View();
        }

        public JsonResult GetItemCollect()
        {

            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var users = claims.Claims.ToList()[0].Value;
            var csd = _configuration.GetConnectionString("Myconnection");
            using var connsd = new SqlConnection(csd);
            connsd.Open();



            string sqld = $"EXEC GetItemsForRunner @User='{users}', @Status='Collect'";
            using var cmdd = new SqlCommand(sqld, connsd);
            SqlDataReader result_clear = cmdd.ExecuteReader();

            List<RunnerClass> items = new List<RunnerClass>();

            while (result_clear.Read())
            {
                RunnerClass item = new RunnerClass();
                item.sku_id = result_clear["sku_id"].ToString();
                item.item_description = result_clear["item_description"].ToString();
                item.item_price = (decimal)result_clear["item_price"];
                item.UPC = (decimal)result_clear["UPC"];
                item.departmentDesc = result_clear["departmentDesc"].ToString();
                item.subDepartmentDesc = result_clear["subDepartmentDesc"].ToString();
                item.classDesc = result_clear["classDesc"].ToString();
                item.subClassDesc = result_clear["subClassDesc"].ToString();
                item.runnerUser = result_clear["runnerUser"].ToString();
                item.Quantity = (int)result_clear["Quantity"];
                item.item_image = result_clear["item_image"].ToString();
                item.CollectedQty = (int)result_clear["CollectedQty"];
                item.collectingStartTime = result_clear["collectingStartTime"].ToString() == "" ? null : (DateTime?)result_clear["collectingStartTime"];
                item.collectingEndTime = result_clear["collectingEndTime"].ToString() == "" ? null : (DateTime?)result_clear["collectingEndTime"];
                item.typeOfexception = result_clear["typeOfexception"].ToString();
                item.onhand = (int)result_clear["onhand"];
                items.Add(item);


            }


            connsd.Close();

            //var items = new List<OrderClass>();
            //items = _userInfoConn.ordersTable.Where(e => e.typeOfexception == "NIB").ToList();


            return Json(new { set = items });
        }

        public JsonResult ScannedUPC(string upc, int sku)
        {

            var status = string.Empty;
            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            //var cs = "Data Source=199.84.0.151;Initial Catalog=SNR_ECOMMERCE;User ID=apps;Password=546@Apps#88";
            //using var conns = new SqlConnection(cs);
            //conns.Open();


            //string Upc = $"SELECT UPC FROM [SNR_ECOMMERCE].[dbo].[ItemUPC] Where SKU={sku} AND UPC = {upc} ";
            //using var cmddd = new SqlCommand(Upc, conns);
            //var upcresult = cmddd.ExecuteScalar();

            ////var items = new List<OrderClass>();
            ////items = _userInfoConn.ordersTable.Where(e => e.typeOfexception == "NIB").ToList();

            //conns.Close();

            decimal? upcresult = null;
            var itemUPC = _userInfoConn.ItemUPC.Where(w => w.SKU == sku && w.UPC == decimal.Parse(upc)).FirstOrDefault();

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

            return Json(new { set = upcresult, status = status });
        }


        public JsonResult GetItemTransfer()
        {

            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var csd = _configuration.GetConnectionString("Myconnection");
            using var connsd = new SqlConnection(csd);
            connsd.Open();

            var status = String.Empty;

            var user = claims.Claims.ToList()[0].Value;
            string sqld = $"EXEC GetItemsForRunner @User='{user}', @Status='Transfer'";
            using var cmdd = new SqlCommand(sqld, connsd);
            SqlDataReader result_clear = cmdd.ExecuteReader();

            List<RunnerClass> items = new List<RunnerClass>();

            while (result_clear.Read())
            {
                RunnerClass item = new RunnerClass();
                item.sku_id = result_clear["sku_id"].ToString();
                item.item_description = result_clear["item_description"].ToString();
                item.item_price = (decimal)result_clear["item_price"];
                item.UPC = (decimal)result_clear["UPC"];
                item.departmentDesc = result_clear["departmentDesc"].ToString();
                item.subDepartmentDesc = result_clear["subDepartmentDesc"].ToString();
                item.classDesc = result_clear["classDesc"].ToString();
                item.subClassDesc = result_clear["subClassDesc"].ToString();
                item.runnerUser = result_clear["runnerUser"].ToString();
                item.Quantity = (int)result_clear["Quantity"];
                item.item_image = result_clear["item_image"].ToString();
                item.CollectedQty = (int)result_clear["CollectedQty"];
                item.inventoryLocation = result_clear["inventoryLocation"].ToString();
                item.transferLocation = result_clear["transferLocation"].ToString();
                item.transferringStartTime = result_clear["transferringStartTime"].ToString() == "" ? null : (DateTime?)result_clear["transferringStartTime"];
                item.transferringEndTime = result_clear["transferringEndTime"].ToString() == "" ? null : (DateTime?)result_clear["transferringEndTime"];
                item.typeOfexception = result_clear["typeOfexception"].ToString();
                items.Add(item);


            }



            connsd.Close();

            var itemComplete = new List<OrderClass>();
            var allItem = new List<OrderClass>();
            itemComplete = _userInfoConn.ordersTable.Where(e => e.runnerUser == user && e.runnerStatus == "Transfer" && e.transferLocation != null).ToList();
            allItem = _userInfoConn.ordersTable.Where(e => e.runnerUser == user && e.typeOfexception == "NIB").ToList();

            if (itemComplete.Count == allItem.Count)
            {
                status = "Complete";
            }
            else
            {
                status = "Incomplete";
            }

            //var items = new List<OrderClass>();
            //items = _userInfoConn.ordersTable.Where(e => e.typeOfexception == "NIB").ToList();


            return Json(new { set = items, status = status });
        }


        public JsonResult SaveItem(itemModel item)
        {

            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var user = claims.Claims.ToList()[0].Value;
            var ordersTable = new List<OrderClass>();
            ordersTable = _userInfoConn.ordersTable.Where(e => e.sku_id == item.SKU && e.runnerUser == user && e.exception == 1).ToList();
            for (int i = 0; i < item.QTY; i++)
            {

                ordersTable[i].runnerStatus = "Collected";
                _userInfoConn.Update(ordersTable[i]);
            }

            _userInfoConn.SaveChanges();



            return Json(new { set = ordersTable });
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
        public JsonResult NOF(itemModel item)
        {

            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var user = claims.Claims.ToList()[0].Value;
            var ordersTable = new List<OrderClass>();
            ordersTable = _userInfoConn.ordersTable.Where(e => e.sku_id == item.SKU && e.runnerUser == user).ToList();
            for (int i = 0; i < ordersTable.Count; i++)
            {
                //UPDATE INVENTORY TO OOS
                //ordersTable[i].runnerStatus = "";
                //ordersTable[i].runnerUser = "";
                ordersTable[i].typeOfexception = "NOF";
                ordersTable[i].NOFUser = user;
                _userInfoConn.Update(ordersTable[i]);
            }

            _userInfoConn.SaveChanges();


            var exceptionItems = new ExceptionItems();
            exceptionItems.orderId = ordersTable[0].orderId;
            exceptionItems.sku = item.SKU;
            exceptionItems.qty = ordersTable.Count;
            exceptionItems.user = user;
            exceptionItems.userType = "Runner";
            exceptionItems.typeOfException = "NOF";
            exceptionItems.dateProcess = DateTime.Now;
            _userInfoConn.Add(exceptionItems);
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
            ordersTable2 = _userInfoConn.ordersTable.Where(e => e.runnerStatus == "Collected" && e.runnerUser == user && e.exception == 1).ToList();
            var dateNow = DateTime.Now;
            for (int x = 0; x < ordersTable2.Count; x++)
            {
                //UPDATE INVENTORY TO OOS
                ordersTable2[x].runnerStatus = "Transfer";
                ordersTable2[x].collectingEndTime = dateNow;
                ordersTable2[x].transferringStartTime = dateNow;
                _userInfoConn.Update(ordersTable2[x]);


                
            }
            _userInfoConn.SaveChanges();

            var ordersTable3 = new List<OrderClass>();
            ordersTable3 = _userInfoConn.ordersTable.Where(e => e.runnerStatus == "Collect" && e.runnerUser == user && e.typeOfexception == "NOF").ToList();
            for (int x = 0; x < ordersTable3.Count; x++)
            {
                //UPDATE INVENTORY TO OOS
                ordersTable3[x].runnerUser = null;
                ordersTable3[x].runnerStatus = null;
                ordersTable3[x].collectingStartTime = null;
                _userInfoConn.Update(ordersTable3[x]);
            }
            _userInfoConn.SaveChanges();

            string HasNoOrders = string.Empty;
            var ordersTableCollect = new List<OrderClass>();
            ordersTableCollect = _userInfoConn.ordersTable.Where(e => e.runnerStatus == "Collect" && e.runnerUser == user && e.exception == 1).ToList();
            var ordersTableCollected = new List<OrderClass>();
            ordersTableCollected = _userInfoConn.ordersTable.Where(e => e.runnerStatus == "Collected" && e.runnerUser == user && e.exception == 1).ToList();
            var ordersTableTransfer = new List<OrderClass>();
            ordersTableTransfer = _userInfoConn.ordersTable.Where(e => e.runnerStatus == "Transfer" && e.runnerUser == user && e.exception == 1).ToList();
            var ordersTableTransferred = new List<OrderClass>();
            ordersTableTransferred = _userInfoConn.ordersTable.Where(e => e.runnerStatus == "Transfer" && e.runnerUser == user && e.exception == 1 && e.transferLocation != null).ToList();

            if (ordersTableCollect.Count < 1 && ordersTableCollected.Count < 1 && ordersTableTransfer.Count < 1 && ordersTableTransferred.Count < 1)
            {
                HasNoOrders = "Yes";
            }
            else
            {
                HasNoOrders = "No";
            }


            return Json(new { set = ordersTable2, nof = ordersTable3, hasNoOrders = HasNoOrders });
        }

        public JsonResult ItemTransferred(itemModel item)
        {

            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var user = claims.Claims.ToList()[0].Value;
            var ordersTable = new List<OrderClass>();
            var ordersTableHeader = new OrderHeaderClass();
            ordersTable = _userInfoConn.ordersTable.Where(e => e.runnerStatus == "Transfer" && e.runnerUser == user && e.transferLocation != null).ToList();

            var clearedOrders = new ClearedOrders();
            var dateNow = DateTime.Now;
            for (int i = 0; i < ordersTable.Count; i++)
            {
                //UPDATE INVENTORY TO OOS
                ordersTable[i].runnerStatus = "Transferred";
                ordersTable[i].typeOfexception = "";
                ordersTable[i].exception = 0;
                ordersTable[i].transferringEndTime = dateNow;
                _userInfoConn.Update(ordersTable[i]);


                var csd = "Data Source=199.84.17.95;Initial Catalog=EcommerceHub;User ID=apps;Password=546@Apps#88;";
                using var connsd = new SqlConnection(csd);
                connsd.Open();

                string sql_ecom = $"UPDATE [dbo].[Inventories] SET OnHand = ISNULL(OnHand, 0) + 1 WHERE SKU = '{ordersTable[i].sku_id}'";
                using var cmd_ecom = new SqlCommand(sql_ecom, connsd);
                var result_ecom = cmd_ecom.ExecuteScalar();
                connsd.Close();
            }
            _userInfoConn.SaveChanges();

            for (int i = 0; i < ordersTable.Count; i++)
            {

                var noExceptionOrders = new List<OrderClass>();
                noExceptionOrders = _userInfoConn.ordersTable.Where(e => e.exception == 1 && e.orderId == ordersTable[i].orderId).ToList();
                if (noExceptionOrders.Count < 1)
                {
                    var existInCleared = new List<ClearedOrders>();
                    existInCleared = _userInfoConn.clearedOrders.Where(e => e.orderId == ordersTable[i].orderId && e.skuId == ordersTable[i].sku_id).ToList();
                    if (existInCleared.Count > 0)
                    {

                        var existInCleared2 = new List<ClearedOrders>();
                        existInCleared2 = _userInfoConn.clearedOrders.Where(e => e.orderId == ordersTable[i].orderId && e.pickerStatus == "Picked").ToList();
                        if (existInCleared2.Count > 0)
                        {
                            for (int l = 0; l < existInCleared.Count; l++)
                            {
                                existInCleared[l].isFromNIB = true;
                                existInCleared[l].isNIB = false;
                                existInCleared[l].pickerUser = null;
                                _userInfoConn.Update(existInCleared[l]);
                            }

                        }
                        else
                        {
                            for (int l = 0; l < existInCleared.Count; l++)
                            {
                                existInCleared[l].isFromNIB = false;
                                existInCleared[l].isNIB = false;
                                existInCleared[l].pickerUser = null;
                                _userInfoConn.Update(existInCleared[l]);
                            }

                        }

                    }
                    else
                    {
                        

                        var shopee_csd = _configuration.GetConnectionString("Myconnection");
                        using var shopee_connsd = new SqlConnection(shopee_csd);
                        shopee_connsd.Open();

                        string sqld1_add = $"INSERT INTO clearedOrders (deductedStockEcom, deductedStock2017, dateProcess, skuId, orderId, module, processBy, isFreeItem, isNIB, isFromNIB)  SELECT 1, 1, GETDATE(), sku_id, orderId, module, 'System', 0, 0, 0 FROM ordersTable WHERE orderId = '{ordersTable[i].orderId}'  AND platform_status <> 'canceled'";
                        using var cmdd1_add = new SqlCommand(sqld1_add, shopee_connsd);
                        cmdd1_add.ExecuteNonQuery();
                        shopee_connsd.Close();
                    }


                }

            }
            _userInfoConn.SaveChanges();
            for (int i = 0; i < ordersTable.Count; i++)
            {
                var noExceptionOrders = new List<OrderClass>();
                noExceptionOrders = _userInfoConn.ordersTable.Where(e => e.exception == 1 && e.orderId == ordersTable[i].orderId).ToList();
                if (noExceptionOrders.Count < 1)
                {
                    ordersTableHeader = _userInfoConn.orderTableHeader.Where(e => e.orderId == ordersTable[i].orderId).FirstOrDefault();
                    ordersTableHeader.exception = 0;
                    ordersTableHeader.status = "";

                    _userInfoConn.Update(ordersTableHeader);


                }
            }

            _userInfoConn.SaveChanges();


            string HasNoOrders = string.Empty;
            var ordersTableCollect = new List<OrderClass>();
            ordersTableCollect = _userInfoConn.ordersTable.Where(e => e.runnerStatus == "Collect" && e.runnerUser == user && e.exception == 1).ToList();
            var ordersTableCollected = new List<OrderClass>();
            ordersTableCollected = _userInfoConn.ordersTable.Where(e => e.runnerStatus == "Collected" && e.runnerUser == user && e.exception == 1).ToList();
            var ordersTableTransfer = new List<OrderClass>();
            ordersTableTransfer = _userInfoConn.ordersTable.Where(e => e.runnerStatus == "Transfer" && e.runnerUser == user && e.exception == 1).ToList();
            var ordersTableTransferred = new List<OrderClass>();
            ordersTableTransferred = _userInfoConn.ordersTable.Where(e => e.runnerStatus == "Transfer" && e.runnerUser == user && e.exception == 1 && e.transferLocation != null).ToList();

            if (ordersTableCollect.Count < 1 && ordersTableCollected.Count < 1 && ordersTableTransfer.Count < 1 && ordersTableTransferred.Count < 1)
            {
                HasNoOrders = "Yes";
            }
            else
            {
                HasNoOrders = "No";
            }



            return Json(new { set = ordersTable, hasNoOrders = HasNoOrders });
        }

    }






}
