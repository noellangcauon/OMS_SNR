using AspNetCore.Reporting;
using FastJSON;
using Infrastructure.External.ShopeeWebApi;
using Lazop.Api;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using Polly;
using Serilog.Context;
using SNR_BGC.DataAccess;
using SNR_BGC.Hubs;
using SNR_BGC.Models;
using SNR_BGC.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IHttpClientFactory = System.Net.Http.IHttpClientFactory;

namespace SNR_BGC.Controllers
{
    public class ShopeeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHubContext<ChatHub, IChatHub> _chatHub;


        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly int _pageSize;

        private readonly string _baseUrl;

        private readonly string _baseUrlorderDetails;

        private int _currentPage = 0;

        private readonly Models.UserClass _userInfoConn;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ShopeeController> _logger;
        private readonly IAuthenthicationTokenProvider _authenthicationTokenProvider;
        private IHostApplicationLifetime _hostApplicationLifetime;
        private CancellationToken cancellationToken;
        private readonly IAsyncPolicy _policy;
        private readonly IServiceScope _scope;

        private readonly IDataRepository _dataRepository;
        private readonly IDbAccess _dataAccess;


        public ShopeeController(IConfiguration configuration,
            UserClass tokenInfo,
            IWebHostEnvironment webHostEnvironment,
            ILogger<ShopeeController> logger,
            IHttpClientFactory httpClientFactory,
            IAsyncPolicy policy,
            IAuthenthicationTokenProvider authenthicationTokenProvider,
            IHostApplicationLifetime hostApplicationLifetime,
            IServiceScopeFactory factory
            , IDataRepository dataRepository, IDbAccess dataAccess, IHubContext<ChatHub, IChatHub> chatHub)
        {
            _configuration = configuration;

            _webHostEnvironment = webHostEnvironment;

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            _pageSize = _configuration.GetValue<int>("ShopeeApi:v1:EndPoints:Item:GetItemsList:MaxPageSize");

            _baseUrl = _configuration["ShopeeApi:v1:EndPoints:Orders:GetOrderList:Url"];

            _baseUrlorderDetails = _configuration["ShopeeApi:v1:EndPoints:Orders:GetOrderDetails:Url"];

            _chatHub = chatHub;
            _userInfoConn = tokenInfo;

            _logger = logger;

            _authenthicationTokenProvider = authenthicationTokenProvider;

            this._hostApplicationLifetime = hostApplicationLifetime;

            _httpClientFactory = httpClientFactory;

            _policy = policy;
            _dataRepository = dataRepository;

            _dataAccess = dataAccess;

            _scope = factory.CreateScope();


        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ViewShopeeOrder()
        {

            return View();
        }


        public JsonResult GetClearedOrders()
        {

            //var result = new List<OrderHeaderClass>();

            //result = _userInfoConn.orderTableHeader.Where(u => u.exception == 0 && u.module == "shopee").ToList();
            try
            {
                var latestDate = new DateTime?();
                latestDate = _userInfoConn.orderTableHeader.Where(u => u.module == "shopee").ToList().Max(x => x.dateCreatedAt);
                if (latestDate == null)
                {
                    latestDate = DateTime.Today;
                }
                //_userInfoConn.Dispose();


                var csd = _configuration.GetConnectionString("Myconnection");
                using var connsd = new SqlConnection(csd);
                connsd.Open();


                string sqld = $"EXEC GetOrders @Exception=0, @Module='shopee'";
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
                    item.exception = result_clear["exception"] == null ? 0 : (int)result_clear["exception"];
                    item.customerID = result_clear["customerID"].ToString();
                    item.employee = result_clear["employee"].ToString();
                    item.item_count = result_clear["item_count"] == null ? 0 : (decimal)result_clear["item_count"];
                    item.total_amount = result_clear["total_amount"] == null ? 0 : (decimal)result_clear["total_amount"];
                    item.userClear = result_clear["userClear"].ToString();
                    item.exceptions_count = result_clear["exceptions_count"] == null ? 0 : (int)result_clear["exceptions_count"];
                    item.username = "";

                    items.Add(item);


                }


                connsd.Close();


                return Json(new { set = items, maxDate = latestDate });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public JsonResult GetExceptionsOrders()
        {

            //var result = new List<OrderHeaderClass>();

            //result = _userInfoConn.orderTableHeader.Where(u => u.exception == 1 && u.module == "shopee").ToList();

            //_userInfoConn.Dispose();

            try
            {
                var csd = _configuration.GetConnectionString("Myconnection");
                using var connsd = new SqlConnection(csd);
                connsd.Open();


                string sqld = $"EXEC GetOrders @Exception=1, @Module='shopee'";
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
                    item.typeOfException = GetOrderItemsException(result_clear["orderId"].ToString());
                    item.username = "";
                    items.Add(item);


                }


                connsd.Close();

                return Json(new { set = items });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<IActionResult> GetShopeeOrderList([FromQuery] DateTime dateFrom, DateTime dateTo)
        {
            var coverage = dateFrom.ToString() + " - " + dateTo.ToString();
            int insertedEntity = 0;

            if (dateTo > DateTime.Now.AddMinutes(-30))
            {

                return Json(new { set = "GreaterThanFrom4Hours" });
            }
            else if (dateFrom > dateTo)
            {

                return Json(new { set = "DateFromIsGreaterThanDateTo" });
            }


            var autoReloadLogs = new AutoReloadLogs();
            autoReloadLogs.platform = "Shopee";
            autoReloadLogs.dateProcess = DateTime.Now;
            autoReloadLogs.status = "Good";
            autoReloadLogs.coverage = coverage;
            autoReloadLogs.agent = "Manual";
            _userInfoConn.Add(autoReloadLogs);
            _userInfoConn.SaveChanges();

            insertedEntity = autoReloadLogs.id;
            var ordersCount = 0;

            var shopee_csdd = _configuration.GetConnectionString("Myconnection");
            using var shopee_connsdd = new SqlConnection(shopee_csdd);
            shopee_connsdd.Open();

            string shopee_sql_token = "SELECT TOP 1 accessToken FROM shopeeToken order by entryId Desc";
            using var shopee_cmd_token = new SqlCommand(shopee_sql_token, shopee_connsdd);
            string access_token = shopee_cmd_token.ExecuteScalar().ToString();
            shopee_connsdd.Close();

            long dateFrom_timestamp = dateFrom.ToTimestamp();

            long dateTo_timestamp = dateTo.ToTimestamp();

            await GetCanceledOrderList(access_token);

            var csd = _configuration.GetConnectionString("Myconnection");
            using var connsd = new SqlConnection(csd);
            connsd.Open();

            string sqlddd = $"EXEC DeleteNoHeader @module = 'shopee'";
            using var cmdddd = new SqlCommand(sqlddd, connsd);
            cmdddd.ExecuteNonQuery();
            connsd.Close();




            using (LogContext.PushProperty("Scope", "Shopee Api"))
            {

                if (dateTo.Subtract(dateFrom).Days > 15)
                {
                    dateFrom = DateTime.Today.AddDays(-14);
                }

                var cs = "Data Source=199.84.0.151;Initial Catalog=SNR_ECOMMERCE;User ID=apps;Password=546@Apps#88";
                using var conns = new SqlConnection(cs);
                conns.Open();



                var shopee_cs = "Data Source=199.84.0.151;Initial Catalog=SNR_ECOMMERCE;User ID=apps;Password=546@Apps#88;Encrypt=False;";
                using var shopee_conns = new SqlConnection(shopee_cs);
                shopee_conns.Open();

                var shopee_csd = _configuration.GetConnectionString("Myconnection");
                using var shopee_connsd = new SqlConnection(shopee_csd);
                shopee_connsd.Open();

                /*string shopee_sql_fetch = "SELECT TOP 1 dateCreatedAt FROM orderTableHeader WHERE module='shopee' ORDER BY dateCreatedAt DESC";
                using var shopee_cmd_fetch = new SqlCommand(shopee_sql_fetch, shopee_connsd);
                var shopee_result_fetch = shopee_cmd_fetch.ExecuteScalar();*/

                var shopee_cs_ecom = "Data Source=199.84.17.95;Initial Catalog=EcommerceHub;User ID=apps;Password=546@Apps#88;";
                using var shopee_conns_ecom = new SqlConnection(shopee_cs_ecom);
                shopee_conns_ecom.Open();

                //NpgsqlConnection shopee_connectionPos = new NpgsqlConnection("User ID=postgres;Password=postgres;Host=199.88.17.100;Port=5432;Database=tplinux;");
                //shopee_connectionPos.Open();


                string url = _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:GetOrder:Url"] ?? "";
                //string url = _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:getShippingList:Url"] ?? "";
                string urlInfo = _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:GetOrderDetails:Url"] ?? "";

                DateTime now = DateTime.Now;

                long timestamp = now.ToTimestamp();



                /*long? date_from;

                if (shopee_result_fetch == "" || shopee_result_fetch == null)
                {
                    date_from = DateTime.Now.AddDays(-14).ToTimestamp();
                }
                else
                {
                    date_from = Convert.ToDateTime(shopee_result_fetch).ToTimestamp();
                }*/


                long partnerId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:PartnerId"),
                    shopId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:ShopId");

                var sign = ShopeeApiUtil.SignShopRequest(
                    partnerId: partnerId.ToString(),
                    apiPath: _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:GetOrder:ApiPath"] ?? "",
                    //apiPath: _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:getShippingList:ApiPath"] ?? "",
                    timestamp: timestamp.ToString(),
                    access_token: access_token,
                    shopid: shopId,
                    partnerKey: _configuration["Infrastructure:ShopeeApi:v2:PartnerKey"] ?? "");

                using HttpClient client = _httpClientFactory.CreateClient(name: _configuration["Infrastructure:ShopeeApi:ConnectionName"] ?? "");

                string responseContentString = string.Empty;
                string responseContentString2 = string.Empty;
                JArray mergedOrderList = new JArray(); // Create a new JArray to hold the merged order lists

                Stopwatch stopwatch = Stopwatch.StartNew();

                try
                {
                    //_logger.LogTrace($"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&sign={sign}&shop_id={shopId}&page_size=100");
                    //using HttpResponseMessage httpResponse = await _policy.ExecuteAsync(async (ct) => await client.GetAsync(
                    //     requestUri: $"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&sign={sign}&shop_id={shopId}&page_size=100",
                    //    cancellationToken: ct),
                    //    cancellationToken: cancellationToken);

                    //_logger.LogTrace($"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&order_status=READY_TO_SHIP&sign={sign}&time_range_field=create_time&time_from={dateFrom_timestamp}&time_to={dateTo_timestamp}&page_size=100");
                    //using HttpResponseMessage httpResponse = await _policy.ExecuteAsync(async (ct) => await client.GetAsync(
                    //    requestUri: $"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&order_status=READY_TO_SHIP&sign={sign}&time_range_field=create_time&time_from={dateFrom_timestamp}&time_to={dateTo_timestamp}&page_size=100",
                    //    cancellationToken: ct),
                    //    cancellationToken: cancellationToken);


                    //responseContentString = await httpResponse.Content.ReadAsStringAsync();

                    //_logger.LogTrace(message: "Url:{Url}, Duration: {duration}s, Response Status: {responseStatus}, Responnse Body:{response}",
                    //    url,
                    //    (stopwatch.ElapsedMilliseconds / 1000m).ToString("#,##0.###"),
                    //    httpResponse.StatusCode.ToString(),
                    //    responseContentString);

                    ///////////////////////////////////start of fetching order details///////////////////////
                    //var responseJson = JObject.Parse(responseContentString);

                    //string responseContentString2 = string.Empty;
                    //if ((bool)responseJson["response"]?["more"] == true)
                    //{
                    //    var cursor = responseJson["response"]?["next_cursor"].ToString();
                    //    _logger.LogTrace($"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&order_status=READY_TO_SHIP&sign={sign}&time_range_field=create_time&time_from={dateFrom_timestamp}&time_to={dateTo_timestamp}&page_size=100&cursor={cursor}");
                    //    using HttpResponseMessage httpResponse2 = await _policy.ExecuteAsync(async (ct) => await client.GetAsync(
                    //        requestUri: $"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&order_status=READY_TO_SHIP&sign={sign}&time_range_field=create_time&time_from={dateFrom_timestamp}&time_to={dateTo_timestamp}&page_size=100&cursor={cursor}",
                    //        cancellationToken: ct),
                    //        cancellationToken: cancellationToken);

                    //    responseContentString2 = await httpResponse2.Content.ReadAsStringAsync();

                    //    _logger.LogTrace(message: "Url:{Url}, Duration: {duration}s, Response Status: {responseStatus}, Responnse Body:{response}",
                    //        url,
                    //        (stopwatch.ElapsedMilliseconds / 1000m).ToString("#,##0.###"),
                    //        httpResponse2.StatusCode.ToString(),
                    //        responseContentString2);

                    //    /////////////////////////////////start of fetching order details///////////////////////
                    //    var responseJson2 = JObject.Parse(responseContentString2);



                    //    string responseContentString3 = string.Empty;
                    //    if ((bool)responseJson2["response"]?["more"] == true)
                    //    {
                    //        var cursor2 = responseJson2["response"]?["next_cursor"].ToString();
                    //        _logger.LogTrace($"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&order_status=READY_TO_SHIP&sign={sign}&time_range_field=create_time&time_from={dateFrom_timestamp}&time_to={dateTo_timestamp}&page_size=100&cursor={cursor2}");
                    //        using HttpResponseMessage httpResponse3 = await _policy.ExecuteAsync(async (ct) => await client.GetAsync(
                    //            requestUri: $"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&order_status=READY_TO_SHIP&sign={sign}&time_range_field=create_time&time_from={dateFrom_timestamp}&time_to={dateTo_timestamp}&page_size=100&cursor={cursor2}",
                    //            cancellationToken: ct),
                    //            cancellationToken: cancellationToken);

                    //        responseContentString3 = await httpResponse3.Content.ReadAsStringAsync();

                    //        _logger.LogTrace(message: "Url:{Url}, Duration: {duration}s, Response Status: {responseStatus}, Responnse Body:{response}",
                    //            url,
                    //            (stopwatch.ElapsedMilliseconds / 1000m).ToString("#,##0.###"),
                    //            httpResponse2.StatusCode.ToString(),
                    //            responseContentString3);

                    //        /////////////////////////////////start of fetching order details///////////////////////
                    //        var responseJson3 = JObject.Parse(responseContentString3);
                    //    }



                    //}


                    //    stopwatch.Stop();
                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    ///




                    _logger.LogTrace($"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&order_status=READY_TO_SHIP&sign={sign}&time_range_field=update_time&time_from={dateFrom_timestamp}&time_to={dateTo_timestamp}&page_size=100");
                    using HttpResponseMessage httpResponse = await _policy.ExecuteAsync(async (ct) => await client.GetAsync(
                        requestUri: $"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&order_status=READY_TO_SHIP&sign={sign}&time_range_field=update_time&time_from={dateFrom_timestamp}&time_to={dateTo_timestamp}&page_size=100",
                        cancellationToken: ct),
                        cancellationToken: cancellationToken);


                    responseContentString = await httpResponse.Content.ReadAsStringAsync();

                    _logger.LogTrace(message: "Url:{Url}, Duration: {duration}s, Response Status: {responseStatus}, Responnse Body:{response}",
                        url,
                        (stopwatch.ElapsedMilliseconds / 1000m).ToString("#,##0.###"),
                        httpResponse.StatusCode.ToString(),
                        responseContentString);

                    /////////////////////////////////start of fetching order details///////////////////////
                    var responseJson = JObject.Parse(responseContentString);
                    JArray orderList1 = (JArray)(responseJson["response"]?["order_list"] ?? new JArray());
                    mergedOrderList.Merge(orderList1);

                    if ((bool)responseJson["response"]?["more"] == true)
                    {
                        bool moreData = true;
                        var cursor = responseJson["response"]?["next_cursor"].ToString();

                        while (moreData)
                        {
                            try
                            {
                                string requestUrl = $"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&order_status=READY_TO_SHIP&sign={sign}&time_range_field=create_time&time_from={dateFrom_timestamp}&time_to={dateTo_timestamp}&page_size=100";

                                if (!string.IsNullOrEmpty(cursor))
                                {
                                    requestUrl += $"&cursor={cursor}";
                                }

                                _logger.LogTrace(requestUrl);

                                using HttpResponseMessage httpResponse2 = await _policy.ExecuteAsync(async (ct) => await client.GetAsync(requestUrl, ct), cancellationToken);

                                responseContentString2 = await httpResponse2.Content.ReadAsStringAsync();

                                _logger.LogTrace($"Url:{requestUrl}, Duration: {(stopwatch.ElapsedMilliseconds / 1000m).ToString("#,##0.###")}s, Response Status: {httpResponse.StatusCode.ToString()}, Response Body:{responseContentString2}");

                                var responseJson2 = JObject.Parse(responseContentString2);

                                JArray orderList2 = (JArray)(responseJson2["response"]?["order_list"] ?? new JArray());
                                mergedOrderList.Merge(orderList2);

                                moreData = (bool)responseJson2["response"]?["more"];
                                cursor = responseJson2["response"]?["next_cursor"]?.ToString();

                                // Process responseJson as needed

                                // Ensure cursor is not null for next iteration
                                if (cursor == "")
                                {
                                    moreData = false;
                                }

                                // Stop loop if no more data
                                if (!moreData)
                                {
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {


                                var autoReloadLogs2 = _userInfoConn.AutoReloadLogs.FirstOrDefault(e => e.id == insertedEntity);

                                if (autoReloadLogs2 != null)
                                {
                                    autoReloadLogs2.logs = "Successful Run";
                                    autoReloadLogs2.status = "Success";
                                    autoReloadLogs2.totalOrder = ordersCount;
                                    autoReloadLogs2.completion = DateTime.Now;

                                    _userInfoConn.SaveChanges();
                                }
                                // Handle the error as needed
                                break; // Exit the loop on error
                            }
                        }
                    }
                    stopwatch.Stop();



                    ordersCount = mergedOrderList.Count;
                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


                    //var dictionary = responseJson.ToDictionary( x => x.OrderById, y => y.PackageNo);

                    //dictionary.TryGetValue()


                    if (mergedOrderList.Count > 0)
                    {
                        //JArray array = (JArray)(responseJson["response"]?["order_list"] ?? "");

                        //// Deserialize the JSON into a List of objects
                        ////List<Dictionary<string, string>> list = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(array.ToString());

                        ////// Create a Dictionary with order_sn as the key and package_number as the value
                        ////Dictionary<string, string> result = list.ToDictionary(item => item["order_sn"], item => item["package_number"]);
                        //var count = array.Count;
                        //Console.WriteLine(count);
                        for (var x = 0; x < mergedOrderList.Count; x++)
                        {
                            string sqqldd = "SELECT COUNT(*) FROM orderTableHeader WHERE orderId='" + mergedOrderList[x]?["order_sn"].ToString() + "'";
                            using var ccmmdd = new SqlCommand(sqqldd, shopee_connsd);
                            var result_exists = ccmmdd.ExecuteScalar();
                            var result_existcount = result_exists == null ? 0 : (int)result_exists;

                            if (result_existcount < 1)
                            {

                                var signInfo = ShopeeApiUtil.SignShopRequest(
                                    partnerId: partnerId.ToString(),
                                    apiPath: _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:GetOrderDetails:ApiPath"] ?? "",
                                    timestamp: timestamp.ToString(),
                                    access_token: access_token,
                                    shopid: shopId,
                                    partnerKey: _configuration["Infrastructure:ShopeeApi:v2:PartnerKey"] ?? "");

                                using HttpClient clientInfo = _httpClientFactory.CreateClient(name: _configuration["Infrastructure:ShopeeApi:ConnectionName"] ?? "");

                                string responseContentStringInfo = string.Empty;
                                string[] paramShopee = { "item_list", "buyer_username" };

                                _logger.LogTrace($"{urlInfo}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&sign={signInfo}&order_sn_list={mergedOrderList[x]?["order_sn"].ToString()}&response_optional_fields={string.Join(separator: ",", paramShopee)}");
                                string req = $"{urlInfo}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&sign={signInfo}&order_sn_list={mergedOrderList[x]?["order_sn"].ToString()}&response_optional_fields={string.Join(separator: ",", paramShopee)}";
                                Stopwatch stopwatchInfo = Stopwatch.StartNew();
                                using HttpResponseMessage httpResponseInfo = await _policy.ExecuteAsync(async (ct) => await clientInfo.GetAsync(
                                requestUri: $"{urlInfo}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&sign={signInfo}&order_sn_list={mergedOrderList[x]?["order_sn"].ToString()}&response_optional_fields={string.Join(separator: ",", paramShopee)}",
                                cancellationToken: ct),
                                cancellationToken: cancellationToken);

                                stopwatchInfo.Stop();

                                responseContentStringInfo = await httpResponseInfo.Content.ReadAsStringAsync();

                                _logger.LogTrace(message: "Url:{Url}, Duration: {duration}s, Response Status: {responseStatus}, Responnse Body:{response}",
                                    url,
                                    (stopwatchInfo.ElapsedMilliseconds / 1000m).ToString("#,##0.###"),
                                    httpResponseInfo.StatusCode.ToString(),
                                    responseContentStringInfo);

                                var responseJsonInfo = JObject.Parse(responseContentStringInfo);
                                //await GetShipmentList(stoppingToken, myList[0]);


                                JArray array1 = (JArray)(responseJsonInfo["response"]?["order_list"] ?? "");

                                foreach (JToken jtoken in array1)
                                {
                                    try
                                    {

                                        var claimUser = "System";
                                        var shopee_order_id = string.Empty;
                                        var shopee_customerID = string.Empty;
                                        decimal shopee_total_amount = 0;

                                        int shopee_cnt = 0;
                                        int shopee_excep = 0;
                                        DateTime shopee_crt_date = new DateTime();

                                        string shopee_ordersn = jtoken.Value<string>("order_sn") ?? "";
                                        Double shopee_unixTimeStamp = jtoken.Value<Double>("create_time");
                                        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                                        dateTime = dateTime.AddSeconds(shopee_unixTimeStamp).ToLocalTime();
                                        shopee_customerID = jtoken.Value<string>("buyer_username") ?? "";

                                        JArray array2 = jtoken.Value<JArray>("item_list")!;
                                        shopee_cnt = array2.Count();

                                        //result.TryGetValue(shopee_ordersn, out var packagenumber);
                                        //Console.WriteLine(packagenumber);
                                        /*var OOS = string.Empty;
                                        var NOB = string.Empty;
                                        var SKU = string.Empty;
                                        var PRC = string.Empty;*/

                                        if (jtoken.Value<string>("order_status") == "READY_TO_SHIP")
                                        {
                                            foreach (JToken jtoken2 in array2)
                                            {
                                                Decimal shopee_variation_quantity_purchased = jtoken2.Value<Decimal>("model_quantity_purchased");

                                                for (int i = 0; i < shopee_variation_quantity_purchased; i++)
                                                {



                                                    shopee_excep = 0;

                                                    decimal shopee_totalStocks_ecom = 0;
                                                    decimal shopee_totalStocks_217 = 0;
                                                    var shopee_OOS = string.Empty;
                                                    var shopee_NOB = string.Empty;
                                                    var shopee_SKU = string.Empty;
                                                    var shopee_NOF = string.Empty;
                                                    var shopee_PRC = string.Empty;
                                                    decimal shopee_pos_price_decimal = 0;



                                                    var clearedOrders = new ClearedOrders();
                                                    var shopee_orderInfo = new OrderClass();
                                                    var shopee_NobNofSkuInfo = new NobNofSkuClass();
                                                    Decimal shopee_total_item_price = 0;

                                                    string shopee_sku_id = jtoken2.Value<string>("item_sku") ?? "";
                                                    Decimal shopee_item_price = jtoken2.Value<Decimal>("model_discounted_price");
                                                    string shopee_item_description = jtoken2.Value<string>("item_name") ?? "";
                                                    // Decimal shopee_variation_quantity_purchased = jtoken2.Value<Decimal>("variation_quantity_purchased");


                                                    //string sqld = "SELECT COUNT(*) FROM ordersTable WHERE orderId='" + shopee_ordersn + "' AND customerID='" + shopee_customerID + "' AND sku_id=" + shopee_sku_id + "";
                                                    //using var cmdd = new SqlCommand(sqld, shopee_connsd);
                                                    //var result_exist = cmdd.ExecuteScalar();
                                                    //var result_existcount = result_exist == null ? 0 : (int)result_exist;

                                                    //if (result_existcount > 0)
                                                    //{
                                                    //    conns.Close();
                                                    //    shopee_connsd.Close();
                                                    //    shopee_conns_ecom.Close();
                                                    //    //connectionPos.Close();
                                                    //    // _userInfoConn.Dispose();
                                                    //    return Json(new { responseText = "Success" });

                                                    //}

                                                    string shopee_sql_clear = "SELECT COUNT(*) FROM clearedOrders WHERE orderId='" + shopee_ordersn + "'";
                                                    using var shopee_cmd_clear = new SqlCommand(shopee_sql_clear, shopee_connsd);
                                                    var shopee_result_clear = shopee_cmd_clear.ExecuteScalar();

                                                    string shopee_sql_ecom = "SELECT SUM([OnHand]) FROM [EcommerceHub].[dbo].[Inventories] Where Sku= " + shopee_sku_id + " GROUP BY SKU";
                                                    using var shopee_cmd_ecom = new SqlCommand(shopee_sql_ecom, shopee_conns_ecom);
                                                    var shopee_result_ecom = shopee_cmd_ecom.ExecuteScalar();
                                                    shopee_totalStocks_ecom = shopee_result_ecom == null ? 0 : (decimal)shopee_result_ecom;

                                                    string shopee_sql = "SELECT SUM(ON_HAND) FROM [SNR_ECOMMERCE].[dbo].[CurrentInventory] Where STORE=217 AND SKU= " + shopee_sku_id + " GROUP BY SKU";
                                                    using var shopee_cmd = new SqlCommand(shopee_sql, shopee_conns);
                                                    var shopee_result = shopee_cmd.ExecuteScalar();
                                                    var totalStocks_217 = shopee_result == null ? 0 : (decimal)shopee_result;
                                                    shopee_totalStocks_217 = shopee_result == null ? 0 : (decimal)shopee_result;


                                                    string upc = "SELECT UPC FROM [SNR_ECOMMERCE].[dbo].[ItemUPC] Where SKU= " + shopee_sku_id;
                                                    using var cmddd = new SqlCommand(upc, conns);
                                                    var upcresult = cmddd.ExecuteScalar();


                                                    string sql_freeitem = "SELECT TOP 1 1 FROM otherItems WHERE skuId='" + shopee_sku_id + "'";
                                                    using var cmd_freeitem = new SqlCommand(sql_freeitem, shopee_connsd);
                                                    var result_freeitem = cmd_freeitem.ExecuteScalar();
                                                    //using (NpgsqlCommand command = shopee_connectionPos.CreateCommand())
                                                    //{
                                                    //    command.CommandText = "select plunmbr, cast(price2 as decimal(18, 2)) / 100 as lazada, cast(long2 as decimal(18, 2)) / 100 as shopee from pluext where plunmbr = '" + shopee_sku_id + "'";
                                                    //    using (NpgsqlDataReader reader = command.ExecuteReader())
                                                    //    {

                                                    //        if (reader.Read())
                                                    //        {
                                                    //            var shopee_pos_price = reader[2].ToString();

                                                    //            shopee_pos_price_decimal = Math.Round(Convert.ToDecimal(shopee_pos_price), 2);



                                                    //            if (Convert.ToDecimal(shopee_pos_price_decimal) != Convert.ToDecimal(shopee_item_price))
                                                    //            {
                                                    //                shopee_excep = 1;
                                                    //                shopee_SKU = " PRC ";
                                                    //                shopee_result_ecom = 0;

                                                    //            }
                                                    //        }
                                                    //        reader.Close();
                                                    //    }
                                                    //}

                                                    string sqldd = $"SELECT COUNT(*) FROM clearedOrders a WHERE skuId='{shopee_sku_id}' AND orderId NOT IN (SELECT orderId FROM boxOrders WHERE boxerStatus = 'Done')";
                                                    using var cmmddd = new SqlCommand(sqldd, shopee_connsd);
                                                    var clearedOrderItems = cmmddd.ExecuteScalar();
                                                    var totalClearedItems = clearedOrderItems == null ? 0 : (int)clearedOrderItems;

                                                    //var clearedOrderItems = new List<ClearedOrders>();
                                                    //clearedOrderItems = _userInfoConn.clearedOrders.Where(u => u.skuId == shopee_sku_id).ToList();
                                                    //decimal totalClearedItems = 0;
                                                    //totalClearedItems = clearedOrderItems.Count();



                                                    var AvailforSelling = (shopee_result == null ? 0 : (decimal)shopee_result) - totalClearedItems;

                                                    var resultOrdersItems = new List<OrderClass>();
                                                    resultOrdersItems = _userInfoConn.ordersTable.Where(u => u.module == "shopee" && u.sku_id == shopee_sku_id && u.orderId == shopee_ordersn).ToList();

                                                    var totalSameSKUOrders = resultOrdersItems.Count();


                                                    if (shopee_result == null)
                                                    {
                                                        if ((result_freeitem == null ? 0 : (int)result_freeitem) > 0)
                                                        {
                                                            var checkNOF = new List<OrderClass>();
                                                            checkNOF = _userInfoConn.ordersTable.Where(e => e.sku_id == shopee_sku_id && e.typeOfexception == "NOF").ToList();


                                                            if (checkNOF.Count < 1)
                                                            {

                                                                var orderHeaderCheck = new List<OrderHeaderClass>();
                                                                orderHeaderCheck = _userInfoConn.orderTableHeader.Where(e => e.orderId == shopee_ordersn).ToList();

                                                                if (orderHeaderCheck.Count < 1)
                                                                {
                                                                    clearedOrders.deductedStock2017 = 1;
                                                                    clearedOrders.deductedStockEcom = 1;
                                                                    clearedOrders.dateProcess = DateTime.Now;
                                                                    clearedOrders.skuId = shopee_sku_id;
                                                                    clearedOrders.orderId = shopee_ordersn;
                                                                    clearedOrders.module = "shopee";
                                                                    clearedOrders.processBy = "System";
                                                                    clearedOrders.isFreeItem = true;
                                                                    clearedOrders.isFromNIB = false;
                                                                    clearedOrders.isNIB = false;
                                                                    _userInfoConn.Add(clearedOrders);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                shopee_excep = 1;
                                                                shopee_NOF = "NOF";
                                                                shopee_result = 0;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            shopee_excep = 1;
                                                            shopee_OOS = "OOS";
                                                            shopee_result = 0;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (totalStocks_217 > 0)
                                                        {
                                                            if (totalSameSKUOrders <= AvailforSelling)
                                                            {
                                                                if (shopee_totalStocks_ecom > 0)
                                                                {
                                                                    var AvailInBasement = shopee_totalStocks_ecom - totalClearedItems;
                                                                    if (totalSameSKUOrders <= AvailInBasement)
                                                                    {

                                                                        string location = "SELECT b.Description FROM EcommerceHub.dbo.Inventories a LEFT JOIN EcommerceHub.dbo.InventoryLocations b ON a.Location = b.id WHERE a.Sku =" + shopee_sku_id;
                                                                        using var shopee_cmd_location = new SqlCommand(location, shopee_conns_ecom);
                                                                        var shopee_result_location = shopee_cmd_location.ExecuteScalar();

                                                                        if (location.Length > 0)
                                                                        {

                                                                            /*string shopee_sqld1_update = "UPDATE [SNR_ECOMMERCE].[dbo].[CurrentInventory] SET ON_HAND=" + shopee_totalStocks_217 + " Where STORE=217 AND SKU= " + shopee_sku_id + " ";
                                                                            using var shopee_cmdd1_update = new SqlCommand(shopee_sqld1_update, shopee_conns );
                                                                            shopee_cmdd1_update.ExecuteNonQuery();*/


                                                                            /*string shopee_sqld1_update = "UPDATE [EcommerceHub].[dbo].[Inventories] SET ON_HAND=" + shopee_totalStocks_ecom + " Where Sku= " + shopee_sku_id + "";
                                                                            using var shopee_cmdd1_update = new SqlCommand(shopee_sqld1_update, shopee_conns_ecom);
                                                                            shopee_cmdd1_update.ExecuteNonQuery();*/


                                                                            //clearedOrders.deductedStock2017 = 1;
                                                                            //clearedOrders.deductedStockEcom = 1;
                                                                            //clearedOrders.dateProcess = DateTime.Now;
                                                                            //clearedOrders.skuId = shopee_sku_id;
                                                                            //clearedOrders.orderId = shopee_ordersn;
                                                                            //clearedOrders.module = "shopee";
                                                                            //clearedOrders.processBy = "System";
                                                                            //clearedOrders.isFreeItem = false;
                                                                            //clearedOrders.isFromNIB = false;
                                                                            //_userInfoConn.Add(clearedOrders);
                                                                            var checkNOF = new List<OrderClass>();
                                                                            checkNOF = _userInfoConn.ordersTable.Where(e => e.sku_id == shopee_sku_id && e.typeOfexception == "NOF").ToList();


                                                                            if (checkNOF.Count < 1)
                                                                            {
                                                                                Console.WriteLine("no exception");
                                                                            }
                                                                            else
                                                                            {
                                                                                shopee_excep = 1;
                                                                                shopee_NOF = "NOF";

                                                                            }

                                                                        }
                                                                        else
                                                                        {
                                                                            shopee_excep = 1;
                                                                            shopee_NOB = "NIB";

                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        shopee_excep = 1;
                                                                        shopee_NOB = "NIB";
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    shopee_excep = 1;
                                                                    shopee_NOB = "NIB";

                                                                }
                                                            }
                                                            else
                                                            {

                                                                shopee_excep = 1;
                                                                shopee_OOS = "OOS";
                                                            }
                                                        }
                                                        else
                                                        {

                                                            shopee_excep = 1;
                                                            shopee_OOS = "OOS";
                                                        }

                                                    }

                                                    //if (Convert.ToDecimal(shopee_result_ecom) <= 0)
                                                    //{
                                                    //    if (Convert.ToDecimal(shopee_result) <= 0)
                                                    //    {
                                                    //        shopee_excep = 1;
                                                    //        shopee_OOS = "OOS";
                                                    //    }
                                                    //    else
                                                    //    {
                                                    //        shopee_totalStocks_217 = Convert.ToDecimal(shopee_result) - 1;
                                                    //        if (Convert.ToInt32(shopee_result_clear) == 0)
                                                    //        {
                                                    //            /*string shopee_sqld1_update = "UPDATE [SNR_ECOMMERCE].[dbo].[CurrentInventory] SET ON_HAND=" + shopee_totalStocks_217 + " Where STORE=217 AND SKU= " + shopee_sku_id + " ";
                                                    //            using var shopee_cmdd1_update = new SqlCommand(shopee_sqld1_update, shopee_conns );
                                                    //            shopee_cmdd1_update.ExecuteNonQuery();*/
                                                    //            clearedOrders.deductedStock2017 = 1;
                                                    //            clearedOrders.deductedStockEcom = 0;
                                                    //            clearedOrders.dateProcess = DateTime.Now;
                                                    //            clearedOrders.skuId = shopee_sku_id;
                                                    //            clearedOrders.orderId = shopee_ordersn;
                                                    //            clearedOrders.module = "shopee";
                                                    //            clearedOrders.processBy = "System";
                                                    //            _userInfoConn.Add(clearedOrders);
                                                    //        }

                                                    //        shopee_excep = 1;
                                                    //        shopee_NOB = "NIB";

                                                    //    }



                                                    //}
                                                    //else
                                                    //{

                                                    //    shopee_totalStocks_ecom = Convert.ToDecimal(shopee_result_ecom) - 1;
                                                    //    if (Convert.ToInt32(shopee_result_clear) == 0)
                                                    //    {
                                                    //        /*string shopee_sqld1_update = "UPDATE [EcommerceHub].[dbo].[Inventories] SET ON_HAND=" + shopee_totalStocks_ecom + " Where Sku= " + shopee_sku_id + "";
                                                    //        using var shopee_cmdd1_update = new SqlCommand(shopee_sqld1_update, shopee_conns_ecom);
                                                    //        shopee_cmdd1_update.ExecuteNonQuery();*/
                                                    //        clearedOrders.deductedStockEcom = 1;
                                                    //        clearedOrders.deductedStock2017 = 0;
                                                    //        clearedOrders.dateProcess = DateTime.Now;
                                                    //        clearedOrders.skuId = shopee_sku_id;
                                                    //        clearedOrders.orderId = shopee_ordersn;
                                                    //        clearedOrders.module = "shopee";
                                                    //        clearedOrders.processBy = "Employee";
                                                    //        _userInfoConn.Add(clearedOrders);
                                                    //    }
                                                    //    shopee_totalStocks_217 = Convert.ToDecimal(shopee_result);
                                                    //}


                                                    var orderHeaderCheck2 = new List<OrderHeaderClass>();
                                                    orderHeaderCheck2 = _userInfoConn.orderTableHeader.Where(e => e.orderId == shopee_ordersn).ToList();

                                                    if (orderHeaderCheck2.Count < 1)
                                                    {

                                                        shopee_orderInfo.orderId = shopee_ordersn;
                                                        shopee_order_id = shopee_ordersn;
                                                        shopee_orderInfo.created_at = dateTime;
                                                        shopee_crt_date = dateTime;
                                                        shopee_orderInfo.item_image = jtoken2["image_info"]?.Value<string>("image_url") ?? "";
                                                        shopee_orderInfo.sku_id = shopee_sku_id;
                                                        shopee_orderInfo.item_price = shopee_item_price;
                                                        shopee_orderInfo.pos_price = shopee_pos_price_decimal;
                                                        shopee_orderInfo.total_item_price = shopee_item_price * shopee_variation_quantity_purchased;
                                                        shopee_total_item_price = shopee_item_price * shopee_variation_quantity_purchased;
                                                        shopee_total_amount = shopee_total_amount + shopee_total_item_price;
                                                        shopee_orderInfo.item_description = shopee_item_description;
                                                        shopee_orderInfo.item_quantity = 1;
                                                        shopee_orderInfo.warehouseQuantity = shopee_totalStocks_ecom + " (Ecom Stocks) / " + shopee_totalStocks_217 + " (217 Stocks)";
                                                        shopee_orderInfo.dateProcess = DateTime.Now;
                                                        shopee_orderInfo.clubID = "217";
                                                        shopee_orderInfo.customerID = shopee_customerID;
                                                        shopee_orderInfo.module = "shopee";
                                                        shopee_orderInfo.exception = shopee_excep;
                                                        shopee_orderInfo.typeOfexception = shopee_SKU == "" ? shopee_OOS == "" ? shopee_NOB == "" ? shopee_NOF == "" ? string.Empty : shopee_NOF : shopee_NOB : shopee_OOS : shopee_SKU;
                                                        shopee_orderInfo.UPC = upcresult == null ? 0 : (decimal)upcresult;
                                                        shopee_orderInfo.platform_status = jtoken.Value<string>("order_status") ?? "";

                                                        _userInfoConn.Add(shopee_orderInfo);
                                                    }
                                                }
                                            }

                                            _userInfoConn.SaveChanges();

                                            if (shopee_excep == 0)
                                            {
                                                var orderHeaderCheck3 = new List<OrderHeaderClass>();
                                                orderHeaderCheck3 = _userInfoConn.orderTableHeader.Where(e => e.orderId == shopee_ordersn).ToList();

                                                if (orderHeaderCheck3.Count < 1)
                                                {

                                                    var ordersTable = new List<OrderClass>();
                                                    ordersTable = _userInfoConn.ordersTable.Where(e => e.orderId == shopee_ordersn && e.exception == 1).ToList();
                                                    if (ordersTable.Count > 0)
                                                    {
                                                        var orderheaderShopee = new OrderHeaderClass
                                                        {
                                                            orderId = shopee_order_id,
                                                            module = "shopee",
                                                            dateFetch = DateTime.Now,
                                                            dateCreatedAt = shopee_crt_date,
                                                            exception = 1,
                                                            customerID = shopee_customerID,
                                                            employee = "System",
                                                            item_count = Convert.ToDecimal(shopee_cnt),
                                                            total_amount = shopee_total_amount,
                                                            status = "with excemption"
                                                            //package_number = packagenumber

                                                        };
                                                        _userInfoConn.Add(orderheaderShopee);
                                                    }
                                                    else
                                                    {
                                                        var orderheaderShopee = new OrderHeaderClass
                                                        {
                                                            orderId = shopee_order_id,
                                                            module = "shopee",
                                                            dateFetch = DateTime.Now,
                                                            dateCreatedAt = shopee_crt_date,
                                                            exception = shopee_excep,
                                                            customerID = shopee_customerID,
                                                            employee = "System",
                                                            item_count = Convert.ToDecimal(shopee_cnt),
                                                            total_amount = shopee_total_amount,
                                                            status = "cleared"
                                                            //package_number = packagenumber
                                                        };
                                                        _userInfoConn.Add(orderheaderShopee);

                                                        string sqld1_add = $"INSERT INTO clearedOrders (deductedStockEcom, deductedStock2017, dateProcess, skuId, orderId, module, processBy, isFreeItem, isNIB, isFromNIB) SELECT 1, 1, GETDATE(), sku_id, orderId, 'shopee', 'System', 0, 0, 0 FROM ordersTable WHERE orderId = '{shopee_ordersn}'";
                                                        using var cmdd1_add = new SqlCommand(sqld1_add, shopee_connsd);
                                                        cmdd1_add.ExecuteNonQuery();
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                var orderHeaderCheck4 = new List<OrderHeaderClass>();
                                                orderHeaderCheck4 = _userInfoConn.orderTableHeader.Where(e => e.orderId == shopee_ordersn).ToList();

                                                if (orderHeaderCheck4.Count < 1)
                                                {
                                                    var orderheaderShopee = new OrderHeaderClass
                                                    {
                                                        orderId = shopee_order_id,
                                                        module = "shopee",
                                                        dateFetch = DateTime.Now,
                                                        dateCreatedAt = shopee_crt_date,
                                                        exception = shopee_excep,
                                                        customerID = shopee_customerID,
                                                        employee = "System",
                                                        item_count = Convert.ToDecimal(shopee_cnt),
                                                        total_amount = shopee_total_amount,
                                                        status = "with excemption"
                                                        //package_number = packagenumber
                                                    };
                                                    _userInfoConn.Add(orderheaderShopee);
                                                }
                                            }

                                        }
                                    }
                                    catch (FormatException)
                                    {
                                        var csds = _configuration.GetConnectionString("Myconnection");
                                        using var connsds = new SqlConnection(csds);
                                        connsds.Open();

                                        string sqld = $"EXEC DeleteNoHeader @module='shopee'";
                                        using var cmdd = new SqlCommand(sqld, connsds);
                                        cmdd.ExecuteNonQuery();
                                        connsds.Close();

                                        throw new ShopeeApiException($"Invalid object ({jtoken.ToString(Formatting.None)}) detected at Shopee.")
                                        {
                                            RequestUrl = urlInfo,
                                            RequestMethod = "POST",
                                            ResponseDescription = responseContentStringInfo,
                                            Timestamp = now
                                        };
                                    }
                                }

                            }

                        }

                        //if (array.Count > 0)
                        //{
                        //    var myList = new List<string>();
                        //    foreach (JToken jtoken in array)
                        //    {

                        //        try
                        //        {

                        //            string ordersn = jtoken.Value<string>("order_sn") ?? "";
                        //            string package = jtoken.Value<string>("package_number") ?? "";
                        //            myList.Add(ordersn);

                        //            //var dictionary = new Dictionary<string, object>();
                        //            //{ orderId: , package_number:}

                        //        }
                        //        catch (FormatException)
                        //        {
                        //            throw new ShopeeApiException(message: "An error occured while accessing shopee API")
                        //            {
                        //                RequestUrl = url ?? "",
                        //                RequestMethod = "POST",
                        //                ResponseDescription = responseContentString,
                        //                Timestamp = now,
                        //                HttpStatusCode = httpResponse.StatusCode
                        //            };
                        //        }
                        //    }

                        //}
                    }
                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        throw new ShopeeApiException(message: "An error occured while accessing shopee API")
                        {
                            RequestUrl = url,
                            RequestMethod = "POST",
                            ResponseDescription = responseContentString,
                            Timestamp = now,
                            HttpStatusCode = httpResponse.StatusCode
                        };
                    }
                }
                catch (TaskCanceledException ex)
                {
                    var csds = _configuration.GetConnectionString("Myconnection");
                    using var connsds = new SqlConnection(csds);
                    connsds.Open();

                    string sqld = $"EXEC DeleteNoHeader  @module='shopee'";
                    using var cmdd = new SqlCommand(sqld, connsds);
                    cmdd.ExecuteNonQuery();
                    connsds.Close();

                    var autoReloadLogs3 = _userInfoConn.AutoReloadLogs.FirstOrDefault(e => e.id == insertedEntity);

                    if (autoReloadLogs3 != null)
                    {
                        autoReloadLogs3.logs = ex.ToString();
                        autoReloadLogs3.status = "Failed";
                        autoReloadLogs3.totalOrder = ordersCount;
                        autoReloadLogs3.completion = DateTime.Now;

                        _userInfoConn.SaveChanges();
                    }
                }
                catch (ShopeeApiException ex)
                {
                    var csds = _configuration.GetConnectionString("Myconnection");
                    using var connsds = new SqlConnection(csds);
                    connsds.Open();

                    string sqld = $"EXEC DeleteNoHeader  @module='shopee'";
                    using var cmdd = new SqlCommand(sqld, connsds);
                    cmdd.ExecuteNonQuery();
                    connsds.Close();


                    var autoReloadLogs4 = _userInfoConn.AutoReloadLogs.FirstOrDefault(e => e.id == insertedEntity);

                    if (autoReloadLogs4 != null)
                    {
                        autoReloadLogs4.logs = ex.ToString();
                        autoReloadLogs4.status = "Failed";
                        autoReloadLogs4.totalOrder = ordersCount;
                        autoReloadLogs4.completion = DateTime.Now;

                        _userInfoConn.SaveChanges();
                    }

                }
                catch (Exception ex)
                {
                    var csds = _configuration.GetConnectionString("Myconnection");
                    using var connsds = new SqlConnection(csds);
                    connsds.Open();

                    string sqld = $"EXEC DeleteNoHeader  @module='shopee'";
                    using var cmdd = new SqlCommand(sqld, connsds);
                    cmdd.ExecuteNonQuery();
                    connsds.Close();


                    var autoReloadLogs5 = _userInfoConn.AutoReloadLogs.FirstOrDefault(e => e.id == insertedEntity);

                    if (autoReloadLogs5 != null)
                    {
                        autoReloadLogs5.logs = ex.ToString();
                        autoReloadLogs5.status = "Failed";
                        autoReloadLogs5.totalOrder = ordersCount;
                        autoReloadLogs5.completion = DateTime.Now;

                        _userInfoConn.SaveChanges();
                    }


                }
                _userInfoConn.SaveChanges();
                //_userInfoConn.Dispose();
                shopee_conns.Close();
                shopee_connsd.Close();
                shopee_conns_ecom.Close();
                //shopee_connectionPos.Close();
            }









            /* LAZADA */



            var autoReloadLogs6 = _userInfoConn.AutoReloadLogs.FirstOrDefault(e => e.id == insertedEntity);

            if (autoReloadLogs6 != null)
            {
                autoReloadLogs6.logs = "Successful Run";
                autoReloadLogs6.status = "Success";
                autoReloadLogs6.totalOrder = ordersCount;
                autoReloadLogs6.completion = DateTime.Now;

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



        public JsonResult ViewShopeeOrders([FromQuery] DateTime dateFrom, DateTime dateTo, string filter)
        {

            var result = new List<OrderHeaderClass>();

            if (!String.IsNullOrEmpty(filter))
            {
                switch (filter)
                {
                    case "good":
                        //var results = _userInfoConn.ordersTable.Where(u => u.exemption == 0 && (u.dateProcess >= dateFrom && u.dateProcess <= dateTo) && u.module == "lazada").GroupBy(u => u.orderId, u => 1).Select(lazada => new { orderId = lazada.Key, count = lazada.Count() }).ToList();
                        result = _userInfoConn.orderTableHeader.Where(u => u.exception == 0 && (u.dateCreatedAt >= dateFrom && u.dateCreatedAt <= dateTo) && u.module == "shopee").ToList();
                        break;
                    case "bad":
                        result = _userInfoConn.orderTableHeader.Where(u => u.exception == 1 && (u.dateCreatedAt >= dateFrom && u.dateCreatedAt <= dateTo) && u.module == "shopee").ToList();
                        break;
                }

            }
            else
            {
                result = _userInfoConn.orderTableHeader.Where(u => (u.dateCreatedAt >= dateFrom && u.dateCreatedAt <= dateTo) && u.module == "shopee").ToList();
            }

            _userInfoConn.Dispose();
            return Json(new { set = result });
        }
        public JsonResult GetItemShopee([FromQuery] string order_id)
        {
            var result = new List<OrderClass>();
            result = _userInfoConn.ordersTable.Where(p => p.orderId == order_id && p.module == "shopee").ToList();
            return Json(new { set = result });


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
                dt = _userInfoConn.ordersTable.Where(p => p.module == "shopee" && p.typeOfexception.Contains("OOS")).ToList();
                parameters.Add("prm", "OUT OF STOCK");
                parameters.Add("module", "SHOPEE");
                path = $"{this._webHostEnvironment.WebRootPath}\\Reports\\ShopeeOrderReportOos.rdlc";
            }
            //if (categ == "SKU")
            //{
            //    dt = _userInfoConn.ordersTable.Where(p => p.module == "shopee" && p.typeOfexception.Contains("SKU")).ToList();
            //    parameters.Add("prm", "SKU DISCREPANCY");
            //    parameters.Add("module", "SHOPEE");
            //    path = $"{this._webHostEnvironment.WebRootPath}\\Reports\\ShopeeOrderReportSku.rdlc";
            //}
            if (categ == "NIB")
            {
                dt = _userInfoConn.ordersTable.Where(p => p.module == "shopee" && p.typeOfexception.Contains("NIB")).ToList();
                parameters.Add("prm", "NOT IN BASEMENT");
                parameters.Add("module", "SHOPEE");
                path = $"{this._webHostEnvironment.WebRootPath}\\Reports\\ShopeeOrderReportNob.rdlc";
            }
            //if (categ == "PRC")
            //{
            //    dt = _userInfoConn.ordersTable.Where(p => p.module == "shopee" && p.typeOfexception.Contains("PRC")).ToList();
            //    parameters.Add("prm", "PRICE DISCREPANCY");
            //    parameters.Add("module", "SHOPEE");
            //    path = $"{this._webHostEnvironment.WebRootPath}\\Reports\\ShopeeOrderReportPrc.rdlc";
            //}

            LocalReport localReport = new LocalReport(path);
            localReport.AddDataSource("LazadaOrder", dt);

            var result = localReport.Execute(RenderType.Pdf, extension, parameters, mimetype);
            return File(result.MainStream, "application/pdf");

        }
        public IActionResult MoveToPickerShopee(string orderId)
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

        public IActionResult GetAuth()
        {
            var result = new List<AuthClass>();
            result = _userInfoConn.authTable.ToList();

            ViewBag.AuthInfo = result;
            return View();
        }

        public JsonResult GetAuthCode()
        {

            DateTime now = DateTime.Now;

            long timestamp = now.ToTimestamp();


            string redirect = _configuration["Infrastructure:ShopeeApi:v2:Auth:RedirectUrl"];

            long partnerId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:PartnerId");

            var sign = ShopeeApiUtil.SignAuthRequest(
                                partnerKey: _configuration["Infrastructure:ShopeeApi:v2:PartnerKey"],
                                partnerId: partnerId.ToString(),
                                apiPath: _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Auth:ShopAuth:ApiPath"],
                                timestamp: timestamp.ToString());

            var link = "https://partner.shopeemobile.com/api/v2/shop/auth_partner?partner_id=" + partnerId + "&timestamp=" + timestamp + "&sign=" + sign + "&redirect=" + redirect;

            return Json(new { responseText = link });
        }

        public JsonResult SaveAuth(string code)
        {
            var authTable = new AuthClass();

            authTable.authCode = code;
            authTable.dateEntry = DateTime.Now;
            authTable.module = "shopee";
            _userInfoConn.Add(authTable);
            _userInfoConn.SaveChanges();







            return Json(new { set = authTable });
        }


        public JsonResult GetDoneClearedOrders()
        {
            string module = "shopee";
            IEnumerable<GridOrderHeaderClass> items = new List<GridOrderHeaderClass>();
            items = _dataAccess.ExecuteSP2<GridOrderHeaderClass, dynamic>("GetDoneClearedOrders", new { module });
            return Json(new { set = items });

        }

        public JsonResult GetDoneBoxOrders()
        {
            string module = "shopee";
            IEnumerable<GridOrderHeaderClass> items = new List<GridOrderHeaderClass>();
            items = _dataAccess.ExecuteSP2<GridOrderHeaderClass, dynamic>("GetDoneBoxOrders", new { module });
            return Json(new { set = items });

        }

        public JsonResult GetCurrentlyPickingOrders()
        {
            string module = "shopee";
            IEnumerable<GridOrderHeaderClass> items = new List<GridOrderHeaderClass>();
            items = _dataAccess.ExecuteSP2<GridOrderHeaderClass, dynamic>("GetCurrentlyPickingOrders", new { module });
            return Json(new { set = items });

        }




        public async Task<IActionResult> GetCanceledOrderList(string access_token)
        {


            var latestDate = new DateTime?();
            latestDate = _userInfoConn.orderTableHeader.Where(u => u.module == "shopee").ToList().Max(x => x.dateCreatedAt);

            var dateTo = DateTime.Now;
            var dateFrom = latestDate ?? DateTime.Today;

            dateFrom = dateFrom.AddMinutes(-10);

            using (LogContext.PushProperty("Scope", "Shopee Api"))
            {

                if (dateTo.Subtract(dateFrom).Days > 15)
                {
                    dateFrom = DateTime.Today.AddDays(-14);
                }


                var shopee_csd = _configuration.GetConnectionString("Myconnection");
                using var shopee_connsd = new SqlConnection(shopee_csd);
                shopee_connsd.Open();

                string url = _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:GetOrder:Url"] ?? "";


                DateTime now = DateTime.Now;

                long timestamp = now.ToTimestamp();




                long dateFrom_timestamp = dateFrom.ToTimestamp();

                long dateTo_timestamp = dateTo.ToTimestamp();

                long partnerId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:PartnerId"),
                    shopId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:ShopId");

                var sign = ShopeeApiUtil.SignShopRequest(
                    partnerId: partnerId.ToString(),
                    apiPath: _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:GetOrder:ApiPath"] ?? "",
                    timestamp: timestamp.ToString(),
                    access_token: access_token,
                    shopid: shopId,
                    partnerKey: _configuration["Infrastructure:ShopeeApi:v2:PartnerKey"] ?? "");

                using HttpClient client = _httpClientFactory.CreateClient(name: _configuration["Infrastructure:ShopeeApi:ConnectionName"] ?? "");

                string responseContentString = string.Empty;

                Stopwatch stopwatch = Stopwatch.StartNew();

                try
                {
                    _logger.LogTrace($"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&order_status=CANCELLED&sign={sign}&time_range_field=create_time&time_from={dateFrom_timestamp}&time_to={dateTo_timestamp}&page_size=50");
                    using HttpResponseMessage httpResponse = await _policy.ExecuteAsync(async (ct) => await client.GetAsync(
                        requestUri: $"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&order_status=CANCELLED&sign={sign}&time_range_field=create_time&time_from={dateFrom_timestamp}&time_to={dateTo_timestamp}&page_size=50",
                        cancellationToken: ct),
                        cancellationToken: cancellationToken);

                    stopwatch.Stop();

                    responseContentString = await httpResponse.Content.ReadAsStringAsync();

                    _logger.LogTrace(message: "Url:{Url}, Duration: {duration}s, Response Status: {responseStatus}, Responnse Body:{response}",
                        url,
                        (stopwatch.ElapsedMilliseconds / 1000m).ToString("#,##0.###"),
                        httpResponse.StatusCode.ToString(),
                        responseContentString);

                    /////////////////////////////////start of fetching order details///////////////////////
                    var responseJson = JObject.Parse(responseContentString);
                    if (responseJson.ContainsKey("message"))

                        if (responseJson.ContainsKey("response"))
                        {
                            JArray array = (JArray)(responseJson["response"]?["order_list"] ?? "");
                            if (array.Count > 0)
                            {
                                var myList = new List<string>();
                                foreach (JToken jtoken in array)
                                {

                                    try
                                    {

                                        string ordersn = jtoken.Value<string>("order_sn") ?? "";


                                        var CanceledOrdersData = new List<CanceledOrders>();
                                        CanceledOrdersData = _userInfoConn.CanceledOrders.Where(e => e.orderId == ordersn).ToList();

                                        if (CanceledOrdersData.Count() < 1)
                                        {
                                            var canceledorder = new CanceledOrders();

                                            canceledorder.orderId = ordersn;
                                            canceledorder.dateFetch = DateTime.Now;
                                            canceledorder.dateProcess = DateTime.Now;
                                            canceledorder.dateCreatedAt = DateTime.Now;
                                            canceledorder.module = "shopee";
                                            canceledorder.status = "CANCELLED";
                                            canceledorder.item_count = 0;
                                            canceledorder.total_amount = 0;
                                            _userInfoConn.Add(canceledorder);
                                            _userInfoConn.SaveChanges();


                                            var ordersTableForCancelled = new List<OrderClass>();
                                            ordersTableForCancelled = _userInfoConn.ordersTable.Where(e => e.orderId == ordersn && (e.typeOfexception == "NOF" || e.typeOfexception == "OOS")).ToList();

                                            if (ordersTableForCancelled.Count > 0)
                                            {
                                                var cs = _configuration.GetConnectionString("Myconnection");
                                                using var conns = new SqlConnection(cs);
                                                conns.Open();


                                                string Upc = $"EXEC InsertCancelledOrders @orderId='{ordersn}'";
                                                using var cmddd = new SqlCommand(Upc, conns);
                                                var upcresult = cmddd.ExecuteScalar();

                                                conns.Close();
                                            }

                                        }
                                        else
                                        {
                                            var ordersTableForCancelled = new List<OrderClass>();
                                            ordersTableForCancelled = _userInfoConn.ordersTable.Where(e => e.orderId == ordersn && (e.typeOfexception == "NOF" || e.typeOfexception == "OOS")).ToList();

                                            if (ordersTableForCancelled.Count > 0)
                                            {
                                                var cs = _configuration.GetConnectionString("Myconnection");
                                                using var conns = new SqlConnection(cs);
                                                conns.Open();


                                                string Upc = $"EXEC InsertCancelledOrders @orderId='{ordersn}'";
                                                using var cmddd = new SqlCommand(Upc, conns);
                                                var upcresult = cmddd.ExecuteScalar();

                                                conns.Close();
                                            }
                                        }


                                    }
                                    catch (FormatException)
                                    {
                                        throw new ShopeeApiException(message: "An error occured while accessing shopee API")
                                        {
                                            RequestUrl = url ?? "",
                                            RequestMethod = "POST",
                                            ResponseDescription = responseContentString,
                                            Timestamp = now,
                                            HttpStatusCode = httpResponse.StatusCode
                                        };
                                    }
                                }

                            }
                        }
                }


                catch (Exception ex)
                {

                    throw new ShopeeApiException("An error occured while accessing shopee API", ex)
                    {
                        RequestUrl = url,
                        RequestMethod = "POST",
                        Timestamp = now
                    };
                }
                //_userInfoConn.Dispose();
                shopee_connsd.Close();
                //shopee_connectionPos.Close();
            }

            return Json(new { responseText = "Success" });
        }


        public async Task<IActionResult> GetShipmentList(string orderId)
        {



            using (LogContext.PushProperty("Scope", "Shopee Api"))
            {


                var shopee_csd = _configuration.GetConnectionString("Myconnection");
                using var shopee_connsd = new SqlConnection(shopee_csd);
                shopee_connsd.Open();

                string shopee_sql_token = "SELECT TOP 1 accessToken FROM shopeeToken order by entryId Desc";
                using var shopee_cmd_token = new SqlCommand(shopee_sql_token, shopee_connsd);
                var access_token = (shopee_cmd_token.ExecuteScalar()).ToString();

                string url = _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:getShippingParam:Url"] ?? "";

                DateTime now = DateTime.Now;

                long timestamp = now.ToTimestamp();





                long partnerId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:PartnerId"),
                    shopId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:ShopId");

                var sign = ShopeeApiUtil.SignShopRequest(
                    partnerId: partnerId.ToString(),
                    apiPath: _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:getShippingParam:ApiPath"] ?? "",
                    timestamp: timestamp.ToString(),
                access_token: access_token,
                shopid: shopId,
                    partnerKey: _configuration["Infrastructure:ShopeeApi:v2:PartnerKey"] ?? "");

                using HttpClient client = _httpClientFactory.CreateClient(name: _configuration["Infrastructure:ShopeeApi:ConnectionName"] ?? "");

                string responseContentString = string.Empty;

                Stopwatch stopwatch = Stopwatch.StartNew();
                try
                {
                    _logger.LogTrace($"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&sign={sign}&shop_id={shopId}&order_sn={orderId}");
                    using HttpResponseMessage httpResponse = await _policy.ExecuteAsync(async (ct) => await client.GetAsync(
                         requestUri: $"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&sign={sign}&shop_id={shopId}&order_sn={orderId}",
                        cancellationToken: ct),
                        cancellationToken: cancellationToken);

                    stopwatch.Stop();

                    responseContentString = await httpResponse.Content.ReadAsStringAsync();

                    _logger.LogTrace(message: "Url:{Url}, Duration: {duration}s, Response Status: {responseStatus}, Responnse Body:{response}",
                        url,
                        (stopwatch.ElapsedMilliseconds / 1000m).ToString("#,##0.###"),
                        httpResponse.StatusCode.ToString(),
                        responseContentString);

                    /////////////////////////////////start of fetching order details///////////////////////
                    var responseJson = JObject.Parse(responseContentString);
                    if (responseJson.ContainsKey("message"))
                    {
                        if (responseJson.ContainsKey("response"))
                        {
                            // Define your URL and parameters

                            timestamp = now.ToTimestamp();

                            using HttpClient client2 = new HttpClient();

                            var address_id = (responseJson["response"]?["pickup"]?["address_list"]?[0]?["address_id"] ?? "").ToString();
                            var pickup_time_id = (responseJson["response"]?["pickup"]?["address_list"]?[0]?["time_slot_list"]?[0]?["pickup_time_id"] ?? "").ToString();


                            var array = new object[]
                               {
                                    new
                                    {
                                        ordersn = orderId,
                                        package_number = "",
                                        pickup = new
                                        {
                                            address_id = address_id,
                                            pickup_time_id = pickup_time_id,
                                            tracking_number = ""
                                        }
                                    }
                               };
                            string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(array);

                            string fullUrl = $"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&sign={sign}&shop_id={shopId}";

                            // Execute the GET request with retries using the defined policy
                            //using HttpResponseMessage httpResponse2 = await _policy.ExecuteAsync(async (ct) => await client.GetAsync(fullUrl, cancellationToken: ct), cancellationToken: CancellationToken.None);

                            using HttpResponseMessage httpResponse2 = await _policy.ExecuteAsync(async (ct) =>
                            {
                                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                                return await client.PostAsync(fullUrl, content, ct);
                            }, CancellationToken.None);

                            // Check the response and handle it as needed
                            if (httpResponse.IsSuccessStatusCode)
                            {
                                // Read the response content
                                string responseContent = await httpResponse2.Content.ReadAsStringAsync();
                                Console.WriteLine(responseContent);
                            }
                            else
                            {
                                // Handle the non-successful response
                                Console.WriteLine($"HTTP Error: {httpResponse2.StatusCode}");
                            }

                        }

                    }
                }


                catch (Exception ex)
                {

                    throw new ShopeeApiException("An error occured while accessing shopee API", ex)
                    {
                        RequestUrl = url,
                        RequestMethod = "POST",
                        Timestamp = now
                    };
                }
                //_userInfoConn.Dispose();
                shopee_connsd.Close();
                //shopee_connectionPos.Close();
            }

            return Json(new { responseText = "Success" });
        }

        public JsonResult GetCurrentlyPackingOrders()
        {
            string module = "shopee";
            IEnumerable<GridOrderHeaderClass> items = new List<GridOrderHeaderClass>();
            items = _dataAccess.ExecuteSP2<GridOrderHeaderClass, dynamic>("GetCurrentlyPackingOrders", new { module });
            return Json(new { set = items });

        }

        public IActionResult sampleSignalR()
        {
            IEnumerable<DiscrepancyCenterViewModel> items = new List<DiscrepancyCenterViewModel>();
            items = _dataAccess.ExecuteSP2<DiscrepancyCenterViewModel, dynamic>("GetDiscrepancyCount", new { });
            int? count = 0;
            foreach (var item in items)
            {
                count = item.boxId;
            }
            _chatHub.Clients.All.RecieveOrders("from controller", count);
            return Ok();
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
