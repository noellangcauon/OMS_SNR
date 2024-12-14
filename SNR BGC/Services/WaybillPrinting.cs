using Infrastructure.External.ShopeeWebApi;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Polly;
using Serilog.Context;
using SNR_BGC.Interface;
using SNR_BGC.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SNR_BGC.Services
{
    public class WaybillPrinting : IWaybillPrinting
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAsyncPolicy _policy;
        private readonly UserClass _userInfoConn;


        public WaybillPrinting(IConfiguration configuration, IHttpClientFactory httpClientFactory, IAsyncPolicy policy, UserClass userInfoConn)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _policy = policy;
            _userInfoConn = userInfoConn;
        }

        public async Task<string> GetShippingDocumentParameter(string access_token, string orderId, string packageNumber)
        {
            string response = string.Empty;
            string jsonPayload = string.Empty;

            var array = new
            {
                order_list = new
                {
                    order_sn = orderId,
                    package_number = packageNumber
                }
            };

            jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(array);

            using (LogContext.PushProperty("Scope", "Shopee Api"))
            {
                string url = _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:getShippingDocumentParameter:Url"] ?? "";
                DateTime now = DateTime.Now;
                long timestamp = now.ToTimestamp();
                long partnerId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:PartnerId"),
                    shopId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:ShopId");
                var sign = ShopeeApiUtil.SignShopRequest(
                    partnerId: partnerId.ToString(),
                    apiPath: _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:getShippingDocumentParameter:ApiPath"] ?? "",
                    timestamp: timestamp.ToString(),
                    access_token: access_token,
                    shopid: shopId,
                    partnerKey: _configuration["Infrastructure:ShopeeApi:v2:PartnerKey"] ?? "");

                using HttpClient client = _httpClientFactory.CreateClient(name: _configuration["Infrastructure:ShopeeApi:ConnectionName"] ?? "");

                string responseContentString = string.Empty;

                Stopwatch stopwatch = Stopwatch.StartNew();

                try
                {
                    string fullUrl = $"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&sign={sign}&shop_id={shopId}";
                    using HttpResponseMessage httpResponse = await _policy.ExecuteAsync(async (ct) =>
                    {
                        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                        return await client.PostAsync(fullUrl, content, ct);
                    }, CancellationToken.None);

                   
                    // Read the response content
                    string responseContent = await httpResponse.Content.ReadAsStringAsync();
                    var responseContentJson = JObject.Parse(responseContent);

                    var errorLogs = new ErrorLogs();
                    errorLogs.orderId = orderId;
                    errorLogs.Logs = responseContent;
                    errorLogs.date = DateTime.Now;

                    _userInfoConn.Add(errorLogs);
                    _userInfoConn.SaveChanges();

                    stopwatch.Stop();
                    var message = responseContentJson["message"].ToString();
                    if (message != "")
                    {
                        return message;
                    }

                    response = "THERMAL_AIR_WAYBILL";
                }
                catch (Exception ex)
                {
                    response = ex.Message;
                }
            }

            return response;
        }

        public async Task<string> CreateShippingDocument(string access_token, string orderId, string packageNumber, string trackingNumber, string shippingDocumentType)
        {
            string response = string.Empty;
            string jsonPayload = string.Empty;

            var array = new
            {
                order_list = new
                {
                    order_sn = orderId,
                    package_number = packageNumber,
                    tracking_number = trackingNumber,
                    shipping_document_type = shippingDocumentType
                }
            };

            jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(array);

            using (LogContext.PushProperty("Scope", "Shopee Api"))
            {
                string url = _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:createShippingDocument:Url"] ?? "";
                DateTime now = DateTime.Now;
                long timestamp = now.ToTimestamp();
                long partnerId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:PartnerId"),
                    shopId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:ShopId");
                var sign = ShopeeApiUtil.SignShopRequest(
                    partnerId: partnerId.ToString(),
                    apiPath: _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:createShippingDocument:ApiPath"] ?? "",
                    timestamp: timestamp.ToString(),
                    access_token: access_token,
                    shopid: shopId,
                    partnerKey: _configuration["Infrastructure:ShopeeApi:v2:PartnerKey"] ?? "");

                using HttpClient client = _httpClientFactory.CreateClient(name: _configuration["Infrastructure:ShopeeApi:ConnectionName"] ?? "");

                string responseContentString = string.Empty;

                Stopwatch stopwatch = Stopwatch.StartNew();

                try
                {
                    string fullUrl = $"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&sign={sign}&shop_id={shopId}";
                    using HttpResponseMessage httpResponse = await _policy.ExecuteAsync(async (ct) =>
                    {
                        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                        return await client.PostAsync(fullUrl, content, ct);
                    }, CancellationToken.None);


                    // Read the response content
                    string responseContent = await httpResponse.Content.ReadAsStringAsync();
                    var responseContentJson = JObject.Parse(responseContent);

                    var errorLogs = new ErrorLogs();
                    errorLogs.orderId = orderId;
                    errorLogs.Logs = responseContent;
                    errorLogs.date = DateTime.Now;

                    _userInfoConn.Add(errorLogs);
                    _userInfoConn.SaveChanges();

                    stopwatch.Stop();
                    var message = responseContentJson["message"].ToString();
                    if (message != "")
                    {
                        return message;
                    }
                }
                catch (Exception ex)
                {
                    response = ex.Message;
                }
            }

            return response;
        }

        public async Task<string> GetShippingDocumentResult(string access_token, string orderId, string packageNumber, string shippingDocumentType)
        {
            string response = string.Empty;
            string jsonPayload = string.Empty;

            var array = new
            {
                order_list = new
                {
                    order_sn = orderId,
                    package_number = packageNumber,
                    shipping_document_type = shippingDocumentType
                }
            };

            jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(array);

            using (LogContext.PushProperty("Scope", "Shopee Api"))
            {
                string url = _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:getShippingDocumentResult:Url"] ?? "";
                DateTime now = DateTime.Now;
                long timestamp = now.ToTimestamp();
                long partnerId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:PartnerId"),
                    shopId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:ShopId");
                var sign = ShopeeApiUtil.SignShopRequest(
                    partnerId: partnerId.ToString(),
                    apiPath: _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:getShippingDocumentResult:ApiPath"] ?? "",
                    timestamp: timestamp.ToString(),
                    access_token: access_token,
                    shopid: shopId,
                    partnerKey: _configuration["Infrastructure:ShopeeApi:v2:PartnerKey"] ?? "");

                using HttpClient client = _httpClientFactory.CreateClient(name: _configuration["Infrastructure:ShopeeApi:ConnectionName"] ?? "");

                string responseContentString = string.Empty;

                Stopwatch stopwatch = Stopwatch.StartNew();

                try
                {
                    string fullUrl = $"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&sign={sign}&shop_id={shopId}";
                    using HttpResponseMessage httpResponse = await _policy.ExecuteAsync(async (ct) =>
                    {
                        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                        return await client.PostAsync(fullUrl, content, ct);
                    }, CancellationToken.None);


                    // Read the response content
                    string responseContent = await httpResponse.Content.ReadAsStringAsync();
                    var responseContentJson = JObject.Parse(responseContent);

                    var errorLogs = new ErrorLogs();
                    errorLogs.orderId = orderId;
                    errorLogs.Logs = responseContent;
                    errorLogs.date = DateTime.Now;

                    _userInfoConn.Add(errorLogs);
                    _userInfoConn.SaveChanges();

                    stopwatch.Stop();
                    var message = responseContentJson["message"].ToString();
                    if (message != "")
                    {
                        return message;
                    }
                }
                catch (Exception ex)
                {
                    response = ex.Message;
                }
            }

            return response;
        }

        public async Task<string> DownloadShippingDocument(string access_token, string orderId, string packageNumber, string shippingDocumentType)
        {
            string response = string.Empty;
            string jsonPayload = string.Empty;

            var array = new
            {
                shipping_document_type = shippingDocumentType,
                order_list = new
                {
                    order_sn = orderId,
                    package_number = packageNumber
                }
            };

            jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(array);

            using (LogContext.PushProperty("Scope", "Shopee Api"))
            {
                string url = _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:downloadShippingDocument:Url"] ?? "";
                DateTime now = DateTime.Now;
                long timestamp = now.ToTimestamp();
                long partnerId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:PartnerId"),
                    shopId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:ShopId");
                var sign = ShopeeApiUtil.SignShopRequest(
                    partnerId: partnerId.ToString(),
                    apiPath: _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Product:downloadShippingDocument:ApiPath"] ?? "",
                    timestamp: timestamp.ToString(),
                    access_token: access_token,
                    shopid: shopId,
                    partnerKey: _configuration["Infrastructure:ShopeeApi:v2:PartnerKey"] ?? "");

                using HttpClient client = _httpClientFactory.CreateClient(name: _configuration["Infrastructure:ShopeeApi:ConnectionName"] ?? "");

                string responseContentString = string.Empty;

                Stopwatch stopwatch = Stopwatch.StartNew();

                try
                {
                    string fullUrl = $"{url}?partner_id={partnerId}&timestamp={timestamp}&access_token={access_token}&sign={sign}&shop_id={shopId}";
                    using HttpResponseMessage httpResponse = await _policy.ExecuteAsync(async (ct) =>
                    {
                        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                        return await client.PostAsync(fullUrl, content, ct);
                    }, CancellationToken.None);


                    // Read the response content
                    string responseContent = await httpResponse.Content.ReadAsStringAsync();
                    var responseContentJson = JObject.Parse(responseContent);
                    stopwatch.Stop();

                    if (responseContentJson.ContainsKey("message"))
                    {
                        return responseContentJson["message"].ToString();
                    }
                    else
                    {
                        if (responseContentJson.ContainsKey("waybill"))
                        {
                            // Get the file content (assuming it's base64 encoded)
                            var base64FileContent = responseContentJson["waybill"].ToString();

                            // Decode the base64 string to a byte array
                            byte[] fileBytes = Convert.FromBase64String(base64FileContent);

                            // Define the path and file name to save it locally
                            string filePath = Path.Combine(Environment.CurrentDirectory, "DownloadedFile.pdf"); // Change extension as needed

                            // Save the byte array to a file
                            await File.WriteAllBytesAsync(filePath, fileBytes);
                        }
                    }
                }
                catch (Exception ex)
                {
                    response = ex.Message;
                }
            }

            return response;
        }
    }
}
