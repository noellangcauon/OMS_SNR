using AspNetCore.ReportingServices.ReportProcessing.ReportObjectModel;
using Infrastructure.External.ShopeeWebApi;
using Lazop.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Serilog.Context;
using SNR_BGC.Models;
using SNR_BGC.Utilities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Microsoft.AspNetCore.Hosting;
using Serilog.Core;
using SNR_BGC.DataAccess;
using System.Text;
using System.Drawing;
using Newtonsoft.Json;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using SNR_BGC.Hubs;
using System.Threading;
using Microsoft.AspNetCore.Authentication;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Http;

namespace SNR_BGC.Controllers
{
    public class BoxerController : Controller
    {
        private readonly UserClass _userInfoConn;
        private readonly IHubContext<ChatHub, IChatHub> _chatHub;

        private readonly IConfiguration _configuration;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ShopeeController> _logger;
        private readonly IAuthenthicationTokenProvider _authenthicationTokenProvider;
        private IHostApplicationLifetime _hostApplicationLifetime;
        private CancellationToken cancellationToken;
        private readonly IAsyncPolicy _policy;
        private readonly IServiceScope _scope;
        private readonly IDbAccess _dataAccess;
        private readonly IDbAccess _dbAccess;


        public BoxerController(UserClass userinfo, IConfiguration configuration,
            UserClass tokenInfo,
            IWebHostEnvironment webHostEnvironment,
            ILogger<ShopeeController> logger,
            IHttpClientFactory httpClientFactory,
            IAsyncPolicy policy,
            IAuthenthicationTokenProvider authenthicationTokenProvider,
            IHostApplicationLifetime hostApplicationLifetime,
            IServiceScopeFactory factory
            , IDataRepository dataRepository, IDbAccess dataAccess, IHubContext<ChatHub, IChatHub> chatHub, IDbAccess dbAccess)

        {
            _userInfoConn = userinfo;
            _configuration = configuration;


            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            _dataAccess = dataAccess;

            _userInfoConn = tokenInfo;
            _chatHub = chatHub;
            _logger = logger;

            _authenthicationTokenProvider = authenthicationTokenProvider;

            this._hostApplicationLifetime = hostApplicationLifetime;

            _httpClientFactory = httpClientFactory;

            _policy = policy;

            _scope = factory.CreateScope();
            _dbAccess = dbAccess;

        }
        public IActionResult Index(string result)
        {
            return View();

        }

        public IActionResult WBItems()
        {
            return View();
        }

        public JsonResult GetWBItems()
        {
            try
            {
                var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
                var user = claims.Claims.ToList()[0].Value;
                //var user = "Dcualing@snrshopping.com";

                IEnumerable<WBItemsClass> items = new List<WBItemsClass>();
                items = _dbAccess.ExecuteSP2<WBItemsClass, dynamic>("sp_GetWBItems", new { user });
                return Json(new { set = items });
            }
            catch (Exception ex)
            {

            }
            return Json(new { set = "" });

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

                if (ordersTable.Count() == ordersTable2.Count())
                {

                    item.orderId = orderNum[j];
                    item.status = "badge-success";
                    orderList.Add(item);

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

        public JsonResult ScannedUPC(string upc)
        {

            var status = string.Empty;
            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            //var cs = "Data Source=199.84.0.151;Initial Catalog=SNR_ECOMMERCE;User ID=apps;Password=546@Apps#88";
            //using var conns = new SqlConnection(cs);
            //conns.Open();


            //string Upc = $"SELECT UPC FROM [SNR_ECOMMERCE].[dbo].[ItemUPC] Where UPC = {upc} ";
            //using var cmddd = new SqlCommand(Upc, conns);
            //var upcresult = cmddd.ExecuteScalar();

            //var items = new List<OrderClass>();
            //items = _userInfoConn.ordersTable.Where(e => e.typeOfexception == "NIB").ToList();

            if (IsNotInteger(upc))
            {
                return Json(new { status = "Wrong" });
            }

            var itemUPC = _userInfoConn.ItemUPC.Where(w => w.UPC == decimal.Parse(upc)).FirstOrDefault();

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


                    //string Upc2 = $"SELECT UPC FROM [SNR_ECOMMERCE].[dbo].[ItemUPC] Where UPC = {convertedUPC} ";
                    //using var cmddd2 = new SqlCommand(Upc2, conns2);
                    //upcresult = cmddd2.ExecuteScalar();
                    //conns2.Close();

                    itemUPC = _userInfoConn.ItemUPC.Where(w => w.UPC == decimal.Parse(convertedUPC)).FirstOrDefault();

                }
            }
            if (itemUPC == null)
            {
                status = "Wrong";
            }
            else
            {
                status = "Correct";
            }

            //conns.Close();
            return Json(new { status = status });
        }


