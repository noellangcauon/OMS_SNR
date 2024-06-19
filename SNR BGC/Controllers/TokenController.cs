using Lazop.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using SNR_BGC.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using AspNetCore.Reporting;
using System.Data;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.VisualBasic;
using SNR_BGC.DataAccess;
using SNR_BGC.Utilities;
using System.Security.Policy;

namespace SNR_BGC.Controllers
{
    public class TokenController : Controller

    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly IConfiguration _configuration;

        private readonly int _pageSize;

        private readonly string _baseUrl;

        private readonly string _baseUrlorderDetails;

        private int _currentPage = 0;

        private readonly Models.UserClass _userInfoConn;
        private static readonly Models.UserClass _userInfoConn2;

        private readonly IDataRepository _dataRepository;
        private readonly IDbAccess _dataAccess;



        public TokenController(IConfiguration configuration, UserClass tokenInfo, IWebHostEnvironment webHostEnvironment, IDataRepository dataRepository, IDbAccess dataAccess)
        {
            _configuration = configuration;

            _webHostEnvironment = webHostEnvironment;

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            _pageSize = _configuration.GetValue<int>("ShopeeApi:v1:EndPoints:Item:GetItemsList:MaxPageSize");

            _baseUrl = _configuration["ShopeeApi:v1:EndPoints:Orders:GetOrderList:Url"];

            _baseUrlorderDetails = _configuration["ShopeeApi:v1:EndPoints:Orders:GetOrderDetails:Url"];


            _userInfoConn = tokenInfo;

            _dataRepository = dataRepository;

            _dataAccess = dataAccess;


        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult VerifyLazadaToken([FromQuery] string code)
        {
            var tinfo = new TokenClass();
            var obj = string.Empty;
            ILazopClient client = new LazopClient(
                serverUrl: "https://auth.lazada.com/rest",
                appKey: "107315",
                appSecret: "crIeEwrkG5pg0QsfbqiUSRzaRdQ9EPzJ");

            LazopRequest request = new LazopRequest("/auth/token/create");
            request.AddApiParameter("code", code);
            LazopResponse response = client.Execute(request);
            if (!response.IsError())
            {
                obj = response.Body;

                JObject responseJson = JObject.Parse(obj);

                tinfo.codeToGetToken = code;
                tinfo.access_token = responseJson["access_token"].ToString();
                tinfo.refresh_token = responseJson["refresh_token"].ToString();
                tinfo.country_user_id = responseJson["country_user_info"][0]["user_id"].ToString();
                tinfo.country_seller_id = responseJson["country_user_info"][0]["seller_id"].ToString();
                tinfo.country_short_code = responseJson["country_user_info"][0]["short_code"].ToString();
                tinfo.refresh_expires_in = responseJson["refresh_expires_in"].ToString();
                tinfo.access_expires_in = responseJson["expires_in"].ToString();
                tinfo.dateEntry = DateTime.Now;
                tinfo.module = "lazada";
                _userInfoConn.Add(tinfo);
                _userInfoConn.SaveChanges();


            }
            return View("GetNewToken");
        }
        public IActionResult VerifyToken()
        {
            var result = new List<TokenClass>();
            result = _userInfoConn.tokenTable.ToList();

            ViewBag.TokenInfo = result;
            return View();
        }
        //public JsonResult GetOrders([FromQuery] DateTime dateFrom, DateTime dateTo)
        //{
        //    var csd = _configuration.GetConnectionString("Myconnection");
        //    using var connsd = new SqlConnection(csd);
        //    connsd.Open();

        //    string sqld = $"EXEC DeleteNoHeader";
        //    using var cmdd = new SqlCommand(sqld, connsd);
        //    cmdd.ExecuteNonQuery();
        //    connsd.Close();

        //    var objCanceled = string.Empty;
        //    var obj = string.Empty;
        //    var url = _configuration["LazadaInfrastructure:url"];
        //    var appkey = _configuration["LazadaInfrastructure:appkey"];
        //    var appSecret = _configuration["LazadaInfrastructure:appSecret"];
        //    var order_id = string.Empty;
        //    var uname = string.Empty;

        //    var latestDate = new DateTime?();
        //    latestDate = _userInfoConn.orderTableHeader.Where(u => u.module == "lazada").ToList().Max(x => x.dateCreatedAt);



        //    dateTo = DateTime.Now;
        //    dateFrom = latestDate ?? DateTime.Today;

        //    dateFrom = dateFrom.AddMinutes(-10);



        //    var accessToken = _userInfoConn.tokenTable.Select(t_code => t_code.access_token).FirstOrDefault();

        //    //for Canceled Status

        //    ILazopClient clientCanceled = new LazopClient(url, appkey, appSecret);
        //    LazopRequest requestCanceled = new LazopRequest();
        //    requestCanceled.SetApiName("/orders/get");
        //    requestCanceled.SetHttpMethod("GET");
        //    requestCanceled.AddApiParameter("sort_direction", "DESC");
        //    requestCanceled.AddApiParameter("offset", "0");
        //    requestCanceled.AddApiParameter("limit", "100");
        //    requestCanceled.AddApiParameter("sort_by", "updated_at");
        //    /*request.AddApiParameter("created_after", finalDateFrom);*/
        //    requestCanceled.AddApiParameter("created_after", dateFrom.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
        //    requestCanceled.AddApiParameter("created_before", dateTo.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
        //    requestCanceled.AddApiParameter("status", "canceled");
        //    LazopResponse responseCanceled = clientCanceled.Execute(requestCanceled, accessToken);
        //    objCanceled = responseCanceled.Body;
        //    JObject responseJsonCanceled = JObject.Parse(objCanceled);


        //    for (int x = 0; x < responseJsonCanceled["data"]["orders"].Count(); x++)
        //    {
        //        var CanceledOrdersData = new List<CanceledOrders>();
        //        CanceledOrdersData = _userInfoConn.CanceledOrders.Where(e => e.orderId == responseJsonCanceled["data"]["orders"][x]["order_number"].ToString()).ToList();

        //        if (CanceledOrdersData.Count() < 1)
        //        {
        //            var canceledorder = new CanceledOrders();

        //            canceledorder.orderId = responseJsonCanceled["data"]["orders"][x]["order_number"].ToString();
        //            canceledorder.dateFetch = DateTime.Now;
        //            canceledorder.dateProcess = DateTime.Now;
        //            canceledorder.dateCreatedAt = (DateTime)responseJsonCanceled["data"]["orders"][x]["created_at"];
        //            canceledorder.module = "lazada";
        //            canceledorder.status = "canceled";
        //            canceledorder.item_count = (decimal)responseJsonCanceled["data"]["orders"][x]["items_count"];
        //            canceledorder.total_amount = (decimal)responseJsonCanceled["data"]["orders"][x]["price"];
        //            _userInfoConn.Add(canceledorder);
        //            _userInfoConn.SaveChanges();
        //        }

        //    }


        //    //for Pending Status

        //    ILazopClient client = new LazopClient(url, appkey, appSecret);
        //    LazopRequest request = new LazopRequest();
        //    request.SetApiName("/orders/get");
        //    request.SetHttpMethod("GET");
        //    request.AddApiParameter("sort_direction", "DESC");
        //    request.AddApiParameter("offset", "0");
        //    request.AddApiParameter("limit", "100");
        //    request.AddApiParameter("sort_by", "updated_at");
        //    /*request.AddApiParameter("created_after", finalDateFrom);*/
        //    request.AddApiParameter("created_after", dateFrom.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
        //    request.AddApiParameter("created_before", dateTo.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
        //    request.AddApiParameter("status", "pending");
        //    LazopResponse response = client.Execute(request, accessToken);
        //    obj = response.Body;
        //    JObject responseJson = JObject.Parse(obj);


        //    for (int x = 0; x < responseJson["data"]["orders"].Count(); x++)
        //    {


        //        order_id = responseJson["data"]["orders"][x]["order_number"].ToString();
        //        var orderHeaderCheck = new List<OrderHeaderClass>();
        //        orderHeaderCheck = _userInfoConn.orderTableHeader.Where(e => e.orderId == order_id).ToList();

        //        if (orderHeaderCheck.Count < 1)
        //        {

        //            SaveOrders(order_id);
        //        }
        //    }







        //    return Json(new { set = obj.ToString() });
        //}



        public JsonResult GetOrders([FromQuery] DateTime dateFrom, DateTime dateTo)
        {
            var ordersCount = 0;
            var coverage = dateFrom.ToString() + " - " + dateTo.ToString();
            int insertedEntity = 0;
            try
            {

                if (dateTo > DateTime.Now.AddMinutes(-30))
                {

                    return Json(new { set = "GreaterThanFrom4Hours" });
                }
                else
                if (dateFrom > dateTo)
                {

                    return Json(new { set = "DateFromIsGreaterThanDateTo" });
                }

                var autoReloadLogs = new AutoReloadLogs();
                autoReloadLogs.platform = "Lazada";
                autoReloadLogs.dateProcess = DateTime.Now;
                autoReloadLogs.status = "Good";
                autoReloadLogs.coverage = coverage;
                autoReloadLogs.agent = "Manual";
                _userInfoConn.Add(autoReloadLogs);
                _userInfoConn.SaveChanges();

                insertedEntity = autoReloadLogs.id;

                //var csd2 = _configuration.GetConnectionString("Myconnection");
                //using var connsd2 = new SqlConnection(csd2);
                //connsd2.Open();

                //string sqld2 = $"EXEC DeleteNoHeader @module= 'lazada'";
                //using var cmdd2 = new SqlCommand(sqld2, connsd2);
                //cmdd2.ExecuteNonQuery();
                //connsd2.Close();

                var objCanceled = string.Empty;
                var obj = string.Empty;
                var urlLaz = _configuration["LazadaInfrastructure:url"];
                var appkey = _configuration["LazadaInfrastructure:appkey"];
                var appSecret = _configuration["LazadaInfrastructure:appSecret"];
                var order_id = string.Empty;
                var uname = string.Empty;

                //var latestDate = new DateTime?();
                //var LazOrderHedear = new List<OrderHeaderClass>();
                //LazOrderHedear = _userInfoConn.orderTableHeader.Where(u => u.module == "lazada").ToList();


                //latestDate = _userInfoConn.orderTableHeader.Where(u => u.module == "lazada").ToList().Max(x => x.dateCreatedAt);



                //var orderIdsDone = _userInfoConn.boxOrders.Where(a => a.boxerStatus == "Done" && a.module == "lazada").Select(a => a.orderId).ToList();

                //var latestDateCancelled = _userInfoConn.orderTableHeader.Where(u => u.module == "lazada" && !orderIdsDone.Contains(u.orderId)).Min(x => x.dateCreatedAt);


                //dateTo = DateTime.Now;
                //dateFrom = latestDate ?? DateTime.Today;
                var dateFromCancelled = DateTime.Today.AddHours(-5);

                //dateFrom = dateFrom.AddMinutes(-10);



                var accessToken = _userInfoConn.tokenTable.Select(t_code => t_code.access_token).FirstOrDefault();

                //for Canceled Status

                ILazopClient clientCanceled = new LazopClient(urlLaz, appkey, appSecret);
                LazopRequest requestCanceled = new LazopRequest();
                requestCanceled.SetApiName("/orders/get");
                requestCanceled.SetHttpMethod("GET");
                requestCanceled.AddApiParameter("sort_direction", "DESC");
                requestCanceled.AddApiParameter("offset", "0");
                requestCanceled.AddApiParameter("limit", "100");
                requestCanceled.AddApiParameter("sort_by", "updated_at");
                /*request.AddApiParameter("created_after", finalDateFrom);*/
                //requestCanceled.AddApiParameter("created_after", dateFromCancelled.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                //requestCanceled.AddApiParameter("created_before", DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                requestCanceled.AddApiParameter("created_after", dateFrom.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                requestCanceled.AddApiParameter("created_before", dateTo.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                requestCanceled.AddApiParameter("status", "canceled");
                LazopResponse responseCanceled = clientCanceled.Execute(requestCanceled, accessToken);
                objCanceled = responseCanceled.Body;
                JObject responseJsonCanceled = JObject.Parse(objCanceled);


                for (int x = 0; x < responseJsonCanceled["data"]["orders"].Count(); x++)
                {
                    var orderId = responseJsonCanceled["data"]["orders"][x]["order_number"].ToString();
                    bool isItemCancelled = false;
                    var CanceledOrdersData = new List<CanceledOrders>();
                    CanceledOrdersData = _userInfoConn.CanceledOrders.Where(e => e.orderId == orderId).ToList();

                    if (CanceledOrdersData.Count() > 0)
                    {
                        var ordersTableForCancelled = new List<OrderClass>();
                        ordersTableForCancelled = _userInfoConn.ordersTable.Where(e => e.orderId == orderId && (e.typeOfexception == "NOF" || e.typeOfexception == "OOS")).ToList();

                        if (ordersTableForCancelled.Count > 0)
                        {
                            var cs = _configuration.GetConnectionString("Myconnection");
                            using var conns = new SqlConnection(cs);
                            conns.Open();


                            string Upc = $"EXEC InsertCancelledOrders @orderId='{orderId}'";
                            using var cmddd = new SqlCommand(Upc, conns);
                            var upcresult = cmddd.ExecuteScalar();

                            conns.Close();


                        }

                    }
                    else
                    {
                        var url = _configuration["LazadaInfrastructure:url"];

                        ILazopClient client = new LazopClient(url, appkey, appSecret);
                        LazopRequest requestDetails = new LazopRequest();
                        requestDetails.SetApiName("/order/items/get");
                        requestDetails.SetHttpMethod("GET");
                        requestDetails.AddApiParameter("order_id", orderId);
                        LazopResponse responseDetails = client.Execute(requestDetails, accessToken);
                        obj = responseDetails.Body;
                        JObject responseJson = JObject.Parse(obj);

                        int cnt = responseJson["data"].Count();

                        for (int i = 0; i < cnt; i++)
                        {
                            if (responseJson["data"][i]["status"].ToString() == "pending")
                            {
                                isItemCancelled = true;

                                break;
                            }

                        }

                        if (isItemCancelled == false)
                        {
                            var canceledorder = new CanceledOrders();

                            canceledorder.orderId = orderId;
                            canceledorder.dateFetch = DateTime.Now;
                            canceledorder.dateProcess = DateTime.Now;
                            canceledorder.dateCreatedAt = (DateTime)responseJsonCanceled["data"]["orders"][x]["created_at"];
                            canceledorder.module = "lazada";
                            canceledorder.status = "canceled";
                            canceledorder.item_count = (decimal)responseJsonCanceled["data"]["orders"][x]["items_count"];
                            canceledorder.total_amount = (decimal)responseJsonCanceled["data"]["orders"][x]["price"];
                            _userInfoConn.Add(canceledorder);
                            _userInfoConn.SaveChanges();


                            var cs = _configuration.GetConnectionString("Myconnection");
                            using var conns = new SqlConnection(cs);
                            conns.Open();


                            string Upc = $"EXEC InsertCancelledOrders @orderId='{orderId}'";
                            using var cmddd = new SqlCommand(Upc, conns);
                            var upcresult = cmddd.ExecuteScalar();

                            conns.Close();
                        }
                        else
                        {

                            var ordersTableExist = new List<OrderClass>();
                            ordersTableExist = _userInfoConn.ordersTable.Where(e => e.orderId == orderId).ToList();

                            var ordersTableHeaderExist = new List<OrderHeaderClass>();
                            ordersTableHeaderExist = _userInfoConn.orderTableHeader.Where(e => e.orderId == orderId).ToList();

                            if (ordersTableExist.Count > 0 && ordersTableHeaderExist.Count < 0)
                            {
                                for (int i = 0; i < cnt; i++)
                                {
                                    if (responseJson["data"][i]["status"].ToString() == "canceled")
                                    {

                                        var ordersTable = new OrderClass();
                                        ordersTable = _userInfoConn.ordersTable.Where(e => e.order_item_id == responseJson["data"][i]["order_item_id"].ToString()).FirstOrDefault();

                                        if(ordersTable.typeOfexception == "NIB")
                                        {
                                            ordersTable.platform_status = responseJson["data"][i]["status"].ToString();
                                            _userInfoConn.Update(ordersTable);
                                            _userInfoConn.SaveChanges();

                                        }
                                        else
                                        {
                                            ordersTable.platform_status = responseJson["data"][i]["status"].ToString();
                                            ordersTable.typeOfexception = "";
                                            ordersTable.exception = 0;
                                            _userInfoConn.Update(ordersTable);
                                            _userInfoConn.SaveChanges();

                                        }
                                        
                                    }


                                }

                                var ordersTableClear = new List<OrderClass>();
                                ordersTableClear = _userInfoConn.ordersTable.Where(e => e.orderId == orderId).ToList();
                                bool isCleared = true;

                                for (int i = 0; i < ordersTableClear.Count; i++)
                                {
                                    if (ordersTableClear[i].exception == 1)
                                    {
                                        isCleared = false;
                                    }
                                }

                                if (isCleared)
                                {
                                    var clearedTable = new List<ClearedOrders>();
                                    clearedTable = _userInfoConn.clearedOrders.Where(e => e.orderId == orderId).ToList();
                                    
                                    if (clearedTable.Count > 0)
                                    {

                                        var orderTableHeader = new OrderHeaderClass();
                                        orderTableHeader = _userInfoConn.orderTableHeader.Where(e => e.orderId == orderId).FirstOrDefault();
                                    }
                                    else
                                    {
                                        var cs = _configuration.GetConnectionString("Myconnection");
                                        using var conns = new SqlConnection(cs);
                                        conns.Open();

                                        string sqld1_add = $"INSERT INTO clearedOrders (deductedStockEcom, deductedStock2017, dateProcess, skuId, orderId, module, processBy, isFreeItem, isNIB, isFromNIB) SELECT 1, 1, GETDATE(), sku_id, orderId, 'shopee', 'System', 0, 0, 0 FROM ordersTable WHERE orderId = '{orderId}' AND platform_status <> 'canceled' ";
                                        using var cmdd1_add = new SqlCommand(sqld1_add, conns);
                                        cmdd1_add.ExecuteNonQuery();
                                        conns.Close();
                                    }

                                   


                                }

                            }
                            else
                            {

                                SaveOrders(orderId);
                            }
                        }
                    }

                }


                //for Pending Status

                //dateFrom = dateFrom.AddHours(-14);
                //dateTo = dateTo.AddHours(-11);
                ILazopClient clientLaz = new LazopClient(urlLaz, appkey, appSecret);
                LazopRequest request = new LazopRequest();
                request.SetApiName("/orders/get");
                request.SetHttpMethod("GET");
                request.AddApiParameter("sort_direction", "DESC");
                request.AddApiParameter("offset", "0");
                request.AddApiParameter("limit", "100");
                request.AddApiParameter("sort_by", "updated_at");
                /*request.AddApiParameter("created_after", finalDateFrom);*/
                request.AddApiParameter("update_after", dateFrom.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                request.AddApiParameter("update_before", dateTo.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                request.AddApiParameter("status", "pending");
                LazopResponse response = clientLaz.Execute(request, accessToken);
                obj = response.Body;
                JObject responseJsonLaz = JObject.Parse(obj);

                JObject responseJsonLaz2 = responseJsonLaz; // Initialize responseJsonLaz2 with responseJsonLaz

                if (responseJsonLaz["data"]["orders"].Count() == 100)
                {



                    for (int i = 100; i <= (int)responseJsonLaz["data"]["countTotal"]; i += 100)
                    {
                        request.SetApiName("/orders/get");
                        request.SetHttpMethod("GET");
                        request.AddApiParameter("sort_direction", "DESC");
                        request.AddApiParameter("offset", i.ToString());
                        request.AddApiParameter("limit", "100");
                        request.AddApiParameter("sort_by", "updated_at");
                        request.AddApiParameter("update_after", dateFrom.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                        request.AddApiParameter("update_before", dateTo.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                        request.AddApiParameter("status", "pending");
                        LazopResponse response2 = clientLaz.Execute(request, accessToken);
                        obj = response2.Body;
                        JObject responseJsonLaz2Part = JObject.Parse(obj);


                        // Merge the orders from responseJsonLaz2Part into responseJsonLaz2
                        foreach (JToken orderJson in responseJsonLaz2Part["data"]["orders"])
                        {
                            ((JArray)responseJsonLaz2["data"]["orders"]).Add(orderJson);
                        }

                    }


                }
                ordersCount = responseJsonLaz2["data"]["orders"].Count();

                for (int x = 0; x < responseJsonLaz["data"]["orders"].Count(); x++)
                {


                    order_id = responseJsonLaz["data"]["orders"][x]["order_number"].ToString();

                    var orderHeaderCheck = new List<OrderHeaderClass>();
                    orderHeaderCheck = _userInfoConn.orderTableHeader.Where(e => e.orderId == order_id).ToList();

                    if (orderHeaderCheck.Count < 1)
                    {

                        SaveOrders(order_id);
                    }
                }
            }
            catch (Exception ex)
            {
                var autoReloadLogs2 = _userInfoConn.AutoReloadLogs.FirstOrDefault(e => e.id == insertedEntity);

                if (autoReloadLogs2 != null)
                {
                    autoReloadLogs2.logs = ex.ToString();
                    autoReloadLogs2.status = "Failed";
                    autoReloadLogs2.totalOrder = ordersCount;
                    autoReloadLogs2.completion = DateTime.Now;

                    _userInfoConn.SaveChanges();
                }

                return Json(new { set = "Failed" });
            }


            var autoReloadLogs3 = _userInfoConn.AutoReloadLogs.FirstOrDefault(e => e.id == insertedEntity);

            if (autoReloadLogs3 != null)
            {
                autoReloadLogs3.logs = "Successful Run";
                autoReloadLogs3.status = "Success";
                autoReloadLogs3.totalOrder = ordersCount;
                autoReloadLogs3.completion = DateTime.Now;

                _userInfoConn.SaveChanges();
            }

            return Json(new { set = "Success" });

        }

        public JsonResult GetOrdersReprocess()
        {

            var csd = _configuration.GetConnectionString("Myconnection");
            using var connsd = new SqlConnection(csd);
            connsd.Open();

            string sqld = $"EXEC ReProcessException";
            using var cmdd = new SqlCommand(sqld, connsd);
            cmdd.ExecuteNonQuery();

            connsd.Close();
            return Json(new { data = "success" });
        }
        public JsonResult GetClearedOrders()
        {

            //var result = new List<OrderHeaderClass>();

            //result = _userInfoConn.orderTableHeader.Where(u => u.exception == 0 && u.module == "lazada").ToList();

            var latestDate = new DateTime?();
            latestDate = _userInfoConn.orderTableHeader.Where(u => u.module == "lazada").ToList().Max(x => x.dateCreatedAt);
            if (latestDate == null)
            {
                latestDate = DateTime.Today;
            }

            string Module = "lazada";
            int Exception = 0;


            IEnumerable<GridOrderHeaderClass> items = new List<GridOrderHeaderClass>();
            items = _dataAccess.ExecuteSP2<GridOrderHeaderClass, dynamic>("GetOrders", new { Module, Exception });
            return Json(new { set = items, maxDate = latestDate });

            //_userInfoConn.Dispose();

            //var csd = _configuration.GetConnectionString("Myconnection");
            //using var connsd = new SqlConnection(csd);
            //connsd.Open();


            //string sqld = $"EXEC GetOrders @Exception=0, @Module='lazada'";
            //using var cmdd = new SqlCommand(sqld, connsd);
            //SqlDataReader result_clear = cmdd.ExecuteReader();



            //List<GridOrderHeaderClass> items = new List<GridOrderHeaderClass>();

            //while (result_clear.Read())
            //{
            //    GridOrderHeaderClass item = new GridOrderHeaderClass();
            //    item.entryNum = (int)result_clear["entryNum"];
            //    item.orderId = result_clear["orderId"].ToString();
            //    item.dateFetch = result_clear["dateFetch"].ToString() == "" ? null : (DateTime?)result_clear["dateFetch"];
            //    item.dateProcess = result_clear["dateProcess"].ToString() == "" ? null : (DateTime?)result_clear["dateProcess"];
            //    item.dateCreatedAt = result_clear["dateCreatedAt"].ToString() == "" ? null : (DateTime?)result_clear["dateCreatedAt"];
            //    item.module = result_clear["module"].ToString();
            //    item.status = result_clear["status"].ToString();
            //    item.exception = (int)result_clear["exceptions_count"];
            //    item.customerID = result_clear["customerID"].ToString();
            //    item.employee = result_clear["employee"].ToString();
            //    item.item_count = (decimal)result_clear["item_count"];
            //    item.total_amount = (decimal)result_clear["total_amount"];
            //    item.userClear = result_clear["userClear"].ToString();

            //    item.tub_no = "";

            //    items.Add(item);


            //}


            //connsd.Close();



            //return Json(new { set = items, maxDate = latestDate });
        }
        public JsonResult GetExceptionsOrders()
        {

            //var result = new List<OrderHeaderClass>();

            //result = _userInfoConn.orderTableHeader.Where(u => u.exception == 1 && u.module == "lazada").ToList();



            //_userInfoConn.Dispose();


            var csd = _configuration.GetConnectionString("Myconnection");
            using var connsd = new SqlConnection(csd);
            connsd.Open();


            string sqld = $"EXEC GetOrders @Exception=1, @Module='lazada'";
            using var cmdd = new SqlCommand(sqld, connsd);
            SqlDataReader result_clear = cmdd.ExecuteReader();

            List<GridOrderHeaderClass> items = new List<GridOrderHeaderClass>();

            while (result_clear.Read())
            {
                GridOrderHeaderClass item = new GridOrderHeaderClass();
                item.entryNum = (int)result_clear["entryNum"];
                item.orderId = result_clear["orderId"].ToString();
                item.dateFetch = result_clear["dateFetch"].ToString() == "" ? null : (DateTime?)result_clear["dateFetch"];
                item.dateProcess = result_clear["dateProcess"].ToString() == "" ? null : (DateTime?)result_clear["dateProcess"];
                item.dateCreatedAt = result_clear["dateCreatedAt"].ToString() == "" ? null : (DateTime?)result_clear["dateCreatedAt"];
                item.module = result_clear["module"].ToString();
                item.status = result_clear["status"].ToString();
                item.exception = (int)result_clear["exception"];
                item.customerID = result_clear["customerID"].ToString();
                item.employee = result_clear["employee"].ToString();
                item.item_count = (decimal)result_clear["item_count"];
                item.total_amount = (decimal)result_clear["total_amount"];
                item.userClear = result_clear["userClear"].ToString();
                item.exceptions_count = (int)result_clear["exceptions_count"];
                item.tub_no = "";
                item.typeOfException = GetOrderItemsException(result_clear["orderId"].ToString());
                item.username = "";
                items.Add(item);


            }


            connsd.Close();


            return Json(new { set = items });
        }

        public JsonResult ViewOrders([FromQuery] DateTime dateFrom, DateTime dateTo, string filter)
        {

            var result = new List<OrderHeaderClass>();

            if (!String.IsNullOrEmpty(filter))
            {
                switch (filter)
                {
                    case "good":
                        //var results = _userInfoConn.ordersTable.Where(u => u.exemption == 0 && (u.dateProcess >= dateFrom && u.dateProcess <= dateTo) && u.module == "lazada").GroupBy(u => u.orderId, u => 1).Select(lazada => new { orderId = lazada.Key, count = lazada.Count() }).ToList();
                        result = _userInfoConn.orderTableHeader.Where(u => u.exception == 0 && (u.dateCreatedAt >= dateFrom && u.dateCreatedAt <= dateTo) && u.module == "lazada").ToList();
                        break;
                    case "bad":
                        result = _userInfoConn.orderTableHeader.Where(u => u.exception == 1 && (u.dateCreatedAt >= dateFrom && u.dateCreatedAt <= dateTo) && u.module == "lazada").ToList();
                        break;
                }

            }
            else
            {
                result = _userInfoConn.orderTableHeader.Where(u => (u.dateCreatedAt >= dateFrom && u.dateCreatedAt <= dateTo) && u.module == "lazada").ToList();
            }

            _userInfoConn.Dispose();
            return Json(new { set = result });
        }
        public IActionResult ViewOrdersLazada()
        {

            return View();
        }

        public JsonResult GetItemLazada([FromQuery] string order_id, string flag)
        {
            var result = new List<OrderClass>();
            result = _userInfoConn.ordersTable.Where(p => p.orderId == order_id && p.module == "lazada").ToList();
            return Json(new { set = result });


        }
        public JsonResult SaveOrders(string order_id)
        {
            var claimUser = User.Claims.FirstOrDefault(c => c.Type == "username");

            var obj = string.Empty;
            var url = _configuration["LazadaInfrastructure:url"];
            var appkey = _configuration["LazadaInfrastructure:appkey"];
            var appSecret = _configuration["LazadaInfrastructure:appSecret"];

            try
            {
                var custumerID = string.Empty;


                decimal total_amount = 0;
                int excep = 0;
                int cnt = 0;
                DateTime crt_date = new DateTime();

                var cs = "Data Source=199.84.0.151;Initial Catalog=SNR_ECOMMERCE;User ID=apps;Password=546@Apps#88";
                using var conns = new SqlConnection(cs);
                conns.Open();

                var csd = _configuration.GetConnectionString("Myconnection");
                using var connsd = new SqlConnection(csd);
                connsd.Open();

                var cs_ecom = "Data Source=199.84.17.95;Initial Catalog=EcommerceHub;User ID=apps;Password=546@Apps#88";
                using var conns_ecom = new SqlConnection(cs_ecom);
                conns_ecom.Open();

                //NpgsqlConnection connectionPos = new NpgsqlConnection("User ID=postgres;Password=postgres;Host=199.88.17.100;Port=5432;Database=tplinux;");

                //connectionPos.Open();


                var accessToken = _userInfoConn.tokenTable.Select(t_code => t_code.access_token).FirstOrDefault();

                ILazopClient client = new LazopClient(url, appkey, appSecret);
                LazopRequest request = new LazopRequest();
                request.SetApiName("/order/items/get");
                request.SetHttpMethod("GET");
                request.AddApiParameter("order_id", order_id);
                LazopResponse response = client.Execute(request, accessToken);
                obj = response.Body;
                JObject responseJson = JObject.Parse(obj);

                cnt = responseJson["data"].Count();

                var CountIfInsert = 0;
                for (int x = 0; x < cnt; x++)
                {
                    if (responseJson["data"][x]["status"].ToString() == "pending")
                    {
                        decimal totalStocks_ecom = 0;
                        decimal totalStocks_217 = 0;
                        excep = 0;
                        var OOS = string.Empty;
                        var NOB = string.Empty;
                        var SKU = string.Empty;
                        var NOF = string.Empty;
                        var PRC = string.Empty;

                        var clearedOrdersLazada = new ClearedOrders();
                        var orderInfo = new OrderClass();
                        decimal pos_price_decimal = 0;
                        var sku_num = responseJson["data"][x]["sku"].ToString();
                        custumerID = responseJson["data"][x]["buyer_id"].ToString();
                        crt_date = (DateTime)responseJson["data"][x]["created_at"];
                        var paid_item = responseJson["data"][x]["item_price"].Value<decimal>();

                        string sqld = "SELECT COUNT(*) FROM ordersTable WHERE orderId='" + @order_id + "' AND customerID=" + @custumerID + " AND sku_id=" + sku_num + "";
                        using var cmdd = new SqlCommand(sqld, connsd);
                        var result_exist = cmdd.ExecuteScalar();
                        var result_existcount = result_exist == null ? 0 : (int)result_exist;

                        if (result_existcount > 0)
                        {
                            conns.Close();
                            connsd.Close();
                            conns_ecom.Close();
                            //connectionPos.Close();
                            // _userInfoConn.Dispose();
                            return Json(new { set = obj });

                        }

                        //string sqld1 = "DELETE FROM orderTableHeader WHERE orderId='" + @order_id + "'";
                        //using var cmdd1 = new SqlCommand(sqld1, connsd);
                        //cmdd1.ExecuteNonQuery();


                        string sql_clear = "SELECT COUNT(*) FROM clearedOrders WHERE orderId='" + order_id + "'";
                        using var cmd_clear = new SqlCommand(sql_clear, connsd);
                        var result_clear = cmd_clear.ExecuteScalar();

                        string sql_ecom = "SELECT SUM([OnHand]) FROM [EcommerceHub].[dbo].[Inventories] Where Sku= " + @sku_num + " AND OnHand IS NOT NULL GROUP BY SKU";
                        using var cmd_ecom = new SqlCommand(sql_ecom, conns_ecom);
                        var result_ecom = cmd_ecom.ExecuteScalar();
                        totalStocks_ecom = result_ecom == null ? 0 : (decimal)result_ecom;

                        string sql = "SELECT SUM(ON_HAND) FROM [SNR_ECOMMERCE].[dbo].[CurrentInventory] Where STORE=217 AND SKU= " + @sku_num + " GROUP BY SKU";
                        using var cmd = new SqlCommand(sql, conns);
                        var result = cmd.ExecuteScalar();
                        totalStocks_217 = result == null ? 0 : (decimal)result;


                        string upc = "SELECT UPC FROM [SNR_ECOMMERCE].[dbo].[ItemUPC] Where SKU= " + @sku_num;
                        using var cmddd = new SqlCommand(upc, conns);
                        var upcresult = cmddd.ExecuteScalar();


                        string sql_freeitem = "SELECT TOP 1 1 FROM otherItems WHERE skuId='" + @sku_num + "'";
                        using var cmd_freeitem = new SqlCommand(sql_freeitem, connsd);
                        var result_freeitem = cmd_freeitem.ExecuteScalar();
                        /*NpgsqlCommand command = new NpgsqlCommand("select plunmbr, cast(price2 as decimal(18, 2)) / 100 as lazada, cast(long2 as decimal(18, 2)) / 100 as shopee from pluext where plunmbr = '"+@sku_num+"'", connectionPos);
                        NpgsqlDataReader reader = command.ExecuteReader();*/

                        /*if (reader.Read())
                        {
                            var pos_price = reader[1].ToString();

                            pos_price_decimal = Math.Round(Convert.ToDecimal(pos_price),2);



                            if (Convert.ToDecimal(pos_price_decimal) != Convert.ToDecimal(paid_item))
                            {
                                excep = 1;
                                SKU = " PRC ";
                                result_ecom = 0;

                            }
                        }*/

                        //using (NpgsqlCommand command = connectionPos.CreateCommand())
                        //{
                        //    command.CommandText = "select plunmbr, cast(price2 as decimal(18, 2)) / 100 as lazada, cast(long2 as decimal(18, 2)) / 100 as shopee from pluext where plunmbr = '" + @sku_num + "'";
                        //    using (NpgsqlDataReader reader = command.ExecuteReader())
                        //    {

                        //        if (reader.Read())
                        //        {
                        //            var shopee_pos_price = reader[2].ToString();

                        //            pos_price_decimal = Math.Round(Convert.ToDecimal(shopee_pos_price), 2);



                        //            if (Convert.ToDecimal(pos_price_decimal) != Convert.ToDecimal(paid_item))
                        //            {
                        //                excep = 1;
                        //                SKU = " PRC ";
                        //                result_ecom = 0;

                        //            }
                        //        }
                        //        reader.Close();
                        //    }
                        //}



                        string sqldd = $"SELECT COUNT(*) FROM clearedOrders a WHERE skuId='{sku_num}' AND orderId NOT IN (SELECT orderId FROM boxOrders WHERE boxerStatus = 'Done')";
                        using var cmmddd = new SqlCommand(sqldd, connsd);
                        var clearedOrderItems = cmmddd.ExecuteScalar();
                        var totalClearedItems = clearedOrderItems == null ? 0 : (int)clearedOrderItems;


                        var AvailforSelling = (result == null ? 0 : (decimal)result) - totalClearedItems;

                        var resultOrdersItems = new List<OrderClass>();
                        resultOrdersItems = _userInfoConn.ordersTable.Where(u => u.module == "lazada" && u.sku_id == sku_num && u.orderId == order_id).ToList();

                        var totalSameSKUOrders = resultOrdersItems.Count();
                        if (result == null)
                        {
                            if ((result_freeitem == null ? 0 : (int)result_freeitem) > 0)
                            {
                                var checkNOF = new List<OrderClass>();
                                checkNOF = _userInfoConn.ordersTable.Where(e => e.sku_id == sku_num && e.typeOfexception == "NOF").ToList();


                                if (checkNOF.Count < 1)
                                {
                                    var orderHeaderCheck = new List<OrderHeaderClass>();
                                    orderHeaderCheck = _userInfoConn.orderTableHeader.Where(e => e.orderId == order_id).ToList();

                                    if (orderHeaderCheck.Count < 1)
                                    {
                                        clearedOrdersLazada.deductedStockEcom = 1;
                                        clearedOrdersLazada.deductedStock2017 = 1;
                                        clearedOrdersLazada.dateProcess = DateTime.Now;
                                        clearedOrdersLazada.skuId = sku_num;
                                        clearedOrdersLazada.orderId = order_id;
                                        clearedOrdersLazada.module = "lazada";
                                        clearedOrdersLazada.processBy = "System";
                                        clearedOrdersLazada.isFreeItem = true;
                                        clearedOrdersLazada.isFromNIB = false;
                                        clearedOrdersLazada.isNIB = false;
                                        _userInfoConn.Add(clearedOrdersLazada);
                                    }
                                }
                                else
                                {
                                    excep = 1;
                                    NOF = "NOF";
                                    result = 0;
                                }
                            }
                            else
                            {
                                excep = 1;
                                OOS = "OOS";
                                result = 0;
                            }
                        }
                        else
                        {
                            if (totalStocks_217 > 0)
                            {
                                if (totalSameSKUOrders <= AvailforSelling)
                                {
                                    if (totalStocks_ecom > 0)
                                    {
                                        var AvailInBasement = result_ecom == null ? 0 : (decimal)result_ecom - totalClearedItems;
                                        if (totalSameSKUOrders <= AvailInBasement)
                                        {
                                            string location = "SELECT b.Description FROM EcommerceHub.dbo.Inventories a LEFT JOIN EcommerceHub.dbo.InventoryLocations b ON a.Location = b.id WHERE a.Sku =" + sku_num;
                                            using var shopee_cmd_location = new SqlCommand(location, conns_ecom);
                                            var shopee_result_location = shopee_cmd_location.ExecuteScalar();
                                            /*string sqld1_update = "UPDATE [SNR_ECOMMERCE].[dbo].[CurrentInventory] SET ON_HAND=" + totalStocks_217 + " Where STORE=217 AND SKU= " + sku_num + " ";
                                            using var cmdd1_update = new SqlCommand(sqld1_update, conns );
                                            shopee_cmdd1_update.ExecuteNonQuery();*/

                                            /*string sqld1_update = "UPDATE [EcommerceHub].[dbo].[Inventories] SET ON_HAND=" + totalStocks_ecom + " Where Sku= " + sku_num + "";
                                            using var cmdd1_update = new SqlCommand(sqld1_update, conns_ecom);
                                            cmdd1_update.ExecuteNonQuery();*/
                                            if (location.Length > 0)
                                            {
                                                var checkNOF = new List<OrderClass>();
                                                checkNOF = _userInfoConn.ordersTable.Where(e => e.sku_id == sku_num && e.typeOfexception == "NOF").ToList();


                                                if (checkNOF.Count < 1)
                                                {
                                                    //excep = 0;
                                                    Console.WriteLine("no exception");
                                                }
                                                else
                                                {
                                                    excep = 1;
                                                    NOF = "NOF";

                                                }
                                            }
                                            else
                                            {
                                                excep = 1;
                                                NOB = "NIB";

                                            }
                                        }
                                        else
                                        {

                                            excep = 1;
                                            NOB = "NIB";


                                        }
                                    }
                                    else
                                    {

                                        excep = 1;
                                        NOB = "NIB";


                                    }
                                }
                                else
                                {

                                    excep = 1;
                                    OOS = "OOS";
                                }
                            }
                            else
                            {

                                excep = 1;
                                OOS = "OOS";
                            }
                        }



                        //if (Convert.ToDecimal(result_ecom) <= 0)
                        //{
                        //    if (Convert.ToDecimal(result) <= 0)
                        //    {
                        //        excep = 1;
                        //        OOS = "OOS";
                        //    }
                        //    else
                        //    {
                        //        totalStocks_217 = Convert.ToDecimal(result) - 1;
                        //        if (Convert.ToInt32(result_clear) == 0)
                        //        {
                        //            /*string sqld1_update = "UPDATE [SNR_ECOMMERCE].[dbo].[CurrentInventory] SET ON_HAND=" + totalStocks_217 + " Where STORE=217 AND SKU= " + sku_num + " ";
                        //            using var cmdd1_update = new SqlCommand(sqld1_update, conns );
                        //            shopee_cmdd1_update.ExecuteNonQuery();*/
                        //            clearedOrdersLazada.deductedStock2017 = 1;
                        //            clearedOrdersLazada.deductedStockEcom = 0;
                        //            clearedOrdersLazada.dateProcess = DateTime.Now;
                        //            clearedOrdersLazada.skuId = sku_num;
                        //            clearedOrdersLazada.orderId = order_id;
                        //            clearedOrdersLazada.module = "lazada";
                        //            clearedOrdersLazada.processBy = "Employee";
                        //            _userInfoConn.Add(clearedOrdersLazada);
                        //        }
                        //        excep = 1;
                        //        NOB = "NIB";
                        //    }

                        //}
                        //else
                        //{
                        //    totalStocks_ecom = Convert.ToDecimal(result_ecom) - 1;
                        //    if (Convert.ToInt32(result_clear) == 0)
                        //    {
                        //        /*string sqld1_update = "UPDATE [EcommerceHub].[dbo].[Inventories] SET ON_HAND=" + totalStocks_ecom + " Where Sku= " + sku_num + "";
                        //        using var cmdd1_update = new SqlCommand(sqld1_update, conns_ecom);
                        //        cmdd1_update.ExecuteNonQuery();*/

                        //        clearedOrdersLazada.deductedStockEcom = 1;
                        //        clearedOrdersLazada.deductedStock2017 = 0;
                        //        clearedOrdersLazada.dateProcess = DateTime.Now;
                        //        clearedOrdersLazada.skuId = sku_num;
                        //        clearedOrdersLazada.orderId = order_id;
                        //        clearedOrdersLazada.module = "lazada";
                        //        clearedOrdersLazada.processBy = "Employee";
                        //        _userInfoConn.Add(clearedOrdersLazada);
                        //    }
                        //    totalStocks_217 = Convert.ToDecimal(result);
                        //}
                        var orderHeaderCheck2 = new List<OrderHeaderClass>();
                        orderHeaderCheck2 = _userInfoConn.orderTableHeader.Where(e => e.orderId == order_id).ToList();

                        if (orderHeaderCheck2.Count < 1)
                        {
                            orderInfo.orderId = order_id;
                            orderInfo.created_at = crt_date;
                            orderInfo.sku_id = sku_num;
                            orderInfo.item_price = paid_item;
                            orderInfo.pos_price = Convert.ToDecimal(pos_price_decimal);
                            orderInfo.total_item_price = responseJson["data"][x]["item_price"].Value<decimal>();

                            total_amount = total_amount + Convert.ToDecimal(responseJson["data"][x]["item_price"]);

                            orderInfo.item_description = responseJson["data"][x]["name"].ToString();
                            orderInfo.item_quantity = 1m;
                            orderInfo.warehouseQuantity = totalStocks_ecom + " (Ecom Stocks) / " + totalStocks_217 + " (217 Stocks)";
                            orderInfo.dateProcess = DateTime.Now;
                            orderInfo.clubID = "217";
                            orderInfo.customerID = custumerID;
                            orderInfo.item_image = responseJson["data"][x]["product_main_image"].ToString();
                            orderInfo.module = "lazada";
                            orderInfo.exception = excep;
                            orderInfo.typeOfexception = SKU == "" ? OOS == "" ? NOB == "" ? NOF == "" ? string.Empty : NOF : NOB : OOS : SKU;
                            orderInfo.UPC = (upcresult == null ? 0 : (decimal)upcresult);
                            orderInfo.tracking_code = responseJson["data"][x]["tracking_code"].ToString();
                            orderInfo.platform_status = responseJson["data"][x]["status"].ToString();
                            orderInfo.shipment_provider = responseJson["data"][x]["shipment_provider"].ToString();
                            orderInfo.order_item_id = responseJson["data"][x]["order_item_id"].ToString();
                            _userInfoConn.Add(orderInfo);
                        }
                    }

                }

                _userInfoConn.SaveChanges();
                if (excep == 0)
                {

                    var orderHeaderCheck3 = new List<OrderHeaderClass>();
                    orderHeaderCheck3 = _userInfoConn.orderTableHeader.Where(e => e.orderId == order_id).ToList();

                    if (orderHeaderCheck3.Count < 1)
                    {
                        var ordersTable = new List<OrderClass>();
                        ordersTable = _userInfoConn.ordersTable.Where(e => e.orderId == order_id && e.exception == 1).ToList();
                        if (ordersTable.Count > 0)
                        {
                            var orderheader = new OrderHeaderClass
                            {
                                orderId = order_id,
                                module = "lazada",
                                dateFetch = DateTime.Now,
                                dateCreatedAt = crt_date,
                                exception = 1,
                                customerID = custumerID,
                                employee = claimUser.Value,
                                item_count = Convert.ToDecimal(cnt),
                                total_amount = total_amount,
                                status = "with excemption"
                            };
                            _userInfoConn.Add(orderheader);
                        }
                        else
                        {
                            var orderheader = new OrderHeaderClass
                            {
                                orderId = order_id,
                                module = "lazada",
                                dateFetch = DateTime.Now,
                                dateCreatedAt = crt_date,
                                exception = excep,
                                customerID = custumerID,
                                employee = claimUser.Value,
                                item_count = Convert.ToDecimal(cnt),
                                total_amount = total_amount,
                                status = "cleared"
                            };
                            _userInfoConn.Add(orderheader);

                            string sqld1_add = $"INSERT INTO clearedOrders (deductedStockEcom, deductedStock2017, dateProcess, skuId, orderId, module, processBy, isFreeItem, isNIB, isFromNIB) SELECT 1, 1, GETDATE(), sku_id, orderId, 'lazada', 'System', 0, 0, 0 FROM ordersTable WHERE orderId = '{order_id}'  AND platform_status <> 'canceled'";
                            using var cmdd1_add = new SqlCommand(sqld1_add, connsd);
                            cmdd1_add.ExecuteNonQuery();



                        }
                    }

                }
                else
                {
                    var orderHeaderCheck3 = new List<OrderHeaderClass>();
                    orderHeaderCheck3 = _userInfoConn.orderTableHeader.Where(e => e.orderId == order_id).ToList();

                    if (orderHeaderCheck3.Count < 1)
                    {
                        var orderheader = new OrderHeaderClass
                        {
                            orderId = order_id,
                            module = "lazada",
                            dateFetch = DateTime.Now,
                            dateCreatedAt = crt_date,
                            exception = excep,
                            customerID = custumerID,
                            employee = claimUser.Value,
                            item_count = Convert.ToDecimal(cnt),
                            total_amount = total_amount,
                            status = "with excemption"
                        };
                        _userInfoConn.Add(orderheader);
                    }
                }



                _userInfoConn.SaveChanges();

                //SaveOrdersHeader(orderheader);


                conns.Close();
                connsd.Close();
                conns_ecom.Close();
                //connectionPos.Close();
            }
            catch (Exception ex)
            {
                var csd = _configuration.GetConnectionString("Myconnection");
                using var connsd = new SqlConnection(csd);
                connsd.Open();

                string sqld = $"EXEC DeleteNoHeader  @module='lazada'";
                using var cmdd = new SqlCommand(sqld, connsd);
                cmdd.ExecuteNonQuery();
                connsd.Close();

                throw;
            }
            // _userInfoConn.Dispose();
            return Json(new { set = obj });


        }
        public JsonResult ReProcess(string order_id)
        {
            var claimUser = User.Claims.FirstOrDefault(c => c.Type == "username");

            var obj = string.Empty;

            // _userInfoConn.Dispose();
            return Json(new { set = obj });


        }


        [HttpPost]
        public JsonResult SaveTransaction([FromBody] List<Transaction> transObj)
        {

            for (var x = 0; x < transObj.Count(); x++)
            {
                var trans = new Transaction();
                trans.order_number = transObj[x].order_number.ToString();
                trans.item_id = transObj[x].item_id.ToString();
                trans.item_description = transObj[x].item_description.ToString();
                trans.trn_date = DateTime.Now;
                trans.item_price = transObj[x].item_price;
                trans.trn_grand_total = transObj[x].trn_grand_total;
                trans.trn_user = transObj[x].trn_user.ToString();
                trans.status = transObj[x].status.ToString();
                trans.submodule = transObj[x].submodule.ToString();
                _userInfoConn.Add(trans);
            }
            _userInfoConn.SaveChanges();


            return Json(new { responseText = "Success" });
        }
        public IActionResult PrintReport(string categ)
        {

            var dt = new List<OrderClass>();


            /*var dt = new DataTable();
            dt = LazadaOrder();*/

            string mimetype = "";
            int extension = 1;
            // var path = $"{this._webHostEnvironment.WebRootPath}\\Reports\\OrderReport.rdlc";
            var path = String.Empty;

            Dictionary<string, string> parameters = new Dictionary<string, string>();



            if (categ == "OOS")
            {
                dt = _userInfoConn.ordersTable.Where(p => p.module == "lazada" && p.typeOfexception.Contains("OOS")).ToList();
                parameters.Add("prm", "OUT OF STOCK");
                parameters.Add("module", "LAZADA");
                path = $"{this._webHostEnvironment.WebRootPath}\\Reports\\LazadaOrderReportOos.rdlc";
            }
            //if (categ == "SKU")
            //{
            //    dt = _userInfoConn.ordersTable.Where(p => p.module == "lazada" && p.typeOfexception.Contains("SKU")).ToList();
            //    parameters.Add("prm", "SKU DISCREPANCY");
            //    parameters.Add("module", "LAZADA");
            //    path = $"{this._webHostEnvironment.WebRootPath}\\Reports\\LazadaOrderReportSku.rdlc";
            //}
            if (categ == "NIB")
            {
                dt = _userInfoConn.ordersTable.Where(p => p.module == "lazada" && p.typeOfexception.Contains("NIB")).ToList();
                parameters.Add("prm", "NOT IN BASEMENT");
                parameters.Add("module", "LAZADA");
                path = $"{this._webHostEnvironment.WebRootPath}\\Reports\\LazadaOrderReportNob.rdlc";
            }
            //if (categ == "PRC")
            //{
            //    dt = _userInfoConn.ordersTable.Where(p => p.module == "lazada" && p.typeOfexception.Contains("PRC")).ToList();
            //    parameters.Add("prm", "PRICE DISCREPANCY");
            //    parameters.Add("module", "LAZADA");
            //    path = $"{this._webHostEnvironment.WebRootPath}\\Reports\\LazadaOrderReportPrc.rdlc";
            //}

            LocalReport localReport = new LocalReport(path);
            localReport.AddDataSource("LazadaOrder", dt);

            var result = localReport.Execute(RenderType.Pdf, extension, parameters, mimetype);
            return File(result.MainStream, "application/pdf");

        }
        public IActionResult MoveToPicker(string orderId)
        {
            var claimUser = User.Claims.FirstOrDefault(c => c.Type == "username");
            var orderHeader = (from c in _userInfoConn.orderTableHeader where c.orderId == orderId select c).First();

            orderHeader.status = "cleared";
            orderHeader.userClear = claimUser.Value;
            orderHeader.dateProcess = DateTime.Now;
            orderHeader.exception = 0;

            _userInfoConn.SaveChanges();

            return Json(new { responseText = "Success" });
        }


        public JsonResult GetDoneClearedOrders()
        {
            string module = "lazada";
            IEnumerable<GridOrderHeaderClass> items = new List<GridOrderHeaderClass>();
            items = _dataAccess.ExecuteSP2<GridOrderHeaderClass, dynamic>("GetDoneClearedOrders", new { module });
            return Json(new { set = items });

        }

        public JsonResult GetDoneBoxOrders()
        {
            string module = "lazada";
            IEnumerable<GridOrderHeaderClass> items = new List<GridOrderHeaderClass>();
            items = _dataAccess.ExecuteSP2<GridOrderHeaderClass, dynamic>("GetDoneBoxOrders", new { module });
            return Json(new { set = items });

        }

        public JsonResult GetCurrentlyPickingOrders()
        {
            string module = "lazada";
            IEnumerable<GridOrderHeaderClass> items = new List<GridOrderHeaderClass>();
            items = _dataAccess.ExecuteSP2<GridOrderHeaderClass, dynamic>("GetCurrentlyPickingOrders", new { module });
            return Json(new { set = items });

        }

        public JsonResult GetCurrentlyPackingOrders()
        {
            string module = "lazada";
            IEnumerable<GridOrderHeaderClass> items = new List<GridOrderHeaderClass>();
            items = _dataAccess.ExecuteSP2<GridOrderHeaderClass, dynamic>("GetCurrentlyPackingOrders", new { module });
            return Json(new { set = items });

        }

        public string GetOrderItemsException(string orderId)
        {
            string exception = "Cleared";
            List<string> list = new List<string>();
            IEnumerable<OrderClass> items = new List<OrderClass>();
            items = _dataAccess.ExecuteSP2<OrderClass, dynamic>("GetOrderItemsException", new { orderId });

            foreach (var item in items)
            {
                if (item.typeOfexception != "")
                {
                    list.Add(item.typeOfexception);
                }

            }
            if (list.Contains("NIB") || list.Contains("NOF") || list.Contains("OOS"))
            {
                exception = string.Join(",", list);
            }




            return exception;

        }

    }
}
