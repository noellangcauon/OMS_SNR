using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using Serilog.Context;
using SNR_BGC.Controllers;
using SNR_BGC.Models;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.External.ShopeeWebApi
{
    
    public interface IAuthenthicationTokenFlowManager
    {
        
        Task<AuthenticationToken?> GetCurrent(CancellationToken cancellationToken);
        Task<AuthenticationToken> RequestNew(CancellationToken cancellationToken);
        Task<AuthenticationToken> Refresh(AuthenticationToken token, CancellationToken cancellationToken);
    }

    public class DatabaseStoreAuthenthicationTokenFlowManager : IAuthenthicationTokenFlowManager
    {

        private readonly IConfiguration _configuration;
        private readonly ILogger<DatabaseStoreAuthenthicationTokenFlowManager> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAsyncPolicy _policy;
        private readonly SNR_BGC.Models.UserClass _userInfoConn;

        public DatabaseStoreAuthenthicationTokenFlowManager(IConfiguration configuration, ILogger<DatabaseStoreAuthenthicationTokenFlowManager> logger, IHttpClientFactory httpClientFactory, IAsyncPolicy policy, SNR_BGC.Models.UserClass userInfoConn)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _policy = policy;
            _userInfoConn = userInfoConn;
        }

        public async Task<AuthenticationToken> GetCurrent(CancellationToken cancellationToken)
        {

            var shopee_csd = _configuration.GetConnectionString("Myconnection");
            using var shopee_connsd = new SqlConnection(shopee_csd);
            shopee_connsd.Open();

            string shopee_sql_token = "SELECT TOP 1 accessToken,accessTokenExpireDate, refreshToken, refreshTokenExpireDate FROM shopeeToken order by entryId Desc";
            using var shopee_cmd_token = new SqlCommand(shopee_sql_token, shopee_connsd);

            //JsonSerializer AuthenticationToken = (JsonSerializer)shopee_cmd_token.ExecuteScalar();

            using var reader = await shopee_cmd_token.ExecuteReaderAsync(cancellationToken);

            while(await reader.ReadAsync(cancellationToken))
            {
                return new AuthenticationToken(
                    accessToken: reader.GetString(0),
                    accessTokenExpiry: reader.GetDateTime(1),
                    refreshToken: reader.GetString(2),
                    refreshTokenExpiry: reader.GetDateTime(3));
            }

            shopee_connsd.Close();
            return null;
        }

        public async Task<AuthenticationToken> RequestNew(CancellationToken cancellationToken)
        {
            using (LogContext.PushProperty("Scope", "Shopee Api"))
            {
                string url = _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Auth:AccessToken:Url"];

                DateTime now = DateTime.Now;

                long timestamp = now.ToTimestamp();

                long partnerId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:PartnerId");

                var sign = ShopeeApiUtil.SignAuthRequest(
                                partnerKey: _configuration["Infrastructure:ShopeeApi:v2:PartnerKey"],
                                partnerId: partnerId.ToString(),
                                apiPath: _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Auth:AccessToken:ApiPath"],
                                timestamp: timestamp.ToString());

                //string redirect = _configuration["Infrastructure:ShopeeApi:v2:Auth:RedirectUrl"];

                JObject parameterJson = JObject.FromObject(
                        new
                        {
                            sign,
                            code = _configuration["Infrastructure:ShopeeApi:v2:Auth:Code"],
                            partner_id = partnerId,
                            shop_id = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:ShopId")

                        });

                string parameter = parameterJson.ToString(Formatting.None);

                using HttpClient client = _httpClientFactory.CreateClient(name: _configuration["Infrastructure:ShopeeApi:ConnectionName"]);

                using StringContent content = new StringContent(parameter, Encoding.UTF8, "application/json");

                string responseContentString = string.Empty;

                Stopwatch stopwatch = Stopwatch.StartNew();

                try
                {
                    using HttpResponseMessage httpResponse = await _policy.ExecuteAsync(async (ct) => await client.PostAsync(
                        requestUri: $"{url}?partner_id={partnerId}&timestamp={timestamp}&sign={sign}",
                        content: content,
                        cancellationToken: ct),
                        cancellationToken: cancellationToken);

                    stopwatch.Stop();

                    responseContentString = await httpResponse.Content.ReadAsStringAsync();

                    _logger.LogTrace(message: "Url:{Url}, Request:{requestBody}, Duration: {duration}s, Response Status: {responseStatus}, Responnse Body:{response}",
                        url,
                        parameter,
                        (stopwatch.ElapsedMilliseconds / 1000m).ToString("#,##0.###"),
                        httpResponse.StatusCode.ToString(),
                        responseContentString);

                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        throw new ShopeeApiException("An error occured while accessing shopee API")
                        {
                            PayloadDescription = parameter.ToString(),
                            RequestUrl = url,
                            RequestMethod = "POST",
                            ResponseDescription = responseContentString,
                            Timestamp = now,
                            HttpStatusCode = httpResponse.StatusCode
                        };
                    }

                    var responseJson = JObject.Parse(responseContentString);

                    string accessToken = responseJson.Value<string>("access_token") ?? throw new InvalidDataException(message: $"Invalid reponse JSON data: \"{responseContentString}\". missing token \"access_token\".");
                    string refreshToken = responseJson.Value<string>("refresh_token") ?? throw new InvalidDataException(message: $"Invalid reponse JSON data: \"{responseContentString}\". missing token \"access_token\".");

                    var authenticationToken = new AuthenticationToken(
                        accessToken: accessToken,
                        accessTokenExpiry: now.AddHours(4),
                        refreshToken: refreshToken,
                        refreshTokenExpiry: now.AddDays(30));

                    SetCurrentToken(authenticationToken);

                    return authenticationToken;
                }
                catch (ShopeeApiException ex)
                {
                    throw;
                }
                catch (TaskCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new ShopeeApiException("An error occured while accessing shopee API", ex)
                    {
                        PayloadDescription = parameter.ToString(),
                        RequestUrl = url,
                        RequestMethod = "POST",
                        Timestamp = now
                    };
                }
            }
        }

        public async Task<AuthenticationToken> Refresh(AuthenticationToken token, CancellationToken cancellationToken)
        {
            using (LogContext.PushProperty("Scope", "Shopee Api"))
            {
                string url = _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Auth:RefreshToken:Url"];

                DateTime now = DateTime.Now;

                long timestamp = now.ToTimestamp();

                long partnerId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:PartnerId");

                string sign = ShopeeApiUtil.SignAuthRequest(
                    partnerKey: _configuration["Infrastructure:ShopeeApi:v2:PartnerKey"],
                    partnerId: partnerId.ToString(),
                    apiPath: _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Auth:RefreshToken:ApiPath"],
                    timestamp: timestamp.ToString());

                JObject parameterJson = JObject.FromObject(
                        new
                        {
                            sign,
                            partner_id = partnerId,
                            timestamp,
                            refresh_token = token.RefreshToken,
                            shop_id = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:ShopId")
                        });

                string parameter = parameterJson.ToString(Formatting.None);

                using HttpClient client = _httpClientFactory.CreateClient(name: _configuration["Infrastructure:ShopeeApi:ConnectionName"]);

                StringContent content = new StringContent(parameter, Encoding.UTF8, "application/json");

                string responseContentString = string.Empty;

                Stopwatch stopwatch = Stopwatch.StartNew();

                try
                {
                    using HttpResponseMessage httpResponse = await _policy.ExecuteAsync(async (ct) => await client.PostAsync(
                        requestUri: $"{url}?partner_id={partnerId}&timestamp={timestamp}&sign={sign}",
                        content: content,
                        cancellationToken: ct),
                        cancellationToken: cancellationToken);

                    stopwatch.Stop();

                    /*responseContentString = await httpResponse.Content.ReadAsStringAsync(cancellationToken);*/
                    responseContentString = await httpResponse.Content.ReadAsStringAsync();

                    _logger.LogTrace(message: "Url:{Url}, Request:{requestBody}, Duration: {duration}s, Response Status: {responseStatus}, Responnse Body:{response}",
                        url,
                        parameter,
                        (stopwatch.ElapsedMilliseconds / 1000m).ToString("#,##0.###"),
                        httpResponse.StatusCode.ToString(),
                        responseContentString);

                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        throw new ShopeeApiException("An error occured while accessing shopee API")
                        {
                            PayloadDescription = parameter.ToString(),
                            RequestUrl = url,
                            RequestMethod = "POST",
                            ResponseDescription = responseContentString,
                            Timestamp = now,
                            HttpStatusCode = httpResponse.StatusCode
                        };
                    }

                    var responseJson = JObject.Parse(responseContentString);

                    string accessToken = responseJson.Value<string>("access_token") ?? string.Empty;
                    string refreshToken = responseJson.Value<string>("refresh_token") ?? string.Empty;

                    var authenticationToken = new AuthenticationToken(
                        accessToken: accessToken,
                        accessTokenExpiry: now.AddHours(4),
                        refreshToken: refreshToken,
                        refreshTokenExpiry: now.AddDays(30));

                    SetCurrentToken(authenticationToken);

                    return authenticationToken;
                }
                catch (ShopeeApiException)
                {
                    throw;
                }
                catch (TaskCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new ShopeeApiException("An error occured while accessing shopee API", ex)
                    {
                        PayloadDescription = parameter.ToString(),
                        RequestUrl = url,
                        RequestMethod = "POST",
                        Timestamp = now
                    };
                }
            }
        }

        private void SetCurrentToken(AuthenticationToken authenticationToken)
        {


            string accessToken = authenticationToken.AccessToken;
            string refreshToken = authenticationToken.RefreshToken;
            DateTime refreshTokenExpireDate = authenticationToken.RefreshTokenExpiry;
            DateTime accessTokenExpireDate = authenticationToken.AccessTokenExpiry;


            var shopee_csd = _configuration.GetConnectionString("Myconnection");
            using var shopee_connsd = new SqlConnection(shopee_csd);
            shopee_connsd.Open();

            string shopee_sql_token = "INSERT INTO shopeeToken (accessToken,accessTokenExpireDate,refreshToken,refreshTokenExpireDate) VALUES ('"+accessToken+"', '"+ accessTokenExpireDate + "', '"+refreshToken+"', '"+refreshTokenExpireDate+"')";
            using var shopee_cmd_token = new SqlCommand(shopee_sql_token, shopee_connsd);
            shopee_cmd_token.ExecuteNonQuery();

            shopee_connsd.Close();
        }
    }

    public class JsonFileStoreAuthenthicationTokenFlowManager : IAuthenthicationTokenFlowManager
    {
      
        private readonly IConfiguration _configuration;
        private readonly ILogger<JsonFileStoreAuthenthicationTokenFlowManager> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAsyncPolicy _policy;
        private readonly string _sourceFilepath;

        public JsonFileStoreAuthenthicationTokenFlowManager(IConfiguration configuration, ILogger<JsonFileStoreAuthenthicationTokenFlowManager> logger, IHttpClientFactory httpClientFactory, IAsyncPolicy policy, string sourceFilepath)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _policy = policy;
            _sourceFilepath = sourceFilepath;
          
        }

        public Task<AuthenticationToken?> GetCurrent(CancellationToken cancellationToken)
        {
            if (File.Exists(_sourceFilepath))
            {
                using (StreamReader streamReader = File.OpenText(_sourceFilepath))
                {
                    var serializer = new JsonSerializer();

                    return Task.FromResult(
                        serializer.Deserialize(
                            reader: streamReader,
                            objectType: typeof(AuthenticationToken))
                        as AuthenticationToken);
                }
            }

            return Task.FromResult<AuthenticationToken?>(null);
        }

        public async Task<AuthenticationToken> RequestNew(CancellationToken cancellationToken)
        {
            using (LogContext.PushProperty("Scope", "Shopee Api"))
            {
                string url = _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Auth:AccessToken:Url"];

                DateTime now = DateTime.Now;

                long timestamp = now.ToTimestamp();

                long partnerId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:PartnerId");

                var sign = ShopeeApiUtil.SignAuthRequest(
                                partnerKey: _configuration["Infrastructure:ShopeeApi:v2:PartnerKey"],
                                partnerId: partnerId.ToString(),
                                apiPath: _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Auth:AccessToken:ApiPath"],
                                timestamp: timestamp.ToString());

                JObject parameterJson = JObject.FromObject(
                        new
                        {
                            sign,
                            code = _configuration["Infrastructure:ShopeeApi:v2:Auth:Code"],
                            partner_id = partnerId,
                            shop_id = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:ShopId")
                        });

                string parameter = parameterJson.ToString(Formatting.None);

                using HttpClient client = _httpClientFactory.CreateClient(name: _configuration["Infrastructure:ShopeeApi:ConnectionName"]);

                using StringContent content = new StringContent(parameter, Encoding.UTF8, "application/json");

                string responseContentString = string.Empty;

                Stopwatch stopwatch = Stopwatch.StartNew();

                try
                {
                    using HttpResponseMessage httpResponse = await _policy.ExecuteAsync(async (ct) => await client.PostAsync(
                        requestUri: $"{url}?partner_id={partnerId}&timestamp={timestamp}&sign={sign}",
                        content: content,
                        cancellationToken: ct), 
                        cancellationToken: cancellationToken);

                    stopwatch.Stop();

                    responseContentString = await httpResponse.Content.ReadAsStringAsync();

                    _logger.LogTrace(message: "Url:{Url}, Request:{requestBody}, Duration: {duration}s, Response Status: {responseStatus}, Responnse Body:{response}",
                        url,
                        parameter,
                        (stopwatch.ElapsedMilliseconds / 1000m).ToString("#,##0.###"),
                        httpResponse.StatusCode.ToString(),
                        responseContentString);

                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        throw new ShopeeApiException("An error occured while accessing shopee API")
                        {
                            PayloadDescription = parameter.ToString(),
                            RequestUrl = url,
                            RequestMethod = "POST",
                            ResponseDescription = responseContentString,
                            Timestamp = now,
                            HttpStatusCode = httpResponse.StatusCode
                        };
                    }

                    var responseJson = JObject.Parse(responseContentString);

                    string accessToken = responseJson.Value<string>("access_token") ?? throw new InvalidDataException(message: $"Invalid reponse JSON data: \"{responseContentString}\". missing token \"access_token\".");
                    string refreshToken = responseJson.Value<string>("refresh_token") ?? throw new InvalidDataException(message: $"Invalid reponse JSON data: \"{responseContentString}\". missing token \"access_token\".");

                    var authenticationToken = new AuthenticationToken(
                        accessToken: accessToken,
                        accessTokenExpiry: now.AddHours(4),
                        refreshToken: refreshToken,
                        refreshTokenExpiry: now.AddDays(30));

                    SetCurrentToken(authenticationToken);

                    return authenticationToken;
                }
                catch (ShopeeApiException)
                {
                    throw;
                }
                catch (TaskCanceledException)
                {
                    throw;
                }
                catch(Exception ex)
                {
                    throw new ShopeeApiException("An error occured while accessing shopee API", ex)
                    {
                        PayloadDescription = parameter.ToString(),
                        RequestUrl = url,
                        RequestMethod = "POST",
                        Timestamp = now
                    };
                }
            }
        }

        public async Task<AuthenticationToken> Refresh(AuthenticationToken token, CancellationToken cancellationToken)
        {
            using (LogContext.PushProperty("Scope", "Shopee Api"))
            {
                string url = _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Auth:RefreshToken:Url"];

                DateTime now = DateTime.Now;

                long timestamp = now.ToTimestamp();

                long partnerId = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:PartnerId");

                string sign = ShopeeApiUtil.SignAuthRequest(
                    partnerKey: _configuration["Infrastructure:ShopeeApi:v2:PartnerKey"],
                    partnerId: partnerId.ToString(),
                    apiPath: _configuration["Infrastructure:ShopeeApi:v2:EndPoints:Auth:RefreshToken:ApiPath"],
                    timestamp: timestamp.ToString());

                JObject parameterJson = JObject.FromObject(
                        new
                        {
                            sign,
                            partner_id = partnerId,
                            timestamp,
                            refresh_token = token.RefreshToken,
                            shop_id = _configuration.GetValue<long>("Infrastructure:ShopeeApi:v2:ShopId")
                        });

                string parameter = parameterJson.ToString(Formatting.None);

                using HttpClient client = _httpClientFactory.CreateClient(name: _configuration["Infrastructure:ShopeeApi:ConnectionName"]);

                StringContent content = new StringContent(parameter, Encoding.UTF8, "application/json");

                string responseContentString = string.Empty;

                Stopwatch stopwatch = Stopwatch.StartNew();

                try
                {
                    using HttpResponseMessage httpResponse = await _policy.ExecuteAsync(async (ct) => await client.PostAsync(
                        requestUri: $"{url}?partner_id={partnerId}&timestamp={timestamp}&sign={sign}",
                        content: content,
                        cancellationToken: ct),
                        cancellationToken: cancellationToken);

                    stopwatch.Stop();

                    /*responseContentString = await httpResponse.Content.ReadAsStringAsync(cancellationToken);*/
                    responseContentString = await httpResponse.Content.ReadAsStringAsync();

                    _logger.LogTrace(message: "Url:{Url}, Request:{requestBody}, Duration: {duration}s, Response Status: {responseStatus}, Responnse Body:{response}",
                        url,
                        parameter,
                        (stopwatch.ElapsedMilliseconds / 1000m).ToString("#,##0.###"),
                        httpResponse.StatusCode.ToString(),
                        responseContentString);

                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        throw new ShopeeApiException("An error occured while accessing shopee API")
                        {
                            PayloadDescription = parameter.ToString(),
                            RequestUrl = url,
                            RequestMethod = "POST",
                            ResponseDescription = responseContentString,
                            Timestamp = now,
                            HttpStatusCode = httpResponse.StatusCode
                        };
                    }

                    var responseJson = JObject.Parse(responseContentString);

                    string accessToken = responseJson.Value<string>("access_token") ?? string.Empty;
                    string refreshToken = responseJson.Value<string>("refresh_token") ?? string.Empty;

                    var authenticationToken = new AuthenticationToken(
                        accessToken: accessToken,
                        accessTokenExpiry: now.AddHours(4),
                        refreshToken: refreshToken,
                        refreshTokenExpiry: now.AddDays(30));

                    SetCurrentToken(authenticationToken);

                    return authenticationToken;
                }
                catch (ShopeeApiException)
                {
                    throw;
                }
                catch (TaskCanceledException)
                {
                    throw;
                }
                catch(Exception ex)
                {
                    throw new ShopeeApiException("An error occured while accessing shopee API", ex)
                    {
                        PayloadDescription = parameter.ToString(),
                        RequestUrl = url,
                        RequestMethod = "POST",
                        Timestamp = now
                    };
                }
            }
        }

        private void SetCurrentToken(AuthenticationToken authenticationToken)
        {
            var serializer = new JsonSerializer();

            using (var sw = new StreamWriter(
                path: _sourceFilepath,
                append: false))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, authenticationToken);
            }

            /*var tinfo = new TokenClass();
            tinfo.codeToGetToken = code;
            tinfo.access_token = responseJson["access_token"].ToString();
            tinfo.refresh_token = responseJson["refresh_token"].ToString();
            tinfo.country_user_id = responseJson["country_user_info"][0]["user_id"].ToString();
            tinfo.country_seller_id = responseJson["country_user_info"][0]["seller_id"].ToString();
            tinfo.country_short_code = responseJson["country_user_info"][0]["short_code"].ToString();
            tinfo.refresh_expires_in = responseJson["refresh_expires_in"].ToString();
            tinfo.access_expires_in = responseJson["expires_in"].ToString();
            tinfo.dateEntry = DateTime.Now;
            tinfo.module = "shopee";
            _userInfoConn.Add(tinfo);
            _userInfoConn.SaveChanges();*/
        }
    }
}