        public JsonResult SaveScan(string upc, string orderId, int qty)
        {
            int Threshold = Convert.ToInt32(_configuration["Threshold"]);
            if (qty > Threshold)
            {

                return Json(new { status = "MaxThreshold" });
            }
            try
            {
                string exist;
                var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
                //var cs = "Data Source=199.84.0.151;Initial Catalog=SNR_ECOMMERCE;User ID=apps;Password=546@Apps#88";
                //using var conns = new SqlConnection(cs);
                //conns.Open();


                var user = claims.Claims.ToList()[0].Value;
                //string sku = $"SELECT SKU FROM [SNR_ECOMMERCE].[dbo].[ItemUPC] Where UPC = {upc} ";
                //using var cmddd = new SqlCommand(sku, conns);
                //var skuresult = cmddd.ExecuteScalar();

                //conns.Close();

                decimal? skuresult = null;
                var itemUPC = _userInfoConn.ItemUPC.Where(w => w.UPC == decimal.Parse(upc)).FirstOrDefault();

                string convertedUPC = string.Empty;
                if (itemUPC == null)
                {
                    BarcodeConverter barcodeConverter = new BarcodeConverter();
                    convertedUPC = barcodeConverter.UPCConverter(upc.ToString());
                    if (convertedUPC != "")
                    {
                        var claims2 = (System.Security.Claims.ClaimsIdentity)User.Identity;
                        //var cs2 = "Data Source=199.84.0.151;Initial Catalog=SNR_ECOMMERCE;User ID=apps;Password=546@Apps#88";
                        //using var conns2 = new SqlConnection(cs2);
                        //conns2.Open();


                        //string sku2 = $"SELECT SKU FROM [SNR_ECOMMERCE].[dbo].[ItemUPC] Where UPC = {convertedUPC} ";
                        //using var cmddd2 = new SqlCommand(sku2, conns2);
                        //skuresult = cmddd2.ExecuteScalar();
                        //conns2.Close();

                        itemUPC = _userInfoConn.ItemUPC.Where(w => w.UPC == decimal.Parse(convertedUPC)).FirstOrDefault();
                    }
                }




                //var items = new List<OrderClass>();
                //items = _userInfoConn.ordersTable.Where(e => e.typeOfexception == "NIB").ToList();


                if (itemUPC == null)
                {
                    return Json(new { status = "NotExistBarcode" });
                }
                else
                    skuresult = itemUPC.SKU;

                var orderItem = new ClearedOrders();
                orderItem = _userInfoConn.clearedOrders.Where(e => e.orderId == orderId && e.skuId == skuresult.ToString()).FirstOrDefault();
                var orderTable = _userInfoConn.ordersTable.Where(e => e.orderId == orderId && e.sku_id == skuresult.ToString()).FirstOrDefault();
                exist = "Existing";

                decimal upcInt = 0;
                try
                {
                    upcInt = decimal.Parse(upc);
                    Console.WriteLine("Parsed integer value: " + upcInt);
                }
                catch (FormatException)
                {
                    Console.WriteLine("The input string is not a valid integer.");
                }
                if (orderItem == null)
                {
                    for (var i = 0; i < qty; i++)
                    {
                        var boxItem2 = new BoxOrders();
                        boxItem2.reserveId = 0;
                        boxItem2.skuId = skuresult.ToString();
                        boxItem2.orderId = orderId;
                        boxItem2.module = "None";
                        boxItem2.processBy = "None";
                        boxItem2.dateProcess = DateTime.Now;
                        boxItem2.boxerStatus = "Wrong";
                        boxItem2.boxerUser = user;
                        boxItem2.boxerStartTime = DateTime.Now;
                        boxItem2.isScanned = true;
                        boxItem2.UPC = upcInt;
                        _userInfoConn.Add(boxItem2);
                        _userInfoConn.SaveChanges();
                    }
                    return Json(new { set = orderId, status = "Existing" });
                }

                for (var i = 0; i < qty; i++)
                {

                    var boxItem = new BoxOrders();
                    boxItem.reserveId = orderItem.reserveId;
                    boxItem.skuId = orderItem.skuId;
                    boxItem.orderId = orderId;
                    boxItem.module = orderItem.module;
                    boxItem.processBy = orderItem.processBy;
                    boxItem.dateProcess = DateTime.Now;
                    boxItem.boxerStatus = "Correct";
                    boxItem.boxerUser = user;
                    boxItem.boxerStartTime = DateTime.Now;
                    boxItem.isScanned = true;
                    boxItem.UPC = upcInt;
                    boxItem.order_item_id = orderTable != null ? orderTable.order_item_id : null;
                    //_userInfoConn.SaveChanges();
                    _userInfoConn.Add(boxItem);
                }


                //var csd1 = _configuration.GetConnectionString("Myconnection");
                //using var connsd1 = new SqlConnection(csd1);
                //connsd1.Open();


                //string sqld1 = $"EXEC GetOrderForBoxer @OrderId='{orderId}'";
                //using var cmdd1 = new SqlCommand(sqld1, connsd1);
                //SqlDataReader result_clear1 = cmdd1.ExecuteReader();

                //List<BoxerClass> items1 = new List<BoxerClass>();

                //while (result_clear1.Read())
                //{
                //    BoxerClass item = new BoxerClass();
                //    item.skuId = result_clear1["skuId"].ToString();
                //    item.item_description = result_clear1["item_description"].ToString();
                //    item.item_price = (decimal)result_clear1["item_price"];
                //    item.UPC = (decimal)result_clear1["UPC"];
                //    item.item_image = result_clear1["item_image"].ToString();
                //    item.transferLocation = result_clear1["transferLocation"].ToString();
                //    item.module = result_clear1["module"].ToString();
                //    item.inventoryLocation = result_clear1["inventoryLocation"].ToString();
                //    item.item_image = result_clear1["item_image"].ToString();
                //    item.ScannedQty = (int)result_clear1["ScannedQty"];
                //    item.Quantity = (int)result_clear1["Quantity"];
                //    item.OrdersQuantity = (int)result_clear1["OrdersQuantity"];
                //    item.TotalOrdersQuantity = (int)result_clear1["TotalOrdersQuantity"];
                //    item.boxerStartTime = result_clear1["boxerStartTime"].ToString() == "" ? null : (DateTime?)result_clear1["boxerStartTime"];
                //    item.boxerEndTime = result_clear1["boxerEndTime"].ToString() == "" ? null : (DateTime?)result_clear1["boxerEndTime"];
                //    items1.Add(item);


                //}


                //connsd1.Close();

                _userInfoConn.SaveChanges();
                return Json(new { set = orderId, status = "Existing" });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }





        public JsonResult ToPOS(string orderId)
        {



            //var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            //var user = claims.Claims.ToList()[0].Value;
            //var boxOrders = new List<BoxOrders>();
            //boxOrders = _userInfoConn.boxOrders.Where(e => e.orderId == orderId).ToList();
            //var dateNow = DateTime.Now;
            //for (var i = 0; i < boxOrders.Count(); i++)
            //{

            //    boxOrders[i].boxerEndTime = dateNow;
            //    boxOrders[i].boxerStatus = "Discrepancy";
            //    _userInfoConn.Update(boxOrders[i]);


            //    _userInfoConn.SaveChanges();

            //}
            var result_clear = string.Empty;
            var csd = _configuration.GetConnectionString("Myconnection");
            using var connsd = new SqlConnection(csd);
            connsd.Open();

            string sqld = $"EXEC InsertDiscrepancyReport @OrderId='{orderId}'";
            using var cmdd = new SqlCommand(sqld, connsd);
            cmdd.ExecuteScalar();
            connsd.Close();


            return Json(new { set = result_clear });
        }


        public JsonResult SupervisorCreds(string id, string pin, string result)
        {



            var supervisorTables = new SupervisorTable();
            supervisorTables = _userInfoConn.SupervisorTable.Where(e => e.supervisorId == id && e.pin == pin).FirstOrDefault();


            return Json(new { set = supervisorTables });
        }


        public async Task<JsonResult> RePrintWaybill(string id, string pin, string result)
        {


            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var user = claims.Claims.ToList()[0].Value;
            var latestDate = new DateTime?();
            latestDate = _userInfoConn.boxOrders.Where(e => e.boxerUser == user).ToList().Max(e => e.boxerEndTime);
            var boxOrders = new List<BoxOrders>();
            boxOrders = _userInfoConn.boxOrders.Where(e => e.boxerUser == user && e.boxerEndTime == latestDate).ToList();
            var supervisorTables = new SupervisorTable();
            supervisorTables = _userInfoConn.SupervisorTable.Where(e => e.supervisorId == id && e.pin == pin).FirstOrDefault();
            var dateNow = DateTime.Now;




            if (supervisorTables.id > 0)
            {
                var reprintWaybillTables = new ReprintWaybillTable();

                reprintWaybillTables.supervisorId = id;
                reprintWaybillTables.boxerUser = user;
                reprintWaybillTables.orderId = boxOrders[0].orderId;
                reprintWaybillTables.module = boxOrders[0].module;
                reprintWaybillTables.printerName = result;
                reprintWaybillTables.transactionDate = DateTime.Now;
                _userInfoConn.Add(reprintWaybillTables);
                _userInfoConn.SaveChanges();
                var shopee_csd = _configuration.GetConnectionString("Myconnection");
                using var shopee_connsd = new SqlConnection(shopee_csd);
                shopee_connsd.Open();

                string shopee_sql_token = "SELECT TOP 1 accessToken FROM shopeeToken order by entryId Desc";
                using var shopee_cmd_token = new SqlCommand(shopee_sql_token, shopee_connsd);
                var access_token = shopee_cmd_token.ExecuteScalar();


                string accessToken = access_token.ToString();
                long partnerId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:PartnerId"),
                    shopId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:ShopId");
                string partnerKey = _configuration["Infrastructure:ShopeeApi:v2:PartnerKey"] ?? "";
                string apiUrl = string.Empty;
                if (boxOrders[0].module == "lazada")
                {
                    var printerExe = new printerExeClass();
                    printerExe = _userInfoConn.printerExe.Where(e => e.printer == result && e.platform == boxOrders[0].module).FirstOrDefault();

                    apiUrl = $"http://199.84.17.110:195/api/OrderPrint/GetOrderPrint?orderPrint={boxOrders[0].orderId}&printerName={result}&module={boxOrders[0].module}&filepath={printerExe.filepath}";
                }
                else
                {
                    var printerExe = new printerExeClass();
                    printerExe = _userInfoConn.printerExe.Where(e => e.printer == result && e.platform == boxOrders[0].module).FirstOrDefault();

                    apiUrl = $"http://199.84.17.110:195/api/OrderPrint/GetOrderPrint?orderPrint={boxOrders[0].orderId}&printerName={result}&module={boxOrders[0].module}&accessToken={accessToken}&partnerKey={partnerKey}&partnerKey={partnerKey}&partnerId={partnerId}&shopId={shopId}&filepath={printerExe.filepath}";
                }
                using (HttpClient client = new HttpClient())
                {
                    // Make a GET request
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        // Read and display the response content
                        string content = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(content);

                        reprintWaybillTables.isSuccess = true;
                        _userInfoConn.Update(reprintWaybillTables);
                        _userInfoConn.SaveChanges();
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");

                        reprintWaybillTables.isSuccess = false;
                        _userInfoConn.Update(reprintWaybillTables);
                        _userInfoConn.SaveChanges();
                    }
                }

                shopee_connsd.Close();

            }


            return Json(new { set = supervisorTables });
        }


