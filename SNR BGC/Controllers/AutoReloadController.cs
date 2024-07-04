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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Newtonsoft.Json;
using Microsoft.CodeAnalysis;

namespace SNR_BGC.Controllers
{
    public class AutoReloadController : Controller
    {
        private readonly IConfiguration _configuration;

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
        public AutoReloadController(IConfiguration configuration,
            UserClass tokenInfo,
            IWebHostEnvironment webHostEnvironment,
            ILogger<ShopeeController> logger,
            IHttpClientFactory httpClientFactory,
            IAsyncPolicy policy,
            IAuthenthicationTokenProvider authenthicationTokenProvider,
            IHostApplicationLifetime hostApplicationLifetime,
            IServiceScopeFactory factory)
        {
            _configuration = configuration;

            _webHostEnvironment = webHostEnvironment;

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            _pageSize = _configuration.GetValue<int>("ShopeeApi:v1:EndPoints:Item:GetItemsList:MaxPageSize");

            _baseUrl = _configuration["ShopeeApi:v1:EndPoints:Orders:GetOrderList:Url"];

            _baseUrlorderDetails = _configuration["ShopeeApi:v1:EndPoints:Orders:GetOrderDetails:Url"];


            _userInfoConn = tokenInfo;

            _logger = logger;

            _authenthicationTokenProvider = authenthicationTokenProvider;

            this._hostApplicationLifetime = hostApplicationLifetime;

            _httpClientFactory = httpClientFactory;

            _policy = policy;

            _scope = factory.CreateScope();


        }


        public IActionResult IndexLazada(string result)
        {
            return View();
        }
        public IActionResult IndexShopee(string result)
        {
            return View();
        }
        public IActionResult IndexReRun(string result)
        {
            return View();
        }
        public IActionResult IndexCatchLazada(string result)
        {
            return View();
        }
        public IActionResult IndexCatchShopee(string result)
        {
            return View();
        }