        public async Task<JsonResult> GetOrderId()
        {

            var message = string.Empty;
            var orderId = string.Empty;
            var module = string.Empty;
            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var user = claims.Claims.ToList()[0].Value;
            var latestDate = new DateTime?();
            latestDate = _userInfoConn.boxOrders.Where(e => e.boxerUser == user).ToList().Max(e => e.boxerEndTime);
            var boxOrders = new List<BoxOrders>();
            boxOrders = _userInfoConn.boxOrders.Where(e => e.boxerUser == user && e.boxerEndTime == latestDate).ToList();
            if (boxOrders.Count > 0)
            {
                orderId = boxOrders[0].orderId;
                module = boxOrders[0].module;



                bool firstPrint = _userInfoConn.SettingsTable.ToList().Max(x => x.firstReprint);

                //if (module == "lazada")
                //{
                if (firstPrint)
                {
                    var reprintTable = new List<ReprintWaybillTable>();
                    reprintTable = _userInfoConn.ReprintWaybillTable.Where(e => e.orderId == orderId).ToList();

                    if (reprintTable.Count < 1)
                    {

                        await RePrintNoSupervisor(orderId);
                        message = "Exist";
                    }
                    else
                    {
                        message = "NotExist";
                    }
                }
                else
                {
                    message = "NotExist";
                }
                //}
                //else
                //{


                //    //string apiCheckExistUrl = $"https://localhost:7078/api/OrderPrint/GetCheckIfFileExist?orderId={boxOrders[0].orderId}";
                //    string apiCheckExistUrl = $"http://199.84.17.110:195/api/OrderPrint/GetCheckIfFileExist?orderId={boxOrders[0].orderId}";
                //    using (HttpClient client = new HttpClient())
                //    {
                //        // Make a GET request
                //        HttpResponseMessage response = await client.GetAsync(apiCheckExistUrl);

                //        if (response.IsSuccessStatusCode)
                //        {
                //            // Read and display the response content
                //            string content = await response.Content.ReadAsStringAsync();


                //            if (content == "Exist")
                //            {
                //                await RePrintNoSupervisor(orderId);
                //                message = content;
                //            }
                //            else if (content == "NotExist")
                //            {
                //                message = content;
                //            }


                //        }
                //        else
                //        {
                //            Console.WriteLine($"Error: {response.StatusCode}");

                //        }
                //    }

                //}

            }






            return Json(new { orderId = orderId, module = module, message = message });
        }

        public async Task<string> RePrintNoSupervisor(string orderId)
        {


            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var user = claims.Claims.ToList()[0].Value;
            var boxOrders = new List<BoxOrders>();
            boxOrders = _userInfoConn.boxOrders.Where(e => e.orderId == orderId).ToList();
            var dateNow = DateTime.Now;





            var reprintWaybillTables = new ReprintWaybillTable();

            reprintWaybillTables.supervisorId = "NoSupervisor";
            reprintWaybillTables.boxerUser = user;
            reprintWaybillTables.orderId = boxOrders[0].orderId;
            reprintWaybillTables.module = boxOrders[0].module;
            reprintWaybillTables.printerName = boxOrders[0].printerName;
            reprintWaybillTables.transactionDate = DateTime.Now;
            _userInfoConn.Add(reprintWaybillTables);
            _userInfoConn.SaveChanges();
            var shopee_csd = _configuration.GetConnectionString("Myconnection");
            using var shopee_connsd = new SqlConnection(shopee_csd);
            shopee_connsd.Open();

            string shopee_sql_token = "SELECT TOP 1 accessToken FROM shopeeToken order by entryId Desc";
            using var shopee_cmd_token = new SqlCommand(shopee_sql_token, shopee_connsd);
            var access_token = shopee_cmd_token.ExecuteScalar();


            string accessToken = access_token.ToString();
            long partnerId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:PartnerId"),
                shopId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:ShopId");
            string partnerKey = _configuration["Infrastructure:ShopeeApi:v2:PartnerKey"] ?? "";
            string apiUrl = string.Empty;
            if (boxOrders[0].module == "lazada")
            {
                var printerExe = new printerExeClass();
                printerExe = _userInfoConn.printerExe.Where(e => e.printer == boxOrders[0].printerName && e.platform == boxOrders[0].module).FirstOrDefault();

                apiUrl = $"http://199.84.17.110:195/api/OrderPrint/GetOrderPrint?orderPrint={boxOrders[0].orderId}&printerName={boxOrders[0].printerName}&module={boxOrders[0].module}&filepath={printerExe.filepath}";
            }
            else
            {
                var printerExe = new printerExeClass();
                printerExe = _userInfoConn.printerExe.Where(e => e.printer == boxOrders[0].printerName && e.platform == boxOrders[0].module).FirstOrDefault();
                apiUrl = $"http://199.84.17.110:195/api/OrderPrint/GetOrderPrint?orderPrint={boxOrders[0].orderId}&printerName={boxOrders[0].printerName}&module={boxOrders[0].module}&accessToken={accessToken}&partnerKey={partnerKey}&partnerKey={partnerKey}&partnerId={partnerId}&shopId={shopId}&filepath={printerExe.filepath}";
            }
            using (HttpClient client = new HttpClient())
            {
                // Make a GET request
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    // Read and display the response content
                    string content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(content);

                    reprintWaybillTables.isSuccess = true;
                    _userInfoConn.Update(reprintWaybillTables);
                    _userInfoConn.SaveChanges();
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");

                    reprintWaybillTables.isSuccess = false;
                    _userInfoConn.Update(reprintWaybillTables);
                    _userInfoConn.SaveChanges();
                }
            }

            shopee_connsd.Close();



            return "Success";
        }

        public JsonResult CheckDiscrepancy(string orderId)
        {
            var csd = _configuration.GetConnectionString("Myconnection");
            using var connsd = new SqlConnection(csd);
            connsd.Open();


            string sqld = $"EXEC CheckOrderFromClearedToBoxer @OrderId='{orderId}'";
            using var cmdd = new SqlCommand(sqld, connsd);
            var result_clear = cmdd.ExecuteScalar();

            connsd.Close();
            return Json(new { set = result_clear });
        }

        public async Task<JsonResult> DoneBoxer(CancellationToken stoppingToken, string orderId, string result)
        
        {
            var shopee_csd = _configuration.GetConnectionString("Myconnection");
            using var shopee_connsd = new SqlConnection(shopee_csd);
            shopee_connsd.Open();

            var csd = _configuration.GetConnectionString("Myconnection");
            using var connsd = new SqlConnection(csd);
            connsd.Open();

            try
            {
                var canceledOrders = new List<CanceledOrders>();
                canceledOrders = _userInfoConn.CanceledOrders.Where(e => e.orderId == orderId).ToList();
                if (canceledOrders.Count() > 0)
                {
                    var cs = _configuration.GetConnectionString("Myconnection");
                    using var conns = new SqlConnection(cs);
                    conns.Open();


                    string Upc = $"EXEC InsertCancelledOrders @orderId='{orderId}'";
                    using var cmddd = new SqlCommand(Upc, conns);
                    var upcresult = cmddd.ExecuteScalar();

                    conns.Close();


                    connsd.Close();
                    shopee_connsd.Close();
                    return Json(new { set = "CancelledOrders" });
                }

                

                string shopee_sql_token = "SELECT TOP 1 accessToken FROM shopeeToken order by entryId Desc";
                using var shopee_cmd_token = new SqlCommand(shopee_sql_token, shopee_connsd);
                var access_token = shopee_cmd_token.ExecuteScalar();



                var url = _configuration["LazadaInfrastructure:url"];
                var appkey = _configuration["LazadaInfrastructure:appkey"];
                var appSecret = _configuration["LazadaInfrastructure:appSecret"];
                var accessToken = _userInfoConn.tokenTable.Select(t_code => t_code.access_token).FirstOrDefault();


                long partnerId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:PartnerId"),
                    shopId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:ShopId");


                var module = _userInfoConn.boxOrders.Where(f => f.orderId == orderId).Select(e => e.module).FirstOrDefault();
                List<string> oderItem = new List<string>();
                IEnumerable<OrdersTableViewModel> orderItem = new List<OrdersTableViewModel>();

                orderItem = _dataAccess.ExecuteSP2<OrdersTableViewModel, dynamic>("GetOrderItemId", new { orderId });

                foreach (var item in orderItem)
                {
                    oderItem.Add(item.order_item_id);
                }

                //string package_id = string.Empty;
                string[] orderItemId = oderItem.ToArray();
                string itemIds = string.Join(",", orderItemId);
                var result_clear = "";
                if (module == "lazada")
                {
                    var obj = string.Empty;
                    var shipper = _userInfoConn.ordersTable.Where(f => f.orderId == orderId).Select(e => e.shipment_provider).FirstOrDefault();

                    //ILazopClient client = new LazopClient(url, appkey, appSecret);
                    //LazopRequest request = new LazopRequest();
                    //request.SetApiName("/order/pack");
                    //request.AddApiParameter("shipping_provider", shipper);
                    //request.AddApiParameter("delivery_type", "dropship");
                    //request.AddApiParameter("order_item_ids", "[" + itemIds + "]");
                    //LazopResponse response = client.Execute(request, accessToken);
                    //Console.WriteLine(response.IsError());
                    //obj = response.Body;
                    //JObject responseJson = JObject.Parse(obj);
                    //Console.WriteLine(response.Body);

                    ILazopClient client = new LazopClient(url, appkey, appSecret);
                    LazopRequest request = new LazopRequest();
                    request.SetApiName("/order/fulfill/pack");
                    request.AddApiParameter("packReq", "{\"pack_order_list\":[{\"order_item_list\":[" + itemIds + "],\"order_id\":" + orderId + "}],\"delivery_type\":\"dropship\",\"shipping_allocate_type\":\"TFS\"}");
                    LazopResponse response = client.Execute(request, accessToken);
                    Console.WriteLine(response.IsError());
                    obj = response.Body;
                    JObject responseJson = JObject.Parse(obj);
                    Console.WriteLine(response.Body);


                    var errorLogs = new ErrorLogs();

                    errorLogs.orderId = orderId;
                    errorLogs.Logs = response.Body;
                    errorLogs.date = DateTime.Now;

                    _userInfoConn.Add(errorLogs);
                    _userInfoConn.SaveChanges();


                    if (responseJson["code"].ToString() == "0")
                    {
                        var boxOrderTracking = new List<BoxOrders>();
                        boxOrderTracking = _userInfoConn.boxOrders.Where(e => e.orderId == orderId).ToList();
                        JArray orderItemListArray = (JArray)responseJson["result"]["data"]["pack_order_list"][0]["order_item_list"];
                        List<OrderItemListClass> orderItemList = orderItemListArray.ToObject<List<OrderItemListClass>>();
                        for (int x = 0; x < boxOrderTracking.Count; x++)
                        {
                            if (string.IsNullOrEmpty(boxOrderTracking[x].order_item_id))
                            {
                                boxOrderTracking[x].trackingNo = responseJson["result"]["data"]["pack_order_list"][0]["order_item_list"][x]["tracking_number"].ToString();
                                boxOrderTracking[x].package_id = responseJson["result"]["data"]["pack_order_list"][0]["order_item_list"][x]["package_id"].ToString();
                                _userInfoConn.Update(boxOrderTracking[x]);

                                //package_id = responseJson["result"]["data"]["pack_order_list"][0]["order_item_list"][x]["package_id"].ToString();
                            }
                            else
                            {
                                boxOrderTracking[x].trackingNo = orderItemList.Where(w => w.order_item_id == boxOrderTracking[x].order_item_id).Select(s => s.tracking_number).FirstOrDefault();
                                boxOrderTracking[x].package_id = orderItemList.Where(w => w.order_item_id == boxOrderTracking[x].order_item_id).Select(s => s.package_id).FirstOrDefault();
                                _userInfoConn.Update(boxOrderTracking[x]);
                            }
                        }
                        _userInfoConn.SaveChanges();
                    }


                    if (responseJson["code"].ToString() != "0")
                    {
                        var errorLogs2 = new List<ErrorLogs>();
                        errorLogs2 = _userInfoConn.ErrorLogs.Where(e => e.orderId == orderId).ToList();
                        bool isAlreadyPack = false;
                        foreach (var errorLog2 in errorLogs2)
                        {
                            if (errorLog2.Logs.Contains("\"code\":\"0\","))
                            {
                                isAlreadyPack = true;
                            }
                        }

                        if (isAlreadyPack)
                        {
                            var boxOrderWithTracking = new List<BoxOrders>();
                            boxOrderWithTracking = _userInfoConn.boxOrders.Where(e => e.orderId == orderId).ToList();
                            string arrayResult = "{\"packages\":[";

                            var distinctPackageIds = boxOrderWithTracking.Select(b => b.package_id).Distinct();

                            foreach (var packageId in distinctPackageIds)
                            {
                                arrayResult += "{\"package_id\":\"" + packageId + "\"},";
                            }

                            arrayResult = arrayResult.TrimEnd(',') + "]}";

                            //for (int i = 0; i < boxOrderWithTracking.Count; i++)
                            //{
                            //    arrayResult = arrayResult + "{\"package_id\":\"" + boxOrderWithTracking[i].package_id + "\"}" + (i == boxOrderWithTracking.Count - 1 ? "]}" : ",");


                            //}

                            ILazopClient client2 = new LazopClient(url, appkey, appSecret);
                            LazopRequest request2 = new LazopRequest();
                            request2.SetApiName("/order/package/rts");
                            //request2.AddApiParameter("readyToShipReq", "{\"packages\":[{\"package_id\":"+ package_id +"}]}");
                            request2.AddApiParameter("readyToShipReq", arrayResult);
                            LazopResponse response2 = client2.Execute(request2, accessToken);
                            Console.WriteLine(response2.IsError());
                            Console.WriteLine(response2.Body);


                            string sqld2 = $"EXEC DoneBoxerInsertPOS @OrderId='{orderId}', @ShopeePartnerID='', @ShopeeShopID='', @PrinterName='{result}',@Token='', @PartnerKey=''";
                            using var cmdd2 = new SqlCommand(sqld2, connsd);
                            result_clear = (cmdd2.ExecuteScalar()).ToString();



                            string apiUrl2 = string.Empty;

                            var printerExe = new printerExeClass();
                            printerExe = _userInfoConn.printerExe.Where(e => e.printer == result && e.platform == module).FirstOrDefault();

                            apiUrl2 = $"http://199.84.17.110:195/api/OrderPrint/GetOrderPrint?orderPrint={orderId}&printerName={result}&module={module}&filepath={printerExe.filepath}";


                            using (HttpClient client3 = new HttpClient())
                            {
                                // Make a GET request
                                HttpResponseMessage response3 = await client3.GetAsync(apiUrl2);

                                if (response3.IsSuccessStatusCode)
                                {
                                    // Read and display the response content
                                    string content = await response3.Content.ReadAsStringAsync();
                                    Console.WriteLine(content);

                                }
                                else
                                {
                                    Console.WriteLine($"Error: {response3.StatusCode}");
                                }
                            }
                        }
                        else
                        {
                            connsd.Close();
                            shopee_connsd.Close();
                            return Json(new
                            {
                                set = "Failed"
                            });
                        }



                    }
                    else
                    {
                        var boxOrderWithTracking = new List<BoxOrders>();
                        boxOrderWithTracking = _userInfoConn.boxOrders.Where(e => e.orderId == orderId).ToList();
                        string arrayResult = "{\"packages\":[";

                        var distinctPackageIds = boxOrderWithTracking.Select(b => b.package_id).Distinct();

                        foreach (var packageId in distinctPackageIds)
                        {
                            arrayResult += "{\"package_id\":\"" + packageId + "\"},";
                        }

                        arrayResult = arrayResult.TrimEnd(',') + "]}";
                        //for (int i = 0; i < boxOrderWithTracking.Count; i++)
                        //{
                        //    arrayResult = arrayResult + "{\"package_id\":\"" + boxOrderWithTracking[i].package_id + "\"}" + (i == boxOrderWithTracking.Count - 1 ? "]}" : ",");


                        //}

                        ILazopClient client2 = new LazopClient(url, appkey, appSecret);
                        LazopRequest request2 = new LazopRequest();
                        request2.SetApiName("/order/package/rts");
                        //request2.AddApiParameter("readyToShipReq", "{\"packages\":[{\"package_id\":"+ package_id +"}]}");
                        request2.AddApiParameter("readyToShipReq", arrayResult);
                        LazopResponse response2 = client2.Execute(request2, accessToken);
                        Console.WriteLine(response2.IsError());
                        Console.WriteLine(response2.Body);


                        string logs = response2.Body;

                        if (logs.Contains("40010"))
                        {
                            ILazopClient client3 = new LazopClient(url, appkey, appSecret);
                            LazopRequest request3 = new LazopRequest();
                            request3.SetApiName("/order/package/rts");
                            request3.AddApiParameter("readyToShipReq", arrayResult);
                            LazopResponse response3 = client3.Execute(request3, accessToken);
                            Console.WriteLine(response3.IsError());
                            Console.WriteLine(response3.Body);

                            string logs2 = response3.Body;

                            if (logs2.Contains("40010"))
                            {
                                ILazopClient client4 = new LazopClient(url, appkey, appSecret);
                                LazopRequest request4 = new LazopRequest();
                                request4.SetApiName("/order/package/rts");
                                request4.AddApiParameter("readyToShipReq", arrayResult);
                                LazopResponse response4 = client4.Execute(request4, accessToken);
                                Console.WriteLine(response4.IsError());
                                Console.WriteLine(response4.Body);


                                string logs3 = response4.Body;
                                if (logs3.Contains("40010"))
                                {
                                    connsd.Close();
                                    shopee_connsd.Close();
                                    return Json(new
                                    {
                                        set = "Failed",
                                        hasSystemError = "SystemError"
                                    });
                                }
                            }

                        }

                    }
                    //test
                }
                else if (module == "shopee")
                {
                    string resultApi = await GetShipmentList(access_token.ToString(), orderId);

                    if (resultApi == "Failed")
                    {
                        Thread.Sleep(3000);
                        string resultApi2 = await GetShipmentList(access_token.ToString(), orderId);

                        if (resultApi2 == "Failed")
                        {
                            string trackingNo = await GetShopeeTrackingNo(access_token.ToString(), orderId);
                            if (string.IsNullOrEmpty(trackingNo) || string.IsNullOrWhiteSpace(trackingNo))
                            {
                                connsd.Close();
                                shopee_connsd.Close();
                                return Json(new
                                {
                                    set = "TrackingNumberNotFound"
                                });
                            }
                        }
                    }
                }


                var partnerKey = _configuration["Infrastructure:ShopeeApi:v2:PartnerKey"] ?? "";

                string sqld = $"EXEC DoneBoxerInsertPOS @OrderId='{orderId}', @ShopeePartnerID='{shopId}', @ShopeeShopID='{shopId}', @PrinterName='{result}',@Token='{access_token.ToString()}', @PartnerKey='{partnerKey}'";
                using var cmdd = new SqlCommand(sqld, connsd);
                result_clear = (cmdd.ExecuteScalar()).ToString();

                string apiUrl = string.Empty;
                if (module == "lazada")
                {
                    var printerExe = new printerExeClass();
                    printerExe = _userInfoConn.printerExe.Where(e => e.printer == result && e.platform == module).FirstOrDefault();

                    apiUrl = $"http://199.84.17.110:195/api/OrderPrint/GetOrderPrint?orderPrint={orderId}&printerName={result}&module={module}&filepath={printerExe.filepath}";

                }
                else
                {
                    var printerExe = new printerExeClass();
                    printerExe = _userInfoConn.printerExe.Where(e => e.printer == result && e.platform == module).FirstOrDefault();

                    apiUrl = $"http://199.84.17.110:195/api/OrderPrint/GetOrderPrint?orderPrint={orderId}&printerName={result}&module={module}&accessToken={access_token.ToString()}&partnerKey={partnerKey}&partnerId={partnerId}&shopId={shopId}&filepath={printerExe.filepath}";

                }
                using (HttpClient client = new HttpClient())
                {
                    // Make a GET request
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        // Read and display the response content
                        string content = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(content);

                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                    }
                }




                //var boxOrders = new List<BoxOrders>();
                //boxOrders = _userInfoConn.boxOrders.Where(e => e.orderId == orderId).ToList();
                //for(int i = 0; i < boxOrders.Count(); i++)
                //{
                //    var cs_ecom = "Data Source=199.84.17.95;Initial Catalog=EcommerceHub;User ID=apps;Password=546@Apps#88;";
                //    using var conns_ecom = new SqlConnection(cs_ecom);
                //    conns_ecom.Open();

                //    string sql_ecom = $"UPDATE [EcommerceHub].[dbo].[Inventories] SET OnHand = OnHand - 1 WHERE Sku = {boxOrders[i].skuId}";
                //    using var cmd_ecom = new SqlCommand(sql_ecom, conns_ecom);
                //    var result_ecom = cmd_ecom.ExecuteScalar();
                //    conns_ecom.Close();
                //}


                //boxerLogs = new BoxerLogs();
                //boxerLogs.orderId = orderId;
                //boxerLogs.logs = "step 9";
                //boxerLogs.dateProcess = DateTime.Now;
                //boxerLogs.printername = result;
                //boxerLogs.module = "";
                //boxerLogs.response = "";
                //boxerLogs.partnerId = "";
                //boxerLogs.shopId = "";
                //boxerLogs.access_token = "";
                //boxerLogs.partnerKey = "";
                //boxerLogs.filepath = "";
                //_userInfoConn.Add(boxerLogs);
                //_userInfoConn.SaveChanges();

                connsd.Close();
                shopee_connsd.Close();

                return Json(new
                {
                    set = result_clear
                });
            }
            catch (Exception ex)
            {
                //var boxerLogs = new BoxerLogs();
                //boxerLogs.orderId = orderId;
                //boxerLogs.logs = "DoneBoxer Exception: " + ex.Message;
                //boxerLogs.dateProcess = DateTime.Now;
                //boxerLogs.printername = result;
                //boxerLogs.module = "";
                //boxerLogs.response = "";
                //boxerLogs.partnerId = "";
                //boxerLogs.shopId = "";
                //boxerLogs.access_token = "";
                //boxerLogs.partnerKey = "";
                //boxerLogs.filepath = "";
                //_userInfoConn.Add(boxerLogs);
                //_userInfoConn.SaveChanges();

                connsd.Close();
                shopee_connsd.Close();

                return Json(new
                {
                    set = "Exception"
                });
            }
        }


        public async Task<JsonResult> DoneBoxerAlready(CancellationToken stoppingToken, string orderId, string result)
        {


            var csd = _configuration.GetConnectionString("Myconnection");
            using var connsd = new SqlConnection(csd);
            connsd.Open();

            var result_clear = "";

            string sqld = $"EXEC DoneBoxerInsertPOSAlready @OrderId='{orderId}', @PrinterName='{result}'";
            using var cmdd = new SqlCommand(sqld, connsd);
            result_clear = (cmdd.ExecuteScalar()).ToString();
            connsd.Close();




            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var user = claims.Claims.ToList()[0].Value;
            var boxOrders = new List<BoxOrders>();
            boxOrders = _userInfoConn.boxOrders.Where(e => e.orderId == orderId).ToList();



            var reprintWaybillTables = new ReprintWaybillTable();

            reprintWaybillTables.supervisorId = "AlreadyInPOS";
            reprintWaybillTables.boxerUser = user;
            reprintWaybillTables.orderId = orderId;
            reprintWaybillTables.module = boxOrders[0].module;
            reprintWaybillTables.printerName = result;
            reprintWaybillTables.transactionDate = DateTime.Now;
            _userInfoConn.Add(reprintWaybillTables);
            _userInfoConn.SaveChanges();

            var shopee_csd = _configuration.GetConnectionString("Myconnection");
            using var shopee_connsd = new SqlConnection(shopee_csd);
            shopee_connsd.Open();

            string shopee_sql_token = "SELECT TOP 1 accessToken FROM shopeeToken order by entryId Desc";
            using var shopee_cmd_token = new SqlCommand(shopee_sql_token, shopee_connsd);
            var access_token = shopee_cmd_token.ExecuteScalar();

            string accessToken = access_token.ToString();
            long partnerId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:PartnerId"),
                shopId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:ShopId");
            string partnerKey = _configuration["Infrastructure:ShopeeApi:v2:PartnerKey"] ?? "";
            string apiUrl = string.Empty;
            if (boxOrders[0].module == "lazada")
            {
                var printerExe = new printerExeClass();
                printerExe = _userInfoConn.printerExe.Where(e => e.printer == result && e.platform == boxOrders[0].module).FirstOrDefault();


                apiUrl = $"http://199.84.17.110:195/api/OrderPrint/GetOrderPrint?orderPrint={orderId}&printerName={result}&module={boxOrders[0].module}&filepath={printerExe.filepath}";
            }
            else
            {
                var printerExe = new printerExeClass();
                printerExe = _userInfoConn.printerExe.Where(e => e.printer == result && e.platform == boxOrders[0].module).FirstOrDefault();
                apiUrl = $"http://199.84.17.110:195/api/OrderPrint/GetOrderPrint?orderPrint={orderId}&printerName={result}&module={boxOrders[0].module}&accessToken={accessToken}&partnerKey={partnerKey}&partnerKey={partnerKey}&partnerId={partnerId}&shopId={shopId}&filepath={printerExe.filepath}";
            }
            using (HttpClient client = new HttpClient())
            {
                // Make a GET request
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    // Read and display the response content
                    string content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(content);

                    reprintWaybillTables.isSuccess = true;
                    _userInfoConn.Update(reprintWaybillTables);
                    _userInfoConn.SaveChanges();
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");

                    reprintWaybillTables.isSuccess = false;
                    _userInfoConn.Update(reprintWaybillTables);
                    _userInfoConn.SaveChanges();
                }
            }



            shopee_connsd.Close();


            return Json(new
            {
                set = result_clear
            });
        }

        public JsonResult LoadScanned(string orderId)
        {
            try
            {
                //    var status = string.Empty;
                //    var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
                //    var user = claims.Claims.ToList()[0].Value;
                var csd = _configuration.GetConnectionString("Myconnection");
                using var connsd = new SqlConnection(csd);
                connsd.Open();


                string sqld = $"EXEC GetOrderForBoxer @OrderId='{orderId}'";
                using var cmdd = new SqlCommand(sqld, connsd);
                SqlDataReader result_clear = cmdd.ExecuteReader();

                List<BoxerClass> items = new List<BoxerClass>();

                while (result_clear.Read())
                {
                    BoxerClass item = new BoxerClass();
                    item.skuId = result_clear["skuId"].ToString();
                    item.orderId = result_clear["orderId"].ToString();
                    item.item_description = result_clear["item_description"].ToString();
                    item.item_price = result_clear["item_price"] as decimal? == null ? 0 : (decimal)result_clear["item_price"];
                    item.UPC = result_clear["UPC"] as decimal? == null ? 0 : (decimal)result_clear["UPC"];
                    item.item_image = result_clear["item_image"] == null ? "" : result_clear["item_image"].ToString();
                    item.transferLocation = result_clear["transferLocation"] == null ? "" : result_clear["transferLocation"].ToString();
                    item.module = result_clear["module"] == null ? "" : result_clear["module"].ToString();
                    item.inventoryLocation = result_clear["inventoryLocation"] == null ? "" : result_clear["inventoryLocation"].ToString();
                    item.item_image = result_clear["item_image"] == null ? "" : result_clear["item_image"].ToString();
                    //item.ScannedQty = (int)result_clear["ScannedQty"];
                    item.Quantity = result_clear["Quantity"] as int? == null ? 0 : (int)result_clear["Quantity"];
                    //item.OrdersQuantity = (int)result_clear["OrdersQuantity"];
                    item.TotalOrdersQuantity = result_clear["TotalOrdersQuantity"] as int? == null ? 0 : (int)result_clear["TotalOrdersQuantity"];
                    item.boxerStartTime = result_clear["boxerStartTime"].ToString() == "" ? null : (DateTime?)result_clear["boxerStartTime"];
                    item.boxerEndTime = result_clear["boxerEndTime"].ToString() == "" ? null : (DateTime?)result_clear["boxerEndTime"];
                    items.Add(item);


                }


                connsd.Close();


                string isDisableToPOS = string.Empty;

                try
                {
                    var operationTimeOut = _userInfoConn.OperationTimeOut.FirstOrDefault();

                    TimeSpan currentTime = DateTime.Now.TimeOfDay;
                    if (currentTime > operationTimeOut.timeOutFrom && currentTime < operationTimeOut.timeOutTo)
                    {
                        isDisableToPOS = "Yes";
                    }
                    else
                    {
                        isDisableToPOS = "No";

                    }
                }
                catch (Exception ex)
                {

                }

                return Json(new { set = items, isDisableToPOS = isDisableToPOS });
            }
            catch (Exception ex)
            {
                throw ex;

            }
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



        public JsonResult DeleteItem(string orderId, string skuId)
        {

            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var user = claims.Claims.ToList()[0].Value;
            var dateProcess = new BoxOrders();
            dateProcess = _userInfoConn.boxOrders.Where(e => e.orderId == orderId && e.skuId == skuId).FirstOrDefault();


            var boxTable = new BoxOrders();

            boxTable = _userInfoConn.boxOrders.Where(e => e.orderId == orderId && e.skuId == skuId && e.dateProcess == dateProcess.dateProcess).FirstOrDefault();
            _userInfoConn.boxOrders.Remove(boxTable);
            _userInfoConn.SaveChanges();

            return Json(new { set = boxTable });
        }


        public async Task<string> GetShipmentList(string access_token, string orderId)
        {



            using (LogContext.PushProperty("Scope", "Shopee Api"))
            {



                var shopee_csd = _configuration.GetConnectionString("Myconnection");
                using var shopee_connsd = new SqlConnection(shopee_csd);
                shopee_connsd.Open();

                string urlGetShippingParam = _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:getShippingParam:Url"] ?? "";

                string urlGetTrackingNumber = _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:getTrackingNumber:Url"] ?? "";


                DateTime now = DateTime.Now;

                long timestamp = now.ToTimestamp();


                var packagenumber = _userInfoConn.orderTableHeader.Where(y => y.orderId == orderId).Select(x => x.package_number).FirstOrDefault();



                long partnerId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:PartnerId"),
                    shopId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:ShopId");

                var signGetShippingParam = ShopeeApiUtil.SignShopRequest(
                    partnerId: partnerId.ToString(),
                    apiPath: _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:getShippingParam:ApiPath"] ?? "",
                    timestamp: timestamp.ToString(),
                access_token: access_token,
                shopid: shopId,
                    partnerKey: _configuration["Infrastructure:ShopeeApi:v2:PartnerKey"] ?? "");



                var signGetTrackingNumber = ShopeeApiUtil.SignShopRequest(
                    partnerId: partnerId.ToString(),
                    apiPath: _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:getTrackingNumber:ApiPath"] ?? "",
                    timestamp: timestamp.ToString(),
                access_token: access_token,
                shopid: shopId,
                    partnerKey: _configuration["Infrastructure:ShopeeApi:v2:PartnerKey"] ?? "");




                using HttpClient client = _httpClientFactory.CreateClient(name: _configuration["Infrastructure:ShopeeApi:ConnectionName"] ?? "");

                string responseContentStringShippingParam = string.Empty;
                string responseContentStringTrackingNumber = string.Empty;

                Stopwatch stopwatch = Stopwatch.StartNew();
                try
                {
                    _logger.LogTrace($"{urlGetShippingParam}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&sign={signGetShippingParam}&shop_id={shopId}&order_sn={orderId}");
                    using HttpResponseMessage httpResponseShippingParam = await _policy.ExecuteAsync(async (ct) => await client.GetAsync(
                         requestUri: $"{urlGetShippingParam}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&sign={signGetShippingParam}&shop_id={shopId}&order_sn={orderId}",
                        cancellationToken: ct),
                        cancellationToken: cancellationToken);



                    responseContentStringShippingParam = await httpResponseShippingParam.Content.ReadAsStringAsync();

                    _logger.LogTrace(message: "Url:{Url}, Duration: {duration}s, Response Status: {responseStatus}, Responnse Body:{response}",
                        urlGetShippingParam,
                        (stopwatch.ElapsedMilliseconds / 1000m).ToString("#,##0.###"),
                        httpResponseShippingParam.StatusCode.ToString(),
                        responseContentStringShippingParam);

                    /////////////////////////////////start of fetching order details///////////////////////
                    var responseJsonShippingParam = JObject.Parse(responseContentStringShippingParam);
                    if (responseJsonShippingParam.ContainsKey("message"))
                    {
                        if (responseJsonShippingParam.ContainsKey("response"))
                        {
                            // Define your URL and parameters

                            timestamp = now.ToTimestamp();

                            using HttpClient client2 = new HttpClient();

                            var address_id = (int)(responseJsonShippingParam["response"]?["pickup"]?["address_list"]?[0]?["address_id"] ?? "");
                            var timeSlotCount = responseJsonShippingParam["response"]?["pickup"]?["address_list"]?[0]?["time_slot_list"].Count();
                            //var pickup_time_id = (responseJsonShippingParam["response"]?["pickup"]?["address_list"]?[0]?["time_slot_list"]?[timeSlotCount - 1]?["pickup_time_id"] ?? "").ToString();

                            var pickup_time_id = "";
                            var jsonPayload = "";
                            if (timeSlotCount > 0)
                            {
                                pickup_time_id = (responseJsonShippingParam["response"]?["pickup"]?["address_list"]?[0]?["time_slot_list"]?[0]?["pickup_time_id"] ?? "").ToString();

                                var array = new
                                {
                                    order_sn = orderId,
                                    pickup = new
                                    {
                                        address_id = address_id,
                                        pickup_time_id = pickup_time_id
                                    }
                                };

                                jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(array);
                            }
                            else
                            {
                                var array = new
                                {
                                    order_sn = orderId,
                                    pickup = new
                                    {
                                        address_id = address_id
                                    }
                                };

                                jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(array);
                            }
                                


                            string url2 = _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:ShipOrder:Url"] ?? "";



                            var sign2 = ShopeeApiUtil.SignShopRequest(
                                partnerId: partnerId.ToString(),
                                apiPath: _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:ShipOrder:ApiPath"] ?? "",
                                timestamp: timestamp.ToString(),
                            access_token: access_token,
                            shopid: shopId,
                                partnerKey: _configuration["Infrastructure:ShopeeApi:v2:PartnerKey"] ?? "");



                            string fullUrl = $"{url2}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&sign={sign2}&shop_id={shopId}";

                            // Execute the GET request with retries using the defined policy
                            //using HttpResponseMessage httpResponse2 = await _policy.ExecuteAsync(async (ct) => await client.GetAsync(fullUrl, cancellationToken: ct), cancellationToken: CancellationToken.None);

                            using HttpResponseMessage httpResponse2 = await _policy.ExecuteAsync(async (ct) =>
                            {
                                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                                return await client.PostAsync(fullUrl, content, ct);
                            }, CancellationToken.None);

                            // Check the response and handle it as needed
                            if (httpResponseShippingParam.IsSuccessStatusCode)
                            {
                                // Read the response content
                                string responseContent = await httpResponse2.Content.ReadAsStringAsync();
                                Console.WriteLine(responseContent);

                                var responseContentJson = JObject.Parse(responseContent);

                                var errorLogs = new ErrorLogs();

                                errorLogs.orderId = orderId;
                                errorLogs.Logs = responseContent;
                                errorLogs.date = DateTime.Now;

                                _userInfoConn.Add(errorLogs);
                                _userInfoConn.SaveChanges();


                                _logger.LogTrace($"{urlGetTrackingNumber}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&sign={signGetTrackingNumber}&shop_id={shopId}&order_sn={orderId}&package_number={packagenumber}");
                                using HttpResponseMessage httpResponseTrackingNumber = await _policy.ExecuteAsync(async (ct) => await client.GetAsync(
                                     requestUri: $"{urlGetTrackingNumber}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&sign={signGetTrackingNumber}&shop_id={shopId}&order_sn={orderId}&package_number={packagenumber}",
                                    cancellationToken: ct),
                                    cancellationToken: cancellationToken);

                                stopwatch.Stop();



                                responseContentStringTrackingNumber = await httpResponseTrackingNumber.Content.ReadAsStringAsync();
                                var responseJsonTrackingNumber = JObject.Parse(responseContentStringTrackingNumber);
                                if (responseJsonTrackingNumber.ContainsKey("message"))
                                {
                                    var trackingNumber = (responseJsonTrackingNumber["response"]?["tracking_number"] ?? "").ToString();


                                    var boxOrderTracking = new List<BoxOrders>();
                                    boxOrderTracking = _userInfoConn.boxOrders.Where(e => e.orderId == orderId).ToList();
                                    for (int x = 0; x < boxOrderTracking.Count; x++)
                                    {
                                        boxOrderTracking[x].trackingNo = trackingNumber;
                                        _userInfoConn.Update(boxOrderTracking[x]);
                                    }
                                    _userInfoConn.SaveChanges();
                                }

                                if (responseContentJson["error"].ToString() != "")
                                {
                                    shopee_connsd.Close();
                                    return "Failed";

                                }
                            }
                            else
                            {
                                // Handle the non-successful response
                                Console.WriteLine($"HTTP Error: {httpResponse2.StatusCode}");


                                var errorLogs = new ErrorLogs();

                                errorLogs.orderId = orderId;
                                errorLogs.Logs = $"HTTP Error: {httpResponse2.StatusCode}";
                                errorLogs.date = DateTime.Now;

                                _userInfoConn.Add(errorLogs);
                                _userInfoConn.SaveChanges();
                            }

                        }

                    }
                }


                catch (Exception ex)
                {

                    throw new ShopeeApiException("An error occured while accessing shopee API", ex)
                    {
                        RequestUrl = urlGetShippingParam,
                        RequestMethod = "POST",
                        Timestamp = now
                    };
                }
                //_userInfoConn.Dispose();
                shopee_connsd.Close();
                //shopee_connectionPos.Close();
            }

            return "Success";
        }


        public async Task<string> GetShopeeTrackingNo(string access_token, string orderId)
        {
            var trackingNumber = string.Empty;


            using (LogContext.PushProperty("Scope", "Shopee Api"))
            {



                var shopee_csd = _configuration.GetConnectionString("Myconnection");
                using var shopee_connsd = new SqlConnection(shopee_csd);
                shopee_connsd.Open();

                string urlInfo = _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:getTrackingInfo:Url"] ?? "";



                DateTime now = DateTime.Now;

                long timestamp = now.ToTimestamp();




                string urlGetTrackingNumber = _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:getTrackingNumber:Url"] ?? "";

                long partnerId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:PartnerId"),
                    shopId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:ShopId");


                var signGetTrackingNumber = ShopeeApiUtil.SignShopRequest(
                    partnerId: partnerId.ToString(),
                    apiPath: _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:getTrackingNumber:ApiPath"] ?? "",
                    timestamp: timestamp.ToString(),
                access_token: access_token,
                shopid: shopId,
                    partnerKey: _configuration["Infrastructure:ShopeeApi:v2:PartnerKey"] ?? "");








                using HttpClient client = _httpClientFactory.CreateClient(name: _configuration["Infrastructure:ShopeeApi:ConnectionName"] ?? "");

                string responseContentStringTrackingNumber = string.Empty;

                Stopwatch stopwatch = Stopwatch.StartNew();
                try
                {


                    _logger.LogTrace($"{urlGetTrackingNumber}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&sign={signGetTrackingNumber}&shop_id={shopId}&order_sn={orderId}");
                    using HttpResponseMessage httpResponseTrackingNumber = await _policy.ExecuteAsync(async (ct) => await client.GetAsync(
                         requestUri: $"{urlGetTrackingNumber}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&sign={signGetTrackingNumber}&shop_id={shopId}&order_sn={orderId}",
                        cancellationToken: ct),
                        cancellationToken: cancellationToken);

                    stopwatch.Stop();



                    responseContentStringTrackingNumber = await httpResponseTrackingNumber.Content.ReadAsStringAsync();
                    var responseJsonTrackingNumber = JObject.Parse(responseContentStringTrackingNumber);
                    if (responseJsonTrackingNumber.ContainsKey("message"))
                    {
                        trackingNumber = (responseJsonTrackingNumber["response"]?["tracking_number"] ?? "").ToString();


                        //var boxOrderTracking = new List<BoxOrders>();
                        //boxOrderTracking = _userInfoConn.boxOrders.Where(e => e.orderId == orderId).ToList();
                        //for (int x = 0; x < boxOrderTracking.Count; x++)
                        //{
                        //    boxOrderTracking[x].trackingNo = trackingNumber;
                        //    _userInfoConn.Update(boxOrderTracking[x]);
                        //}
                        //_userInfoConn.SaveChanges();
                    }

                }


                catch (Exception ex)
                {

                    throw new ShopeeApiException("An error occured while accessing shopee API", ex)
                    {
                        RequestUrl = urlInfo,
                        RequestMethod = "POST",
                        Timestamp = now
                    };
                }
                //_userInfoConn.Dispose();
                shopee_connsd.Close();
                //shopee_connectionPos.Close();
            }

            return trackingNumber;
        }



        public async Task<string> PlatformStatus(string orderId)
        {
            var status = string.Empty;


            using (LogContext.PushProperty("Scope", "Shopee Api"))
            {

                var shopee_csd = _configuration.GetConnectionString("Myconnection");
                using var shopee_connsd = new SqlConnection(shopee_csd);
                shopee_connsd.Open();

                string shopee_sql_token = "SELECT TOP 1 accessToken FROM shopeeToken order by entryId Desc";
                using var shopee_cmd_token = new SqlCommand(shopee_sql_token, shopee_connsd);
                var access_token = shopee_cmd_token.ExecuteScalar();



                string urlInfo = _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:GetOrderDetails:Url"] ?? "";


                DateTime now = DateTime.Now;

                long timestamp = now.ToTimestamp();





                long partnerId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:PartnerId"),
                    shopId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:ShopId");

                var signInfo = ShopeeApiUtil.SignShopRequest(
                    partnerId: partnerId.ToString(),
                    apiPath: _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:GetOrderDetails:ApiPath"] ?? "",
                    timestamp: timestamp.ToString(),
                access_token: access_token.ToString(),
                shopid: shopId,
                    partnerKey: _configuration["Infrastructure:ShopeeApi:v2:PartnerKey"] ?? "");








                using HttpClient client = _httpClientFactory.CreateClient(name: _configuration["Infrastructure:ShopeeApi:ConnectionName"] ?? "");

                string responseContentString = string.Empty;

                Stopwatch stopwatch = Stopwatch.StartNew();
                try
                {

                    string[] paramShopee = { "item_list", "buyer_username" };
                    _logger.LogTrace($"{urlInfo}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token.ToString()}&shop_id={shopId}&sign={signInfo}&order_sn_list={orderId}&response_optional_fields={paramShopee}");
                    string req = $"{urlInfo}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token.ToString()}&shop_id={shopId}&sign={signInfo}&order_sn_list={orderId}&response_optional_fields={paramShopee}";
                    Stopwatch stopwatchInfo = Stopwatch.StartNew();
                    using HttpResponseMessage httpResponseInfo = await _policy.ExecuteAsync(async (ct) => await client.GetAsync(
                    requestUri: $"{urlInfo}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token.ToString()}&shop_id={shopId}&sign={signInfo}&order_sn_list={orderId}&response_optional_fields={paramShopee}",
                    cancellationToken: ct),
                    cancellationToken: cancellationToken);


                    stopwatch.Stop();

                    responseContentString = await httpResponseInfo.Content.ReadAsStringAsync();

                    _logger.LogTrace(message: "Url:{Url}, Duration: {duration}s, Response Status: {responseStatus}, Responnse Body:{response}",
                        urlInfo,
                        (stopwatch.ElapsedMilliseconds / 1000m).ToString("#,##0.###"),
                        httpResponseInfo.StatusCode.ToString(),
                        responseContentString);

                    /////////////////////////////////start of fetching order details///////////////////////
                    var responseJson = JObject.Parse(responseContentString);


                    if (responseJson.ContainsKey("message"))
                    {
                        if (responseJson.ContainsKey("response"))
                        {
                            // Define your URL and parameters

                            JArray array = (JArray)(responseJson["response"]?["order_list"] ?? "");

                            // Deserialize the JSON into a List of objects
                            List<Dictionary<string, string>> list = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(array.ToString());

                            // Create a Dictionary with order_sn as the key and package_number as the value
                            Dictionary<string, string> result = list.ToDictionary(item => item["order_sn"], item => item["order_status"]);

                            status = result[orderId];




                        }

                    }
                }


                catch (Exception ex)
                {

                    throw new ShopeeApiException("An error occured while accessing shopee API", ex)
                    {
                        RequestUrl = urlInfo,
                        RequestMethod = "POST",
                        Timestamp = now
                    };
                }
                //_userInfoConn.Dispose();
                shopee_connsd.Close();
                //shopee_connectionPos.Close();
            }

            return status;
        }



    }


}