        public async Task<IActionResult> GetCanceledOrderList(string access_token)
        {




            var dateFrom = DateTime.Now;
            dateFrom = dateFrom.AddDays(-2);

            var dateTo = DateTime.Now;


            //dateFrom = dateFrom.AddMinutes(-10);

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
                    _logger.LogTrace($"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&order_status=CANCELLED&sign={sign}&time_range_field=update_time&time_from={dateFrom_timestamp}&time_to={dateTo_timestamp}&page_size=50");
                    using HttpResponseMessage httpResponse = await _policy.ExecuteAsync(async (ct) => await client.GetAsync(
                        requestUri: $"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&order_status=CANCELLED&sign={sign}&time_range_field=update_time&time_from={dateFrom_timestamp}&time_to={dateTo_timestamp}&page_size=50",
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
                                        }

                                        var cs = _configuration.GetConnectionString("Myconnection");
                                        using var conns = new SqlConnection(cs);
                                        conns.Open();


                                        string query = $"EXEC InsertCancelledOrders @orderId='{ordersn}'";
                                        using var cmddd = new SqlCommand(query, conns);
                                        var result = cmddd.ExecuteScalar();

                                        conns.Close();


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

        public async Task<JsonResult> GetOrdersShopee([FromQuery] DateTime dateFrom, DateTime dateTo, CancellationToken stoppingToken)
        {
            int insertedEntity = 0;

            var autoReloadLogs = new AutoReloadLogs();
            autoReloadLogs.platform = "Shopee";
            autoReloadLogs.dateProcess = DateTime.Now;
            autoReloadLogs.status = "Good";
            autoReloadLogs.agent = "AutoReload";
            _userInfoConn.Add(autoReloadLogs);
            _userInfoConn.SaveChanges();


            insertedEntity = autoReloadLogs.id;


            Infrastructure.External.ShopeeWebApi.AuthenticationToken token = await _authenthicationTokenProvider.GetAuthenticationToken(stoppingToken);
            var result = await GetShopeeOrderList(dateFrom, dateTo, token.AccessToken);
            //await GetLazadaOrderList(dateFrom, dateFrom, dateTo);

            dynamic resultValue = result.Value;

            string setResult = resultValue.set;
            int ordersCount = resultValue.ordersCount;
            string coverage = resultValue.coverage;
            DateTime coverageFrom = resultValue.coverageFrom;
            DateTime coverageTo = resultValue.coverageTo;


            var autoReloadLogs2 = _userInfoConn.AutoReloadLogs.FirstOrDefault(e => e.id == insertedEntity);

            if (autoReloadLogs2 != null)
            {
                autoReloadLogs2.logs = setResult == "Success" ? "Successful Run" : setResult;
                autoReloadLogs2.status = setResult == "Success" ? "Success" : "Failed";
                autoReloadLogs2.totalOrder = ordersCount;
                autoReloadLogs2.completion = DateTime.Now;
                autoReloadLogs2.coverage = coverage;
                autoReloadLogs2.GetOrdersDone = "Done";
                autoReloadLogs.coverageFrom = coverageFrom;
                autoReloadLogs.coverageTo = coverageTo;

                _userInfoConn.SaveChanges();
            }



            var boxOrdersShopee = _userInfoConn.boxOrders.Where(e => e.boxerStatus == "Done" && e.platformStatus == null && e.module == "shopee").Select(a => a.orderId).Distinct();

            for (var i = 0; i < boxOrdersShopee.Count(); i++)
            {

                await ShopeePickupStatus(token.AccessToken, boxOrdersShopee.ToArray()[i]);
            }


            //var boxOrdersLazada = _userInfoConn.boxOrders.Where(e => e.boxerStatus == "Done" && e.platformStatus == null && e.module == "lazada").Select(a => a.orderId).Distinct();
            //for (var i = 0; i < boxOrdersLazada.Count(); i++)
            //{

            //    await LazadaPickupStatus(boxOrdersLazada.ToArray()[i]);
            //}

            var boxOrdersShopeeTracking = _userInfoConn.boxOrders.Where(e => e.boxerStatus == "Done" && (e.trackingNo == "" || e.trackingNo == null) && e.module == "shopee").Select(a => a.orderId).Distinct();
            for (var i = 0; i < boxOrdersShopeeTracking.Count(); i++)
            {

                await ShopeeTrackingNo(token.AccessToken, boxOrdersShopeeTracking.ToArray()[i]);
            }



            var autoReloadLogs3 = _userInfoConn.AutoReloadLogs.FirstOrDefault(e => e.id == insertedEntity);

            if (autoReloadLogs3 != null)
            {
                autoReloadLogs3.GetDispatchDone = "Done";

                _userInfoConn.SaveChanges();
            }




            return Json(new { set = "Success" });
        }
        public async Task<JsonResult> GetOrdersCatchShopee([FromQuery] DateTime dateFrom, DateTime dateTo, CancellationToken stoppingToken)
        {
            int insertedEntity = 0;

            var autoReloadLogs = new AutoReloadLogs();
            autoReloadLogs.platform = "Shopee";
            autoReloadLogs.dateProcess = DateTime.Now;
            autoReloadLogs.status = "Good";
            autoReloadLogs.agent = "AutoReloadCatch";
            _userInfoConn.Add(autoReloadLogs);
            _userInfoConn.SaveChanges();


            insertedEntity = autoReloadLogs.id;


            var shopee_csd = _configuration.GetConnectionString("Myconnection");
            using var shopee_connsd = new SqlConnection(shopee_csd);
            shopee_connsd.Open();

            string shopee_sql_token = "SELECT TOP 1 accessToken FROM shopeeToken order by entryId Desc";
            using var shopee_cmd_token = new SqlCommand(shopee_sql_token, shopee_connsd);
            var access_token = shopee_cmd_token.ExecuteScalar();
            shopee_connsd.Close();

            //Infrastructure.External.ShopeeWebApi.AuthenticationToken token = await _authenthicationTokenProvider.GetAuthenticationToken(stoppingToken);
            var result = await GetShopeeOrderListCatch(dateFrom, dateTo, access_token.ToString());
            //await GetLazadaOrderList(dateFrom, dateFrom, dateTo);

            dynamic resultValue = result.Value;

            string setResult = resultValue.set;
            int ordersCount = resultValue.ordersCount;
            string coverage = resultValue.coverage;
            DateTime coverageFrom = resultValue.coverageFrom;
            DateTime coverageTo = resultValue.coverageTo;


            var autoReloadLogs2 = _userInfoConn.AutoReloadLogs.FirstOrDefault(e => e.id == insertedEntity);

            if (autoReloadLogs2 != null)
            {
                autoReloadLogs2.logs = setResult == "Success" ? "Successful Run" : setResult;
                autoReloadLogs2.status = setResult == "Success" ? "Success" : "Failed";
                autoReloadLogs2.totalOrder = ordersCount;
                autoReloadLogs2.completion = DateTime.Now;
                autoReloadLogs2.coverage = coverage;
                autoReloadLogs2.GetOrdersDone = "Done";
                autoReloadLogs.coverageFrom = coverageFrom;
                autoReloadLogs.coverageTo = coverageTo;

                _userInfoConn.SaveChanges();
            }











            return Json(new { set = "Success" });
        }

        public async Task<JsonResult> GetOrdersCatchLazada([FromQuery] DateTime dateFrom, DateTime dateTo, CancellationToken stoppingToken)
        {

            int insertedEntity = 0;

            var autoReloadLogs = new AutoReloadLogs();
            autoReloadLogs.platform = "Lazada";
            autoReloadLogs.dateProcess = DateTime.Now;
            autoReloadLogs.status = "Good";
            autoReloadLogs.agent = "AutoReloadCatch";
            _userInfoConn.Add(autoReloadLogs);
            _userInfoConn.SaveChanges();


            insertedEntity = autoReloadLogs.id;
            //Infrastructure.External.ShopeeWebApi.AuthenticationToken token = await _authenthicationTokenProvider.GetAuthenticationToken(stoppingToken);
            //await GetShopeeOrderList(dateFrom, dateTo, token.AccessToken);
            var result = await GetLazadaOrderListCatch(dateFrom, dateFrom, dateTo);

            dynamic resultValue = result.Value;

            string setResult = resultValue.set;
            int ordersCount = resultValue.ordersCount;
            string coverage = resultValue.coverage;
            DateTime coverageFrom = resultValue.coverageFrom;
            DateTime coverageTo = resultValue.coverageTo;

            //var boxOrdersShopee = _userInfoConn.boxOrders.Where(e => e.boxerStatus == "Done" && e.platformStatus == null && e.module == "shopee").Select(a => a.orderId).Distinct();

            //for (var i = 0; i < boxOrdersShopee.Count(); i++)
            //{

            //    await ShopeePickupStatus(token.AccessToken, boxOrdersShopee.ToArray()[i]);
            //}
            if (setResult != "Success")
            {
                var result2 = await GetLazadaOrderListCatch(dateFrom, dateFrom, dateTo);

                dynamic resultValue2 = result2.Value;

                setResult = resultValue2.set;
                ordersCount = resultValue2.ordersCount;
                coverage = resultValue2.coverage;
                coverageFrom = resultValue2.coverageFrom;
                coverageTo = resultValue2.coverageTo;

                if (setResult != "Success")
                {
                    var result3 = await GetLazadaOrderListCatch(dateFrom, dateFrom, dateTo);

                    dynamic resultValue3 = result3.Value;

                    setResult = resultValue3.set;
                    ordersCount = resultValue3.ordersCount;
                    coverage = resultValue3.coverage;
                    coverageFrom = resultValue3.coverageFrom;
                    coverageTo = resultValue3.coverageTo;


                    var autoReloadLogs2 = _userInfoConn.AutoReloadLogs.FirstOrDefault(e => e.id == insertedEntity);

                    if (autoReloadLogs2 != null)
                    {
                        autoReloadLogs2.logs = setResult == "Success" ? "Successful Run" : setResult;
                        autoReloadLogs2.status = setResult == "Success" ? "Success" : "Failed";
                        autoReloadLogs2.totalOrder = ordersCount;
                        autoReloadLogs2.completion = DateTime.Now;
                        autoReloadLogs2.coverage = coverage;
                        autoReloadLogs2.GetOrdersDone = "Done";
                        autoReloadLogs.coverageFrom = coverageFrom;
                        autoReloadLogs.coverageTo = coverageTo;

                        _userInfoConn.SaveChanges();
                    }

                }
                else
                {

                    var autoReloadLogs2 = _userInfoConn.AutoReloadLogs.FirstOrDefault(e => e.id == insertedEntity);

                    if (autoReloadLogs2 != null)
                    {
                        autoReloadLogs2.logs = setResult == "Success" ? "Successful Run" : setResult;
                        autoReloadLogs2.status = setResult == "Success" ? "Success" : "Failed";
                        autoReloadLogs2.totalOrder = ordersCount;
                        autoReloadLogs2.completion = DateTime.Now;
                        autoReloadLogs2.coverage = coverage;
                        autoReloadLogs2.GetOrdersDone = "Done";
                        autoReloadLogs.coverageFrom = coverageFrom;
                        autoReloadLogs.coverageTo = coverageTo;

                        _userInfoConn.SaveChanges();
                    }
                }
            }
            else
            {
                var autoReloadLogs2 = _userInfoConn.AutoReloadLogs.FirstOrDefault(e => e.id == insertedEntity);

                if (autoReloadLogs2 != null)
                {
                    autoReloadLogs2.logs = setResult == "Success" ? "Successful Run" : setResult;
                    autoReloadLogs2.status = setResult == "Success" ? "Success" : "Failed";
                    autoReloadLogs2.totalOrder = ordersCount;
                    autoReloadLogs2.completion = DateTime.Now;
                    autoReloadLogs2.coverage = coverage;
                    autoReloadLogs2.GetOrdersDone = "Done";
                    autoReloadLogs.coverageFrom = coverageFrom;
                    autoReloadLogs.coverageTo = coverageTo;

                    _userInfoConn.SaveChanges();
                }
            }



            //var boxOrdersShopeeTracking = _userInfoConn.boxOrders.Where(e => e.boxerStatus == "Done" && (e.trackingNo == "" || e.trackingNo == null) && e.module == "shopee").Select(a => a.orderId).Distinct();
            //for (var i = 0; i < boxOrdersShopeeTracking.Count(); i++)
            //{

            //    await ShopeeTrackingNo(token.AccessToken, boxOrdersShopeeTracking.ToArray()[i]);
            //}





            return Json(new { set = "Success" });
        }
        public async Task<JsonResult> GetOrdersLazada([FromQuery] DateTime dateFrom, DateTime dateTo, CancellationToken stoppingToken)
        {
            //var dateNow = DateTime.Now;
            //var arLogs = _userInfoConn.AutoReloadLogs.OrderByDescending(o => o.id).FirstOrDefault(w => w.platform.ToLower() == "lazada");
            //if (arLogs != null)
            //if ((dateNow - arLogs.dateProcess.Value).TotalMinutes < 10)
            //{
            //    return Json(new { set = "Success" });
            //}

            int insertedEntity = 0;

            var autoReloadLogs = new AutoReloadLogs();
            autoReloadLogs.platform = "Lazada";
            autoReloadLogs.dateProcess = DateTime.Now;
            autoReloadLogs.status = "Good";
            autoReloadLogs.agent = "AutoReload";
            _userInfoConn.Add(autoReloadLogs);
            _userInfoConn.SaveChanges();


            insertedEntity = autoReloadLogs.id;
            //Infrastructure.External.ShopeeWebApi.AuthenticationToken token = await _authenthicationTokenProvider.GetAuthenticationToken(stoppingToken);
            //await GetShopeeOrderList(dateFrom, dateTo, token.AccessToken);
            var result = await GetLazadaOrderList(dateFrom, dateFrom, dateTo);

            dynamic resultValue = result.Value;

            string setResult = resultValue.set;
            int ordersCount = resultValue.ordersCount;
            string coverage = resultValue.coverage;
            DateTime coverageFrom = resultValue.coverageFrom;
            DateTime coverageTo = resultValue.coverageTo;

            //var boxOrdersShopee = _userInfoConn.boxOrders.Where(e => e.boxerStatus == "Done" && e.platformStatus == null && e.module == "shopee").Select(a => a.orderId).Distinct();

            //for (var i = 0; i < boxOrdersShopee.Count(); i++)
            //{

            //    await ShopeePickupStatus(token.AccessToken, boxOrdersShopee.ToArray()[i]);
            //}
            if (setResult != "Success")
            {
                var result2 = await GetLazadaOrderList(dateFrom, dateFrom, dateTo);

                dynamic resultValue2 = result2.Value;

                setResult = resultValue2.set;
                ordersCount = resultValue2.ordersCount;
                coverage = resultValue2.coverage;
                coverageFrom = resultValue2.coverageFrom;
                coverageTo = resultValue2.coverageTo;

                if (setResult != "Success")
                {
                    var result3 = await GetLazadaOrderList(dateFrom, dateFrom, dateTo);

                    dynamic resultValue3 = result3.Value;

                    setResult = resultValue3.set;
                    ordersCount = resultValue3.ordersCount;
                    coverage = resultValue3.coverage;
                    coverageFrom = resultValue3.coverageFrom;
                    coverageTo = resultValue3.coverageTo;


                    var autoReloadLogs2 = _userInfoConn.AutoReloadLogs.FirstOrDefault(e => e.id == insertedEntity);

                    if (autoReloadLogs2 != null)
                    {
                        autoReloadLogs2.logs = setResult == "Success" ? "Successful Run" : setResult;
                        autoReloadLogs2.status = setResult == "Success" ? "Success" : "Failed";
                        autoReloadLogs2.totalOrder = ordersCount;
                        autoReloadLogs2.completion = DateTime.Now;
                        autoReloadLogs2.coverage = coverage;
                        autoReloadLogs2.GetOrdersDone = "Done";
                        autoReloadLogs.coverageFrom = coverageFrom;
                        autoReloadLogs.coverageTo = coverageTo;

                        _userInfoConn.SaveChanges();
                    }

                }
                else
                {

                    var autoReloadLogs2 = _userInfoConn.AutoReloadLogs.FirstOrDefault(e => e.id == insertedEntity);

                    if (autoReloadLogs2 != null)
                    {
                        autoReloadLogs2.logs = setResult == "Success" ? "Successful Run" : setResult;
                        autoReloadLogs2.status = setResult == "Success" ? "Success" : "Failed";
                        autoReloadLogs2.totalOrder = ordersCount;
                        autoReloadLogs2.completion = DateTime.Now;
                        autoReloadLogs2.coverage = coverage;
                        autoReloadLogs2.GetOrdersDone = "Done";
                        autoReloadLogs.coverageFrom = coverageFrom;
                        autoReloadLogs.coverageTo = coverageTo;

                        _userInfoConn.SaveChanges();
                    }
                }
            }
            else
            {
                var autoReloadLogs2 = _userInfoConn.AutoReloadLogs.FirstOrDefault(e => e.id == insertedEntity);

                if (autoReloadLogs2 != null)
                {
                    autoReloadLogs2.logs = setResult == "Success" ? "Successful Run" : setResult;
                    autoReloadLogs2.status = setResult == "Success" ? "Success" : "Failed";
                    autoReloadLogs2.totalOrder = ordersCount;
                    autoReloadLogs2.completion = DateTime.Now;
                    autoReloadLogs2.coverage = coverage;
                    autoReloadLogs2.GetOrdersDone = "Done";
                    autoReloadLogs.coverageFrom = coverageFrom;
                    autoReloadLogs.coverageTo = coverageTo;

                    _userInfoConn.SaveChanges();
                }
            }

            var boxOrdersLazada = _userInfoConn.boxOrders.Where(e => e.boxerStatus == "Done" && e.platformStatus == null && e.module == "lazada").Select(a => a.orderId).Distinct();
            for (var i = 0; i < boxOrdersLazada.Count(); i++)
            {

                await LazadaPickupStatus(boxOrdersLazada.ToArray()[i]);
            }

            //var boxOrdersShopeeTracking = _userInfoConn.boxOrders.Where(e => e.boxerStatus == "Done" && (e.trackingNo == "" || e.trackingNo == null) && e.module == "shopee").Select(a => a.orderId).Distinct();
            //for (var i = 0; i < boxOrdersShopeeTracking.Count(); i++)
            //{

            //    await ShopeeTrackingNo(token.AccessToken, boxOrdersShopeeTracking.ToArray()[i]);
            //}
            var autoReloadLogs3 = _userInfoConn.AutoReloadLogs.FirstOrDefault(e => e.id == insertedEntity);

            if (autoReloadLogs3 != null)
            {

                autoReloadLogs3.GetDispatchDone = "Done";

                _userInfoConn.SaveChanges();
            }



            return Json(new { set = "Success" });
        }


        public async Task<JsonResult> GetOrdersReRun([FromQuery] DateTime dateFrom, DateTime dateTo, CancellationToken stoppingToken)
        {
            try
            {
                var shopee_csd = _configuration.GetConnectionString("Myconnection");
                using var shopee_connsd = new SqlConnection(shopee_csd);
                shopee_connsd.Open();

                string shopee_sql_token = "SELECT TOP 1 accessToken FROM shopeeToken order by entryId Desc";
                using var shopee_cmd_token = new SqlCommand(shopee_sql_token, shopee_connsd);
                var access_token = shopee_cmd_token.ExecuteScalar();
                shopee_connsd.Close();

                //var autoreloadlogs = new List<AutoReloadLogs>();
                //autoreloadlogs = _userInfoConn.AutoReloadLogs.Where(e => e.status != "Success" && e.agent != "Manual").ToList();
                var autoreloadlogs = new List<AutoReloadLogs>();
                autoreloadlogs = _userInfoConn.AutoReloadLogs.Where(e => e.status != "Success" && e.agent != "Manual" && e.dateProcess < DateTime.Now.AddMinutes(-20)).ToList();

                var autoReloadLogs2 = new AutoReloadLogs();

                //Infrastructure.External.ShopeeWebApi.AuthenticationToken token = await _authenthicationTokenProvider.GetAuthenticationToken(stoppingToken);

                for (int i = 0; i < autoreloadlogs.Count; i++)
                {
                    if (autoreloadlogs[i].agent == "AutoReload")
                    {
                        if (autoreloadlogs[i].platform == "Lazada")
                        {
                            //var dateFromVar = (autoreloadlogs[i].dateProcess ?? DateTime.Now).AddMinutes(-41).AddSeconds(-(autoreloadlogs[i].dateProcess ?? DateTime.Now).Second).AddMilliseconds(-(autoreloadlogs[i].dateProcess ?? DateTime.Now).Millisecond);
                            //var dateToVar = (autoreloadlogs[i].dateProcess ?? DateTime.Now).AddMinutes(-30).AddSeconds(-(autoreloadlogs[i].dateProcess ?? DateTime.Now).Second).AddMilliseconds(-(autoreloadlogs[i].dateProcess ?? DateTime.Now).Millisecond); ;
                            var dateFromVar = (autoreloadlogs[i].dateProcess ?? DateTime.Now).AddMinutes(-21).AddSeconds(-(autoreloadlogs[i].dateProcess ?? DateTime.Now).Second).AddMilliseconds(-(autoreloadlogs[i].dateProcess ?? DateTime.Now).Millisecond);
                            var dateToVar = (autoreloadlogs[i].dateProcess ?? DateTime.Now).AddMinutes(-10).AddSeconds(-(autoreloadlogs[i].dateProcess ?? DateTime.Now).Second).AddMilliseconds(-(autoreloadlogs[i].dateProcess ?? DateTime.Now).Millisecond); ;
                            if (autoreloadlogs[i].coverageFrom != null)
                            {
                                dateFromVar = autoreloadlogs[i].coverageFrom ?? DateTime.Now;
                            }
                            if (autoreloadlogs[i].coverageTo != null)
                            {
                                dateToVar = autoreloadlogs[i].coverageTo ?? DateTime.Now;
                            }

                            var result = await GetLazadaOrderListReRun(dateFromVar, dateFromVar, dateToVar);


                            dynamic resultValue = result.Value;

                            string setResult = resultValue.set;
                            int ordersCount = resultValue.ordersCount;
                            string coverage = resultValue.coverage;



                            autoReloadLogs2 = _userInfoConn.AutoReloadLogs.FirstOrDefault(e => e.id == autoreloadlogs[i].id);

                            if (autoReloadLogs2 != null)
                            {
                                autoReloadLogs2.logs = setResult == "Success" ? "Successful Run" : setResult;
                                autoReloadLogs2.status = setResult == "Success" ? "Success" : "Failed";
                                autoReloadLogs2.totalOrder = ordersCount;
                                autoReloadLogs2.completion = DateTime.Now;
                                autoReloadLogs2.coverage = coverage;
                                autoReloadLogs2.GetOrdersDone = "Done";
                                autoReloadLogs2.coverageFrom = autoreloadlogs[i].coverageFrom;
                                autoReloadLogs2.coverageTo = autoreloadlogs[i].coverageTo;
                                autoReloadLogs2.fromFailed = true;

                                _userInfoConn.SaveChanges();
                            }
                        }
                        else if (autoreloadlogs[i].platform == "Shopee")
                        {


                            //var dateFromVar = (autoreloadlogs[i].dateProcess ?? DateTime.Now).AddMinutes(-41).AddSeconds(-(autoreloadlogs[i].dateProcess ?? DateTime.Now).Second).AddMilliseconds(-(autoreloadlogs[i].dateProcess ?? DateTime.Now).Millisecond);
                            //var dateToVar = (autoreloadlogs[i].dateProcess ?? DateTime.Now).AddMinutes(-30).AddSeconds(-(autoreloadlogs[i].dateProcess ?? DateTime.Now).Second).AddMilliseconds(-(autoreloadlogs[i].dateProcess ?? DateTime.Now).Millisecond);
                            var dateFromVar = (autoreloadlogs[i].dateProcess ?? DateTime.Now).AddMinutes(-21).AddSeconds(-(autoreloadlogs[i].dateProcess ?? DateTime.Now).Second).AddMilliseconds(-(autoreloadlogs[i].dateProcess ?? DateTime.Now).Millisecond);
                            var dateToVar = (autoreloadlogs[i].dateProcess ?? DateTime.Now).AddMinutes(-10).AddSeconds(-(autoreloadlogs[i].dateProcess ?? DateTime.Now).Second).AddMilliseconds(-(autoreloadlogs[i].dateProcess ?? DateTime.Now).Millisecond);
                            if (autoreloadlogs[i].coverageFrom != null)
                            {
                                dateFromVar = autoreloadlogs[i].coverageFrom ?? DateTime.Now;
                            }
                            if (autoreloadlogs[i].coverageTo != null)
                            {
                                dateToVar = autoreloadlogs[i].coverageTo ?? DateTime.Now;
                            }

                            if (autoreloadlogs[i].dateProcess < DateTime.Now.AddMinutes(-10))
                            {
                                var result = await GetShopeeOrderListReRun(dateFromVar, dateToVar, access_token.ToString());


                                dynamic resultValue = result.Value;

                                string setResult = resultValue.set;
                                int ordersCount = resultValue.ordersCount;
                                string coverage = resultValue.coverage;
                                DateTime? coverageFrom = resultValue.coverageFrom;
                                DateTime? coverageTo = resultValue.coverageTo;


                                autoReloadLogs2 = _userInfoConn.AutoReloadLogs.FirstOrDefault(e => e.id == autoreloadlogs[i].id);

                                if (autoReloadLogs2 != null)
                                {
                                    autoReloadLogs2.logs = setResult == "Success" ? "Successful Run" : setResult;
                                    autoReloadLogs2.status = setResult == "Success" ? "Success" : "Failed";
                                    autoReloadLogs2.totalOrder = ordersCount;
                                    autoReloadLogs2.completion = DateTime.Now;
                                    autoReloadLogs2.coverage = coverage;
                                    autoReloadLogs2.GetOrdersDone = "Done";
                                    autoReloadLogs2.coverageFrom = autoreloadlogs[i].coverageFrom;
                                    autoReloadLogs2.coverageTo = autoreloadlogs[i].coverageTo;
                                    autoReloadLogs2.fromFailed = true;

                                    _userInfoConn.SaveChanges();
                                }
                            }
                        }
                    }
                    else if (autoreloadlogs[i].agent == "AutoReloadCatch")
                    {
                        if (autoreloadlogs[i].platform == "Lazada")
                        {
                            var dateFromVar = (autoreloadlogs[i].dateProcess ?? DateTime.Now).AddHours(-8);
                            var dateToVar = (autoreloadlogs[i].dateProcess ?? DateTime.Now).AddHours(-4); ;
                            if (autoreloadlogs[i].coverageFrom != null)
                            {
                                dateFromVar = autoreloadlogs[i].coverageFrom ?? DateTime.Now;
                            }
                            if (autoreloadlogs[i].coverageTo != null)
                            {
                                dateToVar = autoreloadlogs[i].coverageTo ?? DateTime.Now;
                            }

                            var result = await GetLazadaOrderListReRun(dateFromVar, dateFromVar, dateToVar);


                            dynamic resultValue = result.Value;

                            string setResult = resultValue.set;
                            int ordersCount = resultValue.ordersCount;
                            string coverage = resultValue.coverage;



                            autoReloadLogs2 = _userInfoConn.AutoReloadLogs.FirstOrDefault(e => e.id == autoreloadlogs[i].id);

                            if (autoReloadLogs2 != null)
                            {
                                autoReloadLogs2.logs = setResult == "Success" ? "Successful Run" : setResult;
                                autoReloadLogs2.status = setResult == "Success" ? "Success" : "Failed";
                                autoReloadLogs2.totalOrder = ordersCount;
                                autoReloadLogs2.completion = DateTime.Now;
                                autoReloadLogs2.coverage = coverage;
                                autoReloadLogs2.GetOrdersDone = "Done";
                                autoReloadLogs2.coverageFrom = autoreloadlogs[i].coverageFrom;
                                autoReloadLogs2.coverageTo = autoreloadlogs[i].coverageTo;
                                autoReloadLogs2.fromFailed = true;

                                _userInfoConn.SaveChanges();
                            }
                        }
                        else if (autoreloadlogs[i].platform == "Shopee")
                        {


                            var dateFromVar = (autoreloadlogs[i].dateProcess ?? DateTime.Now).AddHours(-8);
                            var dateToVar = (autoreloadlogs[i].dateProcess ?? DateTime.Now).AddHours(-4); ;
                            if (autoreloadlogs[i].coverageFrom != null)
                            {
                                dateFromVar = autoreloadlogs[i].coverageFrom ?? DateTime.Now;
                            }
                            if (autoreloadlogs[i].coverageTo != null)
                            {
                                dateToVar = autoreloadlogs[i].coverageTo ?? DateTime.Now;
                            }

                            if (autoreloadlogs[i].dateProcess < DateTime.Now.AddMinutes(-20))
                            {
                                var result = await GetShopeeOrderListReRun(dateFromVar, dateToVar, access_token.ToString());


                                dynamic resultValue = result.Value;

                                string setResult = resultValue.set;
                                int ordersCount = resultValue.ordersCount;
                                string coverage = resultValue.coverage;
                                DateTime? coverageFrom = resultValue.coverageFrom;
                                DateTime? coverageTo = resultValue.coverageTo;


                                autoReloadLogs2 = _userInfoConn.AutoReloadLogs.FirstOrDefault(e => e.id == autoreloadlogs[i].id);

                                if (autoReloadLogs2 != null)
                                {
                                    autoReloadLogs2.logs = setResult == "Success" ? "Successful Run" : setResult;
                                    autoReloadLogs2.status = setResult == "Success" ? "Success" : "Failed";
                                    autoReloadLogs2.totalOrder = ordersCount;
                                    autoReloadLogs2.completion = DateTime.Now;
                                    autoReloadLogs2.coverage = coverage;
                                    autoReloadLogs2.GetOrdersDone = "Done";
                                    autoReloadLogs2.coverageFrom = autoreloadlogs[i].coverageFrom;
                                    autoReloadLogs2.coverageTo = autoreloadlogs[i].coverageTo;
                                    autoReloadLogs2.fromFailed = true;

                                    _userInfoConn.SaveChanges();
                                }
                            }
                        }

                    }
                }
                //await GetShopeeOrderList(dateFrom, dateTo, token.AccessToken);






                return Json(new { set = "Success" });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public async Task<JsonResult> GetOrdersLazada2([FromQuery] DateTime dateFrom, DateTime dateTo, CancellationToken stoppingToken)
        {

            //Infrastructure.External.ShopeeWebApi.AuthenticationToken token = await _authenthicationTokenProvider.GetAuthenticationToken(stoppingToken);
            //await GetShopeeOrderList(dateFrom, dateTo, token.AccessToken);
            await GetLazadaOrderList2(dateFrom, dateFrom, dateTo);
            await GetLazadaOrderList3(dateFrom, dateFrom, dateTo);
            await GetLazadaOrderList4(dateFrom, dateFrom, dateTo);


            //var boxOrdersShopee = _userInfoConn.boxOrders.Where(e => e.boxerStatus == "Done" && e.platformStatus == null && e.module == "shopee").Select(a => a.orderId).Distinct();

            //for (var i = 0; i < boxOrdersShopee.Count(); i++)
            //{

            //    await ShopeePickupStatus(token.AccessToken, boxOrdersShopee.ToArray()[i]);
            //}


            var boxOrdersLazada = _userInfoConn.boxOrders.Where(e => e.boxerStatus == "Done" && e.platformStatus == null && e.module == "lazada").Select(a => a.orderId).Distinct();
            for (var i = 0; i < boxOrdersLazada.Count(); i++)
            {

                await LazadaPickupStatus(boxOrdersLazada.ToArray()[i]);
            }

            //var boxOrdersShopeeTracking = _userInfoConn.boxOrders.Where(e => e.boxerStatus == "Done" && (e.trackingNo == "" || e.trackingNo == null) && e.module == "shopee").Select(a => a.orderId).Distinct();
            //for (var i = 0; i < boxOrdersShopeeTracking.Count(); i++)
            //{

            //    await ShopeeTrackingNo(token.AccessToken, boxOrdersShopeeTracking.ToArray()[i]);
            //}




            return Json(new { set = "Success" });
        }
        public async Task<JsonResult> GetShopeeOrderList([FromQuery] DateTime dateFrom, DateTime dateTo, string access_token)
        {
        //    dateFrom = DateTime.Now;
        //    dateFrom = dateFrom.AddMinutes(-41).AddSeconds(-dateFrom.Second).AddMilliseconds(-dateFrom.Millisecond);

        //    dateTo = DateTime.Now;
        //    dateTo = dateTo.AddMinutes(-30).AddSeconds(-dateTo.Second).AddMilliseconds(-dateTo.Millisecond);

            dateFrom = DateTime.Now;
            dateFrom = dateFrom.AddMinutes(-21).AddSeconds(-dateFrom.Second).AddMilliseconds(-dateFrom.Millisecond);

            dateTo = DateTime.Now;
            dateTo = dateTo.AddMinutes(-10).AddSeconds(-dateTo.Second).AddMilliseconds(-dateTo.Millisecond);



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


            var ordersCount = 0;

            var coverage = dateFrom.ToString() + " - " + dateTo.ToString();

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
                                string requestUrl = $"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&order_status=READY_TO_SHIP&sign={sign}&time_range_field=update_time&time_from={dateFrom_timestamp}&time_to={dateTo_timestamp}&page_size=100";

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


                                return Json(new { set = ex, ordersCount = ordersCount, coverage = coverage, coverageFrom = dateFrom, coverageTo = dateTo });
                                // Handle the error as needed
                                break; // Exit the loop on error
                            }
                        }
                    }
                    stopwatch.Stop();



                    ordersCount = mergedOrderList.Count;


                    if (mergedOrderList.Count > 0)
                    {
                        //JArray array = (JArray)(responseJson["response"]?["order_list"] ?? "");

                        // Deserialize the JSON into a List of objects
                        //List<Dictionary<string, string>> list = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(array.ToString());

                        //// Create a Dictionary with order_sn as the key and package_number as the value
                        //Dictionary<string, string> result = list.ToDictionary(item => item["order_sn"], item => item["package_number"]);

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

                                                    string shopee_sql_ecom = "SELECT SUM([OnHand]) FROM [EcommerceHub].[dbo].[Inventories] Where Sku= " + shopee_sku_id + " AND OnHand IS NOT NULL GROUP BY SKU";
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
                                    catch (FormatException ex)
                                    {
                                        var csds = _configuration.GetConnectionString("Myconnection");
                                        using var connsds = new SqlConnection(csds);
                                        connsds.Open();

                                        string sqld = $"EXEC DeleteNoHeader @module = 'shopee'";
                                        using var cmdd = new SqlCommand(sqld, connsds);
                                        cmdd.ExecuteNonQuery();
                                        connsds.Close();


                                        return Json(new { set = ex, ordersCount = ordersCount, coverage = coverage, coverageFrom = dateFrom, coverageTo = dateTo });

                                        //throw new ShopeeApiException($"Invalid object ({jtoken.ToString(Formatting.None)}) detected at Shopee.")
                                        //{
                                        //    RequestUrl = urlInfo,
                                        //    RequestMethod = "POST",
                                        //    ResponseDescription = responseContentStringInfo,
                                        //    Timestamp = now
                                        //};
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
                        return Json(new { set = httpResponse.IsSuccessStatusCode, ordersCount = ordersCount, coverage = coverage });


                        //throw new ShopeeApiException(message: "An error occured while accessing shopee API")
                        //{
                        //    RequestUrl = url,
                        //    RequestMethod = "POST",
                        //    ResponseDescription = responseContentString,
                        //    Timestamp = now,
                        //    HttpStatusCode = httpResponse.StatusCode
                        //};
                    }
                }
                catch (TaskCanceledException ex)
                {
                    var csds = _configuration.GetConnectionString("Myconnection");
                    using var connsds = new SqlConnection(csds);
                    connsds.Open();

                    string sqld = $"EXEC DeleteNoHeader @module = 'shopee'";
                    using var cmdd = new SqlCommand(sqld, connsds);
                    cmdd.ExecuteNonQuery();
                    connsds.Close();



                    return Json(new { set = ex, ordersCount = ordersCount, coverage = coverage, coverageFrom = dateFrom, coverageTo = dateTo });
                    //throw;
                }
                catch (ShopeeApiException ex)
                {
                    var csds = _configuration.GetConnectionString("Myconnection");
                    using var connsds = new SqlConnection(csds);
                    connsds.Open();

                    string sqld = $"EXEC DeleteNoHeader @module = 'shopee'";
                    using var cmdd = new SqlCommand(sqld, connsds);
                    cmdd.ExecuteNonQuery();
                    connsds.Close();


                    return Json(new { set = ex, ordersCount = ordersCount, coverage = coverage, coverageFrom = dateFrom, coverageTo = dateTo });
                    //throw;
                }
                catch (Exception ex)
                {
                    var csds = _configuration.GetConnectionString("Myconnection");
                    using var connsds = new SqlConnection(csds);
                    connsds.Open();

                    string sqld = $"EXEC DeleteNoHeader @module = 'shopee'";
                    using var cmdd = new SqlCommand(sqld, connsds);
                    cmdd.ExecuteNonQuery();
                    connsds.Close();


                    return Json(new { set = ex, ordersCount = ordersCount, coverage = coverage, coverageFrom = dateFrom, coverageTo = dateTo });

                    //throw new ShopeeApiException("An error occured while accessing shopee API", ex)
                    //{
                    //    RequestUrl = url,
                    //    RequestMethod = "POST",
                    //    Timestamp = now
                    //};
                }
                _userInfoConn.SaveChanges();
                //_userInfoConn.Dispose();
                shopee_conns.Close();
                shopee_connsd.Close();
                shopee_conns_ecom.Close();
                //shopee_connectionPos.Close();
            }









            /* LAZADA */








            return Json(new { set = "Success", ordersCount = ordersCount, coverage = coverage, coverageFrom = dateFrom, coverageTo = dateTo });


        }
        public async Task<JsonResult> GetShopeeOrderListCatch([FromQuery] DateTime dateFrom, DateTime dateTo, string access_token)
        {
            dateFrom = DateTime.Now;
            dateFrom = dateFrom.AddDays(-2);

            dateTo = DateTime.Now;
            dateTo = dateTo.AddHours(-6);



            long dateFrom_timestamp = dateFrom.ToTimestamp();

            long dateTo_timestamp = dateTo.ToTimestamp();

            //await GetCanceledOrderList(access_token);

            //var csd = _configuration.GetConnectionString("Myconnection");
            //using var connsd = new SqlConnection(csd);
            //connsd.Open();

            //string sqlddd = $"EXEC DeleteNoHeader @module = 'shopee'";
            //using var cmdddd = new SqlCommand(sqlddd, connsd);
            //cmdddd.ExecuteNonQuery();
            //connsd.Close();


            var ordersCount = 0;

            var coverage = dateFrom.ToString() + " - " + dateTo.ToString();

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
                                string requestUrl = $"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&order_status=READY_TO_SHIP&sign={sign}&time_range_field=update_time&time_from={dateFrom_timestamp}&time_to={dateTo_timestamp}&page_size=100";

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


                                return Json(new { set = ex, ordersCount = ordersCount, coverage = coverage, coverageFrom = dateFrom, coverageTo = dateTo });
                                // Handle the error as needed
                                break; // Exit the loop on error
                            }
                        }
                    }
                    stopwatch.Stop();



                    ordersCount = mergedOrderList.Count;


                    if (mergedOrderList.Count > 0)
                    {
                        //JArray array = (JArray)(responseJson["response"]?["order_list"] ?? "");

                        // Deserialize the JSON into a List of objects
                        //List<Dictionary<string, string>> list = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(array.ToString());

                        //// Create a Dictionary with order_sn as the key and package_number as the value
                        //Dictionary<string, string> result = list.ToDictionary(item => item["order_sn"], item => item["package_number"]);

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

                                                    string shopee_sql_ecom = "SELECT SUM([OnHand]) FROM [EcommerceHub].[dbo].[Inventories] Where Sku= " + shopee_sku_id + " AND OnHand IS NOT NULL GROUP BY SKU";
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
                                    catch (FormatException ex)
                                    {
                                        var csds = _configuration.GetConnectionString("Myconnection");
                                        using var connsds = new SqlConnection(csds);
                                        connsds.Open();

                                        string sqld = $"EXEC DeleteNoHeader @module = 'shopee'";
                                        using var cmdd = new SqlCommand(sqld, connsds);
                                        cmdd.ExecuteNonQuery();
                                        connsds.Close();


                                        return Json(new { set = ex, ordersCount = ordersCount, coverage = coverage, coverageFrom = dateFrom, coverageTo = dateTo });

                                        //throw new ShopeeApiException($"Invalid object ({jtoken.ToString(Formatting.None)}) detected at Shopee.")
                                        //{
                                        //    RequestUrl = urlInfo,
                                        //    RequestMethod = "POST",
                                        //    ResponseDescription = responseContentStringInfo,
                                        //    Timestamp = now
                                        //};
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
                        return Json(new { set = httpResponse.IsSuccessStatusCode, ordersCount = ordersCount, coverage = coverage });


                        //throw new ShopeeApiException(message: "An error occured while accessing shopee API")
                        //{
                        //    RequestUrl = url,
                        //    RequestMethod = "POST",
                        //    ResponseDescription = responseContentString,
                        //    Timestamp = now,
                        //    HttpStatusCode = httpResponse.StatusCode
                        //};
                    }
                }
                catch (TaskCanceledException ex)
                {
                    var csds = _configuration.GetConnectionString("Myconnection");
                    using var connsds = new SqlConnection(csds);
                    connsds.Open();

                    string sqld = $"EXEC DeleteNoHeader @module = 'shopee'";
                    using var cmdd = new SqlCommand(sqld, connsds);
                    cmdd.ExecuteNonQuery();
                    connsds.Close();



                    return Json(new { set = ex, ordersCount = ordersCount, coverage = coverage, coverageFrom = dateFrom, coverageTo = dateTo });
                    //throw;
                }
                catch (ShopeeApiException ex)
                {
                    var csds = _configuration.GetConnectionString("Myconnection");
                    using var connsds = new SqlConnection(csds);
                    connsds.Open();

                    string sqld = $"EXEC DeleteNoHeader @module = 'shopee'";
                    using var cmdd = new SqlCommand(sqld, connsds);
                    cmdd.ExecuteNonQuery();
                    connsds.Close();


                    return Json(new { set = ex, ordersCount = ordersCount, coverage = coverage, coverageFrom = dateFrom, coverageTo = dateTo });
                    //throw;
                }
                catch (Exception ex)
                {
                    var csds = _configuration.GetConnectionString("Myconnection");
                    using var connsds = new SqlConnection(csds);
                    connsds.Open();

                    string sqld = $"EXEC DeleteNoHeader @module = 'shopee'";
                    using var cmdd = new SqlCommand(sqld, connsds);
                    cmdd.ExecuteNonQuery();
                    connsds.Close();


                    return Json(new { set = ex, ordersCount = ordersCount, coverage = coverage, coverageFrom = dateFrom, coverageTo = dateTo });

                    //throw new ShopeeApiException("An error occured while accessing shopee API", ex)
                    //{
                    //    RequestUrl = url,
                    //    RequestMethod = "POST",
                    //    Timestamp = now
                    //};
                }
                _userInfoConn.SaveChanges();
                //_userInfoConn.Dispose();
                shopee_conns.Close();
                shopee_connsd.Close();
                shopee_conns_ecom.Close();
                //shopee_connectionPos.Close();
            }









            /* LAZADA */








            return Json(new { set = "Success", ordersCount = ordersCount, coverage = coverage, coverageFrom = dateFrom, coverageTo = dateTo });


        }


        public async Task<JsonResult> GetShopeeOrderListReRun([FromQuery] DateTime dateFrom, DateTime dateTo, string access_token)
        {
            //dateFrom = DateTime.Now;
            //dateFrom = dateFrom.AddMinutes(-20);

            //dateTo = DateTime.Now;
            //dateTo = dateTo.AddMinutes(-10);



            long dateFrom_timestamp = dateFrom.ToTimestamp();

            long dateTo_timestamp = dateTo.ToTimestamp();

            //await GetCanceledOrderList(access_token);

            //var csd = _configuration.GetConnectionString("Myconnection");
            //using var connsd = new SqlConnection(csd);
            //connsd.Open();

            //string sqlddd = $"EXEC DeleteNoHeader @module = 'shopee'";
            //using var cmdddd = new SqlCommand(sqlddd, connsd);
            //cmdddd.ExecuteNonQuery();
            //connsd.Close();


            var ordersCount = 0;

            var coverage = dateFrom.ToString() + " - " + dateTo.ToString();

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
                                string requestUrl = $"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&order_status=READY_TO_SHIP&sign={sign}&time_range_field=update_time&time_from={dateFrom_timestamp}&time_to={dateTo_timestamp}&page_size=100";

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


                                return Json(new { set = ex, ordersCount = ordersCount, coverage = coverage, coverageFrom = dateFrom, coverageTo = dateTo });
                                // Handle the error as needed
                                break; // Exit the loop on error
                            }
                        }
                    }
                    stopwatch.Stop();



                    ordersCount = mergedOrderList.Count;


                    if (mergedOrderList.Count > 0)
                    {
                        //JArray array = (JArray)(responseJson["response"]?["order_list"] ?? "");

                        // Deserialize the JSON into a List of objects
                        //List<Dictionary<string, string>> list = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(array.ToString());

                        //// Create a Dictionary with order_sn as the key and package_number as the value
                        //Dictionary<string, string> result = list.ToDictionary(item => item["order_sn"], item => item["package_number"]);

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

                                                    string shopee_sql_ecom = "SELECT SUM([OnHand]) FROM [EcommerceHub].[dbo].[Inventories] Where Sku= " + shopee_sku_id + " AND OnHand IS NOT NULL GROUP BY SKU";
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
                                    catch (FormatException ex)
                                    {
                                        var csds = _configuration.GetConnectionString("Myconnection");
                                        using var connsds = new SqlConnection(csds);
                                        connsds.Open();

                                        string sqld = $"EXEC DeleteNoHeader @module = 'shopee'";
                                        using var cmdd = new SqlCommand(sqld, connsds);
                                        cmdd.ExecuteNonQuery();
                                        connsds.Close();


                                        return Json(new { set = ex, ordersCount = ordersCount, coverage = coverage, coverageFrom = dateFrom, coverageTo = dateTo });

                                        //throw new ShopeeApiException($"Invalid object ({jtoken.ToString(Formatting.None)}) detected at Shopee.")
                                        //{
                                        //    RequestUrl = urlInfo,
                                        //    RequestMethod = "POST",
                                        //    ResponseDescription = responseContentStringInfo,
                                        //    Timestamp = now
                                        //};
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
                        return Json(new { set = httpResponse.IsSuccessStatusCode, ordersCount = ordersCount, coverage = coverage });


                        //throw new ShopeeApiException(message: "An error occured while accessing shopee API")
                        //{
                        //    RequestUrl = url,
                        //    RequestMethod = "POST",
                        //    ResponseDescription = responseContentString,
                        //    Timestamp = now,
                        //    HttpStatusCode = httpResponse.StatusCode
                        //};
                    }
                }
                catch (TaskCanceledException ex)
                {
                    var csds = _configuration.GetConnectionString("Myconnection");
                    using var connsds = new SqlConnection(csds);
                    connsds.Open();

                    string sqld = $"EXEC DeleteNoHeader @module = 'shopee'";
                    using var cmdd = new SqlCommand(sqld, connsds);
                    cmdd.ExecuteNonQuery();
                    connsds.Close();



                    return Json(new { set = ex, ordersCount = ordersCount, coverage = coverage, coverageFrom = dateFrom, coverageTo = dateTo });
                    //throw;
                }
                catch (ShopeeApiException ex)
                {
                    var csds = _configuration.GetConnectionString("Myconnection");
                    using var connsds = new SqlConnection(csds);
                    connsds.Open();

                    string sqld = $"EXEC DeleteNoHeader @module = 'shopee'";
                    using var cmdd = new SqlCommand(sqld, connsds);
                    cmdd.ExecuteNonQuery();
                    connsds.Close();


                    return Json(new { set = ex, ordersCount = ordersCount, coverage = coverage, coverageFrom = dateFrom, coverageTo = dateTo });
                    //throw;
                }
                catch (Exception ex)
                {
                    var csds = _configuration.GetConnectionString("Myconnection");
                    using var connsds = new SqlConnection(csds);
                    connsds.Open();

                    string sqld = $"EXEC DeleteNoHeader @module = 'shopee'";
                    using var cmdd = new SqlCommand(sqld, connsds);
                    cmdd.ExecuteNonQuery();
                    connsds.Close();


                    return Json(new { set = ex, ordersCount = ordersCount, coverage = coverage, coverageFrom = dateFrom, coverageTo = dateTo });

                    //throw new ShopeeApiException("An error occured while accessing shopee API", ex)
                    //{
                    //    RequestUrl = url,
                    //    RequestMethod = "POST",
                    //    Timestamp = now
                    //};
                }
                _userInfoConn.SaveChanges();
                //_userInfoConn.Dispose();
                shopee_conns.Close();
                shopee_connsd.Close();
                shopee_conns_ecom.Close();
                //shopee_connectionPos.Close();
            }









            /* LAZADA */








            return Json(new { set = "Success", ordersCount = ordersCount, coverage = coverage, coverageFrom = dateFrom, coverageTo = dateTo });


        }


        public async Task<IActionResult> GetShopeeOrderList2([FromQuery] DateTime dateFrom, DateTime dateTo, string access_token)
        {
            dateFrom = DateTime.Now;
            dateFrom = dateFrom.AddHours(-4);

            dateTo = DateTime.Now;
            dateTo = dateTo.AddHours(-3);



            long dateFrom_timestamp = dateFrom.ToTimestamp();

            long dateTo_timestamp = dateTo.ToTimestamp();

            //await GetCanceledOrderList(access_token);

            //var csd = _configuration.GetConnectionString("Myconnection");
            //using var connsd = new SqlConnection(csd);
            //connsd.Open();

            //string sqlddd = $"EXEC DeleteNoHeader";
            //using var cmdddd = new SqlCommand(sqlddd, connsd);
            //cmdddd.ExecuteNonQuery();
            //connsd.Close();




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

                Stopwatch stopwatch = Stopwatch.StartNew();

                try
                {
                    //_logger.LogTrace($"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&sign={sign}&shop_id={shopId}&page_size=100");
                    //using HttpResponseMessage httpResponse = await _policy.ExecuteAsync(async (ct) => await client.GetAsync(
                    //     requestUri: $"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&sign={sign}&shop_id={shopId}&page_size=100",
                    //    cancellationToken: ct),
                    //    cancellationToken: cancellationToken);

                    _logger.LogTrace($"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&order_status=READY_TO_SHIP&sign={sign}&time_range_field=create_time&time_from={dateFrom_timestamp}&time_to={dateTo_timestamp}&page_size=100");
                    using HttpResponseMessage httpResponse = await _policy.ExecuteAsync(async (ct) => await client.GetAsync(
                        requestUri: $"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&order_status=READY_TO_SHIP&sign={sign}&time_range_field=create_time&time_from={dateFrom_timestamp}&time_to={dateTo_timestamp}&page_size=100",
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

                    //var dictionary = responseJson.ToDictionary( x => x.OrderById, y => y.PackageNo);

                    //dictionary.TryGetValue()

                    if (responseJson.ContainsKey("message"))

                        if (responseJson.ContainsKey("response"))
                        {
                            JArray array = (JArray)(responseJson["response"]?["order_list"] ?? "");

                            // Deserialize the JSON into a List of objects
                            //List<Dictionary<string, string>> list = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(array.ToString());

                            //// Create a Dictionary with order_sn as the key and package_number as the value
                            //Dictionary<string, string> result = list.ToDictionary(item => item["order_sn"], item => item["package_number"]);

                            for (var x = 0; x < array.Count; x++)
                            {
                                string sqqldd = "SELECT COUNT(*) FROM orderTableHeader WHERE orderId='" + array[x]["order_sn"].ToString() + "'";
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

                                    _logger.LogTrace($"{urlInfo}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&sign={signInfo}&order_sn_list={array[x]["order_sn"].ToString()}&response_optional_fields={string.Join(separator: ",", paramShopee)}");
                                    string req = $"{urlInfo}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&sign={signInfo}&order_sn_list={array[x]["order_sn"].ToString()}&response_optional_fields={string.Join(separator: ",", paramShopee)}";
                                    Stopwatch stopwatchInfo = Stopwatch.StartNew();
                                    using HttpResponseMessage httpResponseInfo = await _policy.ExecuteAsync(async (ct) => await clientInfo.GetAsync(
                                    requestUri: $"{urlInfo}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&sign={signInfo}&order_sn_list={array[x]["order_sn"].ToString()}&response_optional_fields={string.Join(separator: ",", paramShopee)}",
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

                                                        string shopee_sql_ecom = "SELECT SUM([OnHand]) FROM [EcommerceHub].[dbo].[Inventories] Where Sku= " + shopee_sku_id + " AND OnHand IS NOT NULL GROUP BY SKU";
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

                                            string sqld = $"EXEC DeleteNoHeader @module = 'shopee'";
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
                catch (TaskCanceledException)
                {
                    //var csds = _configuration.GetConnectionString("Myconnection");
                    //using var connsds = new SqlConnection(csds);
                    //connsds.Open();

                    //string sqld = $"EXEC DeleteNoHeader";
                    //using var cmdd = new SqlCommand(sqld, connsds);
                    //cmdd.ExecuteNonQuery();
                    //connsds.Close();

                    throw;
                }
                catch (ShopeeApiException)
                {
                    //var csds = _configuration.GetConnectionString("Myconnection");
                    //using var connsds = new SqlConnection(csds);
                    //connsds.Open();

                    //string sqld = $"EXEC DeleteNoHeader";
                    //using var cmdd = new SqlCommand(sqld, connsds);
                    //cmdd.ExecuteNonQuery();
                    //connsds.Close();

                    throw;
                }
                catch (Exception ex)
                {
                    //var csds = _configuration.GetConnectionString("Myconnection");
                    //using var connsds = new SqlConnection(csds);
                    //connsds.Open();

                    //string sqld = $"EXEC DeleteNoHeader";
                    //using var cmdd = new SqlCommand(sqld, connsds);
                    //cmdd.ExecuteNonQuery();
                    //connsds.Close();

                    throw new ShopeeApiException("An error occured while accessing shopee API", ex)
                    {
                        RequestUrl = url,
                        RequestMethod = "POST",
                        Timestamp = now
                    };
                }
                _userInfoConn.SaveChanges();
                //_userInfoConn.Dispose();
                shopee_conns.Close();
                shopee_connsd.Close();
                shopee_conns_ecom.Close();
                //shopee_connectionPos.Close();
            }









            /* LAZADA */








            return Json(new { set = "Success" });


        }

        public async Task<IActionResult> GetShopeeOrderList3([FromQuery] DateTime dateFrom, DateTime dateTo, string access_token)
        {
            dateFrom = DateTime.Now;
            dateFrom = dateFrom.AddHours(-3);

            dateTo = DateTime.Now;
            dateTo = dateTo.AddHours(-2);



            long dateFrom_timestamp = dateFrom.ToTimestamp();

            long dateTo_timestamp = dateTo.ToTimestamp();

            //await GetCanceledOrderList(access_token);

            //var csd = _configuration.GetConnectionString("Myconnection");
            //using var connsd = new SqlConnection(csd);
            //connsd.Open();

            //string sqlddd = $"EXEC DeleteNoHeader";
            //using var cmdddd = new SqlCommand(sqlddd, connsd);
            //cmdddd.ExecuteNonQuery();
            //connsd.Close();




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

                Stopwatch stopwatch = Stopwatch.StartNew();

                try
                {
                    //_logger.LogTrace($"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&sign={sign}&shop_id={shopId}&page_size=100");
                    //using HttpResponseMessage httpResponse = await _policy.ExecuteAsync(async (ct) => await client.GetAsync(
                    //     requestUri: $"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&sign={sign}&shop_id={shopId}&page_size=100",
                    //    cancellationToken: ct),
                    //    cancellationToken: cancellationToken);

                    _logger.LogTrace($"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&order_status=READY_TO_SHIP&sign={sign}&time_range_field=create_time&time_from={dateFrom_timestamp}&time_to={dateTo_timestamp}&page_size=100");
                    using HttpResponseMessage httpResponse = await _policy.ExecuteAsync(async (ct) => await client.GetAsync(
                        requestUri: $"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&order_status=READY_TO_SHIP&sign={sign}&time_range_field=create_time&time_from={dateFrom_timestamp}&time_to={dateTo_timestamp}&page_size=100",
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

                    //var dictionary = responseJson.ToDictionary( x => x.OrderById, y => y.PackageNo);

                    //dictionary.TryGetValue()

                    if (responseJson.ContainsKey("message"))

                        if (responseJson.ContainsKey("response"))
                        {
                            JArray array = (JArray)(responseJson["response"]?["order_list"] ?? "");

                            // Deserialize the JSON into a List of objects
                            //List<Dictionary<string, string>> list = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(array.ToString());

                            //// Create a Dictionary with order_sn as the key and package_number as the value
                            //Dictionary<string, string> result = list.ToDictionary(item => item["order_sn"], item => item["package_number"]);

                            for (var x = 0; x < array.Count; x++)
                            {
                                string sqqldd = "SELECT COUNT(*) FROM orderTableHeader WHERE orderId='" + array[x]["order_sn"].ToString() + "'";
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

                                    _logger.LogTrace($"{urlInfo}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&sign={signInfo}&order_sn_list={array[x]["order_sn"].ToString()}&response_optional_fields={string.Join(separator: ",", paramShopee)}");
                                    string req = $"{urlInfo}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&sign={signInfo}&order_sn_list={array[x]["order_sn"].ToString()}&response_optional_fields={string.Join(separator: ",", paramShopee)}";
                                    Stopwatch stopwatchInfo = Stopwatch.StartNew();
                                    using HttpResponseMessage httpResponseInfo = await _policy.ExecuteAsync(async (ct) => await clientInfo.GetAsync(
                                    requestUri: $"{urlInfo}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&sign={signInfo}&order_sn_list={array[x]["order_sn"].ToString()}&response_optional_fields={string.Join(separator: ",", paramShopee)}",
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

                                                        string shopee_sql_ecom = "SELECT SUM([OnHand]) FROM [EcommerceHub].[dbo].[Inventories] Where Sku= " + shopee_sku_id + " AND OnHand IS NOT NULL GROUP BY SKU";
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

                                            string sqld = $"EXEC DeleteNoHeader @module = 'shopee'";
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
                catch (TaskCanceledException)
                {
                    //var csds = _configuration.GetConnectionString("Myconnection");
                    //using var connsds = new SqlConnection(csds);
                    //connsds.Open();

                    //string sqld = $"EXEC DeleteNoHeader";
                    //using var cmdd = new SqlCommand(sqld, connsds);
                    //cmdd.ExecuteNonQuery();
                    //connsds.Close();

                    throw;
                }
                catch (ShopeeApiException)
                {
                    //var csds = _configuration.GetConnectionString("Myconnection");
                    //using var connsds = new SqlConnection(csds);
                    //connsds.Open();

                    //string sqld = $"EXEC DeleteNoHeader";
                    //using var cmdd = new SqlCommand(sqld, connsds);
                    //cmdd.ExecuteNonQuery();
                    //connsds.Close();

                    throw;
                }
                catch (Exception ex)
                {
                    //var csds = _configuration.GetConnectionString("Myconnection");
                    //using var connsds = new SqlConnection(csds);
                    //connsds.Open();

                    //string sqld = $"EXEC DeleteNoHeader";
                    //using var cmdd = new SqlCommand(sqld, connsds);
                    //cmdd.ExecuteNonQuery();
                    //connsds.Close();

                    throw new ShopeeApiException("An error occured while accessing shopee API", ex)
                    {
                        RequestUrl = url,
                        RequestMethod = "POST",
                        Timestamp = now
                    };
                }
                _userInfoConn.SaveChanges();
                //_userInfoConn.Dispose();
                shopee_conns.Close();
                shopee_connsd.Close();
                shopee_conns_ecom.Close();
                //shopee_connectionPos.Close();
            }









            /* LAZADA */








            return Json(new { set = "Success" });


        }


        public async Task<IActionResult> GetShopeeOrderList4([FromQuery] DateTime dateFrom, DateTime dateTo, string access_token)
        {
            dateFrom = DateTime.Now;
            dateFrom = dateFrom.AddHours(-2);

            dateTo = DateTime.Now;
            dateTo = dateTo.AddHours(-1);



            long dateFrom_timestamp = dateFrom.ToTimestamp();

            long dateTo_timestamp = dateTo.ToTimestamp();

            //await GetCanceledOrderList(access_token);

            //var csd = _configuration.GetConnectionString("Myconnection");
            //using var connsd = new SqlConnection(csd);
            //connsd.Open();

            //string sqlddd = $"EXEC DeleteNoHeader";
            //using var cmdddd = new SqlCommand(sqlddd, connsd);
            //cmdddd.ExecuteNonQuery();
            //connsd.Close();




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

                Stopwatch stopwatch = Stopwatch.StartNew();

                try
                {
                    //_logger.LogTrace($"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&sign={sign}&shop_id={shopId}&page_size=100");
                    //using HttpResponseMessage httpResponse = await _policy.ExecuteAsync(async (ct) => await client.GetAsync(
                    //     requestUri: $"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&sign={sign}&shop_id={shopId}&page_size=100",
                    //    cancellationToken: ct),
                    //    cancellationToken: cancellationToken);

                    _logger.LogTrace($"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&order_status=READY_TO_SHIP&sign={sign}&time_range_field=create_time&time_from={dateFrom_timestamp}&time_to={dateTo_timestamp}&page_size=100");
                    using HttpResponseMessage httpResponse = await _policy.ExecuteAsync(async (ct) => await client.GetAsync(
                        requestUri: $"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&order_status=READY_TO_SHIP&sign={sign}&time_range_field=create_time&time_from={dateFrom_timestamp}&time_to={dateTo_timestamp}&page_size=100",
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

                    //var dictionary = responseJson.ToDictionary( x => x.OrderById, y => y.PackageNo);

                    //dictionary.TryGetValue()

                    if (responseJson.ContainsKey("message"))

                        if (responseJson.ContainsKey("response"))
                        {
                            JArray array = (JArray)(responseJson["response"]?["order_list"] ?? "");

                            // Deserialize the JSON into a List of objects
                            //List<Dictionary<string, string>> list = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(array.ToString());

                            //// Create a Dictionary with order_sn as the key and package_number as the value
                            //Dictionary<string, string> result = list.ToDictionary(item => item["order_sn"], item => item["package_number"]);

                            for (var x = 0; x < array.Count; x++)
                            {
                                string sqqldd = "SELECT COUNT(*) FROM orderTableHeader WHERE orderId='" + array[x]["order_sn"].ToString() + "'";
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

                                    _logger.LogTrace($"{urlInfo}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&sign={signInfo}&order_sn_list={array[x]["order_sn"].ToString()}&response_optional_fields={string.Join(separator: ",", paramShopee)}");
                                    string req = $"{urlInfo}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&sign={signInfo}&order_sn_list={array[x]["order_sn"].ToString()}&response_optional_fields={string.Join(separator: ",", paramShopee)}";
                                    Stopwatch stopwatchInfo = Stopwatch.StartNew();
                                    using HttpResponseMessage httpResponseInfo = await _policy.ExecuteAsync(async (ct) => await clientInfo.GetAsync(
                                    requestUri: $"{urlInfo}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&sign={signInfo}&order_sn_list={array[x]["order_sn"].ToString()}&response_optional_fields={string.Join(separator: ",", paramShopee)}",
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

                                                        string shopee_sql_ecom = "SELECT SUM([OnHand]) FROM [EcommerceHub].[dbo].[Inventories] Where Sku= " + shopee_sku_id + " AND OnHand IS NOT NULL GROUP BY SKU";
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

                                            string sqld = $"EXEC DeleteNoHeader @module = 'shopee'";
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
                catch (TaskCanceledException)
                {
                    //var csds = _configuration.GetConnectionString("Myconnection");
                    //using var connsds = new SqlConnection(csds);
                    //connsds.Open();

                    //string sqld = $"EXEC DeleteNoHeader";
                    //using var cmdd = new SqlCommand(sqld, connsds);
                    //cmdd.ExecuteNonQuery();
                    //connsds.Close();

                    throw;
                }
                catch (ShopeeApiException)
                {
                    //var csds = _configuration.GetConnectionString("Myconnection");
                    //using var connsds = new SqlConnection(csds);
                    //connsds.Open();

                    //string sqld = $"EXEC DeleteNoHeader";
                    //using var cmdd = new SqlCommand(sqld, connsds);
                    //cmdd.ExecuteNonQuery();
                    //connsds.Close();

                    throw;
                }
                catch (Exception ex)
                {
                    //var csds = _configuration.GetConnectionString("Myconnection");
                    //using var connsds = new SqlConnection(csds);
                    //connsds.Open();

                    //string sqld = $"EXEC DeleteNoHeader";
                    //using var cmdd = new SqlCommand(sqld, connsds);
                    //cmdd.ExecuteNonQuery();
                    //connsds.Close();

                    throw new ShopeeApiException("An error occured while accessing shopee API", ex)
                    {
                        RequestUrl = url,
                        RequestMethod = "POST",
                        Timestamp = now
                    };
                }
                _userInfoConn.SaveChanges();
                //_userInfoConn.Dispose();
                shopee_conns.Close();
                shopee_connsd.Close();
                shopee_conns_ecom.Close();
                //shopee_connectionPos.Close();
            }









            /* LAZADA */








            return Json(new { set = "Success" });


        }

        public async Task<JsonResult> GetLazadaOrderList([FromQuery] DateTime dateFrom, DateTime dateFromCancelled, DateTime dateTo)
        {
            var ordersCount = 0;
            var coverage = string.Empty;
            try
            {
                var csd2 = _configuration.GetConnectionString("Myconnection");
                using var connsd2 = new SqlConnection(csd2);
                connsd2.Open();

                string sqld2 = $"EXEC DeleteNoHeader @module = 'lazada'";
                using var cmdd2 = new SqlCommand(sqld2, connsd2);
                cmdd2.ExecuteNonQuery();
                connsd2.Close();

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


                //dateFrom = DateTime.Now;
                //dateFrom = dateFrom.AddMinutes(-41).AddSeconds(-dateFrom.Second).AddMilliseconds(-dateFrom.Millisecond);

                //dateTo = DateTime.Now;
                //dateTo = dateTo.AddMinutes(-30).AddSeconds(-dateTo.Second).AddMilliseconds(-dateTo.Millisecond);


                dateFrom = DateTime.Now;
                dateFrom = dateFrom.AddMinutes(-21).AddSeconds(-dateFrom.Second).AddMilliseconds(-dateFrom.Millisecond);

                dateTo = DateTime.Now;
                dateTo = dateTo.AddMinutes(-10).AddSeconds(-dateTo.Second).AddMilliseconds(-dateTo.Millisecond);


                dateFromCancelled = DateTime.Now.AddDays(-2);

                //dateFrom = dateFrom.AddMinutes(-10);

                coverage = dateFrom.ToString() + " - " + dateTo.ToString();

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
                requestCanceled.AddApiParameter("update_after", dateFromCancelled.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                requestCanceled.AddApiParameter("update_before", dateTo.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                requestCanceled.AddApiParameter("status", "canceled");
                LazopResponse responseCanceled = clientCanceled.Execute(requestCanceled, accessToken);
                objCanceled = responseCanceled.Body;
                JObject responseJsonCanceled = JObject.Parse(objCanceled); 






                for (int x = 0; x < responseJsonCanceled["data"]["orders"].Count(); x++)
                {
                    var orderId = responseJsonCanceled["data"]["orders"][x]["order_number"].ToString(); //get canceled orderid
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
                        var canceledItemsCount = 0;
                        for (int i = 0; i < cnt; i++)
                        {
                            if (responseJson["data"][i]["status"].ToString() == "canceled")
                                canceledItemsCount++;
                        }

                        if (canceledItemsCount == cnt)
                            isItemCancelled = true;


                        if (isItemCancelled)
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

                            if (ordersTableExist.Count > 0 && ordersTableHeaderExist.Count > 0)
                            {
                                for (int i = 0; i < cnt; i++)
                                {
                                    if (responseJson["data"][i]["status"].ToString() == "canceled")
                                    {

                                        var ordersTable = new OrderClass();
                                        ordersTable = _userInfoConn.ordersTable.Where(e => e.order_item_id == responseJson["data"][i]["order_item_id"].ToString()).FirstOrDefault();

                                        if (ordersTable != null)
                                        {
                                            if (ordersTable.typeOfexception == "NIB")
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

                                        string sqld1_add = $"INSERT INTO clearedOrders (deductedStockEcom, deductedStock2017, dateProcess, skuId, orderId, module, processBy, isFreeItem, isNIB, isFromNIB) SELECT 1, 1, GETDATE(), sku_id, orderId, 'lazada', 'System', 0, 0, 0 FROM ordersTable WHERE orderId = '{orderId}' AND platform_status <> 'canceled' ";
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




                //////////////////////////////////////////////////////////////////


                request.SetApiName("/orders/get");
                request.SetHttpMethod("GET");
                request.AddApiParameter("sort_direction", "DESC");
                request.AddApiParameter("offset", "0");
                request.AddApiParameter("limit", "100");
                request.AddApiParameter("sort_by", "updated_at");
                /*request.AddApiParameter("created_after", finalDateFrom);*/
                request.AddApiParameter("update_before", dateFrom.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                request.AddApiParameter("update_after", dateTo.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                request.AddApiParameter("status", "pending");
                LazopResponse responseUpdate = clientLaz.Execute(request, accessToken);
                obj = responseUpdate.Body;
                JObject responseJsonLazUpdate = JObject.Parse(obj);

                JObject responseJsonLazUpdate2 = responseJsonLazUpdate; // Initialize responseJsonLaz2 with responseJsonLaz

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
                        request.AddApiParameter("created_after", dateFrom.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                        request.AddApiParameter("created_before", dateTo.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
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
                return Json(new { set = ex.ToString(), ordersCount = ordersCount, coverage = coverage, coverageFrom = dateFrom, coverageTo = dateTo });
            }

            return Json(new { set = "Success", ordersCount = ordersCount, coverage = coverage, coverageFrom = dateFrom, coverageTo = dateTo });

        }

        public async Task<JsonResult> GetLazadaOrderListCatch([FromQuery] DateTime dateFrom, DateTime dateFromCancelled, DateTime dateTo)
        {
            var ordersCount = 0;
            var coverage = string.Empty;
            try
            {
                //var csd2 = _configuration.GetConnectionString("Myconnection");
                //using var connsd2 = new SqlConnection(csd2);
                //connsd2.Open();

                //string sqld2 = $"EXEC DeleteNoHeader @module = 'lazada'";
                //using var cmdd2 = new SqlCommand(sqld2, connsd2);
                //cmdd2.ExecuteNonQuery();
                //connsd2.Close();

                //var objCanceled = string.Empty;
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


                dateFrom = DateTime.Now;
                dateFrom = dateFrom.AddDays(-2);

                dateTo = DateTime.Now;
                dateTo = dateTo.AddHours(-6);


                //dateFrom = dateFrom.AddMinutes(-10);

                coverage = dateFrom.ToString() + " - " + dateTo.ToString();

                var accessToken = _userInfoConn.tokenTable.Select(t_code => t_code.access_token).FirstOrDefault();

                //for Canceled Status



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
                        request.AddApiParameter("created_after", dateFrom.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                        request.AddApiParameter("created_before", dateTo.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
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
                return Json(new { set = ex.ToString(), ordersCount = ordersCount, coverage = coverage, coverageFrom = dateFrom, coverageTo = dateTo });
            }

            return Json(new { set = "Success", ordersCount = ordersCount, coverage = coverage, coverageFrom = dateFrom, coverageTo = dateTo });

        }


        public async Task<JsonResult> GetLazadaOrderListReRun([FromQuery] DateTime dateFrom, DateTime dateFromCancelled, DateTime dateTo)
        {
            var ordersCount = 0;
            var coverage = string.Empty;
            try
            {
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



                dateFromCancelled = DateTime.Now.AddDays(-2);

                //dateFrom = dateFrom.AddMinutes(-10);

                coverage = dateFrom.ToString() + " - " + dateTo.ToString();

                var accessToken = _userInfoConn.tokenTable.Select(t_code => t_code.access_token).FirstOrDefault();




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
                return Json(new { set = ex.ToString(), ordersCount = ordersCount, coverage = coverage, coverageFrom = dateFrom, coverageTo = dateTo });
            }

            return Json(new { set = "Success", ordersCount = ordersCount, coverage = coverage, coverageFrom = dateFrom, coverageTo = dateTo });

        }


        public async Task<IActionResult> GetLazadaOrderList2([FromQuery] DateTime dateFrom, DateTime dateFromCancelled, DateTime dateTo)
        {
            try
            {
                //var csd2 = _configuration.GetConnectionString("Myconnection");
                //using var connsd2 = new SqlConnection(csd2);
                //connsd2.Open();

                //string sqld2 = $"EXEC DeleteNoHeader";
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


                dateTo = DateTime.Now.AddHours(-3);
                dateFrom = DateTime.Now.AddHours(-4);
                dateFromCancelled = DateTime.Now.AddHours(-3);

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
                requestCanceled.AddApiParameter("created_after", dateFromCancelled.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                requestCanceled.AddApiParameter("created_before", dateTo.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                requestCanceled.AddApiParameter("status", "canceled");
                LazopResponse responseCanceled = clientCanceled.Execute(requestCanceled, accessToken);
                objCanceled = responseCanceled.Body;
                JObject responseJsonCanceled = JObject.Parse(objCanceled);


                for (int x = 0; x < responseJsonCanceled["data"]["orders"].Count(); x++)
                {
                    var CanceledOrdersData = new List<CanceledOrders>();
                    CanceledOrdersData = _userInfoConn.CanceledOrders.Where(e => e.orderId == responseJsonCanceled["data"]["orders"][x]["order_number"].ToString()).ToList();

                    if (CanceledOrdersData.Count() < 1)
                    {
                        var canceledorder = new CanceledOrders();

                        canceledorder.orderId = responseJsonCanceled["data"]["orders"][x]["order_number"].ToString();
                        canceledorder.dateFetch = DateTime.Now;
                        canceledorder.dateProcess = DateTime.Now;
                        canceledorder.dateCreatedAt = (DateTime)responseJsonCanceled["data"]["orders"][x]["created_at"];
                        canceledorder.module = "lazada";
                        canceledorder.status = "canceled";
                        canceledorder.item_count = (decimal)responseJsonCanceled["data"]["orders"][x]["items_count"];
                        canceledorder.total_amount = (decimal)responseJsonCanceled["data"]["orders"][x]["price"];
                        _userInfoConn.Add(canceledorder);
                        _userInfoConn.SaveChanges();
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
                request.AddApiParameter("created_after", dateFrom.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                request.AddApiParameter("created_before", dateTo.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                request.AddApiParameter("status", "pending");
                LazopResponse response = clientLaz.Execute(request, accessToken);
                obj = response.Body;
                JObject responseJsonLaz = JObject.Parse(obj);


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
                return Json(new { set = "Failed" });
            }

            return Json(new { set = "Success" });

        }


        public async Task<IActionResult> GetLazadaOrderList3([FromQuery] DateTime dateFrom, DateTime dateFromCancelled, DateTime dateTo)
        {
            try
            {
                //var csd2 = _configuration.GetConnectionString("Myconnection");
                //using var connsd2 = new SqlConnection(csd2);
                //connsd2.Open();

                //string sqld2 = $"EXEC DeleteNoHeader";
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


                dateTo = DateTime.Now.AddHours(-2);
                dateFrom = DateTime.Now.AddHours(-3);
                dateFromCancelled = DateTime.Now.AddHours(-3);

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
                requestCanceled.AddApiParameter("created_after", dateFromCancelled.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                requestCanceled.AddApiParameter("created_before", dateTo.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                requestCanceled.AddApiParameter("status", "canceled");
                LazopResponse responseCanceled = clientCanceled.Execute(requestCanceled, accessToken);
                objCanceled = responseCanceled.Body;
                JObject responseJsonCanceled = JObject.Parse(objCanceled);


                for (int x = 0; x < responseJsonCanceled["data"]["orders"].Count(); x++)
                {
                    var CanceledOrdersData = new List<CanceledOrders>();
                    CanceledOrdersData = _userInfoConn.CanceledOrders.Where(e => e.orderId == responseJsonCanceled["data"]["orders"][x]["order_number"].ToString()).ToList();

                    if (CanceledOrdersData.Count() < 1)
                    {
                        var canceledorder = new CanceledOrders();

                        canceledorder.orderId = responseJsonCanceled["data"]["orders"][x]["order_number"].ToString();
                        canceledorder.dateFetch = DateTime.Now;
                        canceledorder.dateProcess = DateTime.Now;
                        canceledorder.dateCreatedAt = (DateTime)responseJsonCanceled["data"]["orders"][x]["created_at"];
                        canceledorder.module = "lazada";
                        canceledorder.status = "canceled";
                        canceledorder.item_count = (decimal)responseJsonCanceled["data"]["orders"][x]["items_count"];
                        canceledorder.total_amount = (decimal)responseJsonCanceled["data"]["orders"][x]["price"];
                        _userInfoConn.Add(canceledorder);
                        _userInfoConn.SaveChanges();
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
                request.AddApiParameter("created_after", dateFrom.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                request.AddApiParameter("created_before", dateTo.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                request.AddApiParameter("status", "pending");
                LazopResponse response = clientLaz.Execute(request, accessToken);
                obj = response.Body;
                JObject responseJsonLaz = JObject.Parse(obj);


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
                return Json(new { set = "Failed" });
            }

            return Json(new { set = "Success" });

        }


        public async Task<IActionResult> GetLazadaOrderList4([FromQuery] DateTime dateFrom, DateTime dateFromCancelled, DateTime dateTo)
        {
            try
            {
                //var csd2 = _configuration.GetConnectionString("Myconnection");
                //using var connsd2 = new SqlConnection(csd2);
                //connsd2.Open();

                //string sqld2 = $"EXEC DeleteNoHeader";
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


                dateTo = DateTime.Now.AddHours(-1);
                dateFrom = DateTime.Now.AddHours(-2);
                dateFromCancelled = DateTime.Now.AddHours(-3);

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
                requestCanceled.AddApiParameter("created_after", dateFromCancelled.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                requestCanceled.AddApiParameter("created_before", dateTo.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                requestCanceled.AddApiParameter("status", "canceled");
                LazopResponse responseCanceled = clientCanceled.Execute(requestCanceled, accessToken);
                objCanceled = responseCanceled.Body;
                JObject responseJsonCanceled = JObject.Parse(objCanceled);


                for (int x = 0; x < responseJsonCanceled["data"]["orders"].Count(); x++)
                {
                    var CanceledOrdersData = new List<CanceledOrders>();
                    CanceledOrdersData = _userInfoConn.CanceledOrders.Where(e => e.orderId == responseJsonCanceled["data"]["orders"][x]["order_number"].ToString()).ToList();

                    if (CanceledOrdersData.Count() < 1)
                    {
                        var canceledorder = new CanceledOrders();

                        canceledorder.orderId = responseJsonCanceled["data"]["orders"][x]["order_number"].ToString();
                        canceledorder.dateFetch = DateTime.Now;
                        canceledorder.dateProcess = DateTime.Now;
                        canceledorder.dateCreatedAt = (DateTime)responseJsonCanceled["data"]["orders"][x]["created_at"];
                        canceledorder.module = "lazada";
                        canceledorder.status = "canceled";
                        canceledorder.item_count = (decimal)responseJsonCanceled["data"]["orders"][x]["items_count"];
                        canceledorder.total_amount = (decimal)responseJsonCanceled["data"]["orders"][x]["price"];
                        _userInfoConn.Add(canceledorder);
                        _userInfoConn.SaveChanges();
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
                request.AddApiParameter("created_after", dateFrom.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                request.AddApiParameter("created_before", dateTo.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszz00"));
                request.AddApiParameter("status", "pending");
                LazopResponse response = clientLaz.Execute(request, accessToken);
                obj = response.Body;
                JObject responseJsonLaz = JObject.Parse(obj);


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
                return Json(new { set = "Failed" });
            }

            return Json(new { set = "Success" });

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

                string sqld = $"EXEC DeleteNoHeader @module = 'lazada'";
                using var cmdd = new SqlCommand(sqld, connsd);
                cmdd.ExecuteNonQuery();
                connsd.Close();

                throw;
            }
            // _userInfoConn.Dispose();
            return Json(new { set = obj });


        }

        public async Task<string> ShopeeTrackingNo(string access_token, string orderId)
        {
            var status = string.Empty;


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


        public async Task<string> ShopeePickupStatus(string access_token, string orderId)
        {
            var status = string.Empty;


            using (LogContext.PushProperty("Scope", "Shopee Api"))
            {



                var shopee_csd = _configuration.GetConnectionString("Myconnection");
                using var shopee_connsd = new SqlConnection(shopee_csd);
                shopee_connsd.Open();

                string urlInfo = _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:getTrackingInfo:Url"] ?? "";



                DateTime now = DateTime.Now;

                long timestamp = now.ToTimestamp();





                long partnerId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:PartnerId"),
                    shopId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:ShopId");

                var signInfo = ShopeeApiUtil.SignShopRequest(
                    partnerId: partnerId.ToString(),
                    apiPath: _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:getTrackingInfo:ApiPath"] ?? "",
                    timestamp: timestamp.ToString(),
                access_token: access_token,
                shopid: shopId,
                    partnerKey: _configuration["Infrastructure:ShopeeApi:v2:PartnerKey"] ?? "");








                using HttpClient client = _httpClientFactory.CreateClient(name: _configuration["Infrastructure:ShopeeApi:ConnectionName"] ?? "");

                string responseContentString = string.Empty;

                Stopwatch stopwatch = Stopwatch.StartNew();
                try
                {

                    string[] paramShopee = { "item_list", "buyer_username" };
                    _logger.LogTrace($"{urlInfo}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&sign={signInfo}&order_sn={orderId}");
                    string req = $"{urlInfo}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&sign={signInfo}&order_sn={orderId}";
                    Stopwatch stopwatchInfo = Stopwatch.StartNew();
                    using HttpResponseMessage httpResponseInfo = await _policy.ExecuteAsync(async (ct) => await client.GetAsync(
                    requestUri: $"{urlInfo}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&shop_id={shopId}&sign={signInfo}&order_sn={orderId}",
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

                            var boxOrder = new List<BoxOrders>();
                            boxOrder = _userInfoConn.boxOrders.Where(o => o.orderId == orderId).ToList();
                            if ((responseJson["response"]?["logistics_status"] ?? null).ToString() == "LOGISTICS_DELIVERY_DONE" || (responseJson["response"]?["logistics_status"] ?? null).ToString() == "LOGISTICS_PICKUP_DONE")
                            {
                                for (var i = 0; i < boxOrder.Count; i++)
                                {
                                    boxOrder[i].platformStatus = (responseJson["response"]?["logistics_status"] ?? null).ToString();
                                    _userInfoConn.Update(boxOrder[i]);
                                }

                                _userInfoConn.SaveChanges();
                            }
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


        public async Task<IActionResult> LazadaPickupStatus(string orderId)
        {
            try
            {

                var obj = string.Empty;
                var urlLaz = _configuration["LazadaInfrastructure:url"];
                var appkey = _configuration["LazadaInfrastructure:appkey"];
                var appSecret = _configuration["LazadaInfrastructure:appSecret"];


                var accessToken = _userInfoConn.tokenTable.Select(t_code => t_code.access_token).FirstOrDefault();

                //for  Status

                ILazopClient client = new LazopClient(urlLaz, appkey, appSecret);
                LazopRequest request = new LazopRequest();
                request.SetApiName("/order/get");
                request.SetHttpMethod("GET");
                request.AddApiParameter("order_id", orderId);
                LazopResponse response = client.Execute(request, accessToken);
                obj = response.Body;
                JObject responseJson = JObject.Parse(obj);

                var boxOrder = new List<BoxOrders>();
                boxOrder = _userInfoConn.boxOrders.Where(o => o.orderId == orderId).ToList();
                if (responseJson["data"]["statuses"][0].ToString() == "confirmed" || responseJson["data"]["statuses"][0].ToString() == "delivered" || responseJson["data"]["statuses"][0].ToString() == "shipped")
                {
                    for (var i = 0; i < boxOrder.Count; i++)
                    {
                        boxOrder[i].platformStatus = responseJson["data"]["statuses"][0].ToString();
                        _userInfoConn.Update(boxOrder[i]);
                    }

                    _userInfoConn.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                return Json(new { set = "Failed" });
            }

            return Json(new { set = "Success" });

        }
    }

}