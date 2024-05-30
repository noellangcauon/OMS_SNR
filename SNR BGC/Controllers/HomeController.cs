using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SNR_BGC.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using SNR_BGC.Utilities;
using SNR_BGC.DataAccess;
using Microsoft.Extensions.Configuration;
using System.Net;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Microsoft.CodeAnalysis.Options;
using AspNetCore.ReportingServices.ReportProcessing.ReportObjectModel;
using Microsoft.CodeAnalysis;

namespace SNR_BGC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        private readonly Models.UserClass _userInfoConn;
        private readonly IDbAccess _dataAccess;
        private readonly IDataRepository _dataRepository;


        public HomeController(IConfiguration configuration, ILogger<HomeController> logger, UserClass userInfo, IDbAccess dataAccess, IDataRepository dataRepository)
        {
            _configuration = configuration;
            _logger = logger;
            _userInfoConn = userInfo;
            _dataAccess = dataAccess;
            _dataRepository = dataRepository;
        }
        [Authorize]
        public IActionResult Index()
        {
            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var user = claims.Claims.ToList()[0].Value;

            bool? usersTable;
            usersTable = _userInfoConn.usersTable.Where(e => e.username == user).Select(a => a.newUser).FirstOrDefault();

            string accessType;
            accessType = _userInfoConn.usersTable.Where(e => e.username == user).Select(a => a.accessType).FirstOrDefault();


            if (usersTable == true && accessType == "Native")
            {
                return RedirectToAction("ChangePassword");
            }
            else
            {
                return View();
            }



        }

        [Authorize]
        public IActionResult ChangePassword()
        {

            return View();
        }

        public JsonResult GetDataChart()
        {
            //ArrayList result = new ArrayList();
            //string[] result;
            var csd = _configuration.GetConnectionString("Myconnection");
            using var connsd = new SqlConnection(csd);
            connsd.Open();


            string sql_nob_lazada = "SELECT COUNT(*) FROM ordersTable WHERE typeOfexception LIKE '%NIB%' AND module='lazada'";
            using var cmd_nob_lazada = new SqlCommand(sql_nob_lazada, connsd);
            var nob_lazada = cmd_nob_lazada.ExecuteScalar();

            string sql_oos_lazada = "SELECT COUNT(*) FROM ordersTable WHERE typeOfexception LIKE '%OOS%' AND module='lazada'";
            using var cmd_oos_lazada = new SqlCommand(sql_oos_lazada, connsd);
            var oos_lazada = cmd_oos_lazada.ExecuteScalar();

            //string sql_sku_lazada = "SELECT COUNT(*) FROM ordersTable WHERE typeOfexception LIKE '%SKU%' AND module='lazada'";
            //using var cmd_sku_lazada = new SqlCommand(sql_sku_lazada, connsd);
            //var sku_lazada = cmd_sku_lazada.ExecuteScalar();

            string sql_clear_lazada = "SELECT COUNT(*) FROM ordersTable WHERE exception = 0 AND module='lazada'";
            using var cmd_clear_lazada = new SqlCommand(sql_clear_lazada, connsd);
            var clear_lazada = cmd_clear_lazada.ExecuteScalar();


            var result_lazada = new int[] { Convert.ToInt32(clear_lazada), Convert.ToInt32(oos_lazada), Convert.ToInt32(nob_lazada) };


            string sql_nob_shopee = "SELECT COUNT(*) FROM ordersTable WHERE typeOfexception LIKE '%NIB%' AND module='shopee'";
            using var cmd_nob_shopee = new SqlCommand(sql_nob_shopee, connsd);
            var nob_shopee = cmd_nob_shopee.ExecuteScalar();

            string sql_oos_shopee = "SELECT COUNT(*) FROM ordersTable WHERE typeOfexception LIKE '%OOS%' AND module='shopee'";
            using var cmd_oos_shopee = new SqlCommand(sql_oos_shopee, connsd);
            var oos_shopee = cmd_oos_shopee.ExecuteScalar();

            //string sql_sku_shopee = "SELECT COUNT(*) FROM ordersTable WHERE typeOfexception LIKE '%SKU%' AND module='shopee'";
            //using var cmd_sku_shopee = new SqlCommand(sql_sku_shopee, connsd);
            //var sku_shopee = cmd_sku_shopee.ExecuteScalar();

            string sql_clear_shopee = "SELECT COUNT(*) FROM ordersTable WHERE exception = 0 AND module='shopee'";
            using var cmd_clear_shopee = new SqlCommand(sql_clear_shopee, connsd);
            var clear_shopee = cmd_clear_shopee.ExecuteScalar();


            var result_shopee = new int[] { Convert.ToInt32(clear_shopee), Convert.ToInt32(oos_shopee), Convert.ToInt32(nob_shopee) };

            connsd.Close();

            return Json(new { lazada = result_lazada, shopee = result_shopee });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet("login")]
        public IActionResult login(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var model = InitializeYourViewModel();

            return View(model);
        }
        private ChooseAppClass InitializeYourViewModel()
        {
            // Logic to initialize YourViewModel
            return new ChooseAppClass
            {
                Options = GetOptionsDefault(),
                SelectedOptionId = 2 // Set the default selected option
            };
        }
        private List<SelectListItem> GetOptionsDefault()
        {
            return new List<SelectListItem>
        {
            new SelectListItem { Value = "0", Text = "Please Choose App" },
        };
        }

        private List<SelectListItem> GetOptions(string username)
        {
            var result = _userInfoConn.usersTable.Where(u => u.username == username).FirstOrDefault();

            if (result == null)
            {
                return new List<SelectListItem>
                 {
                     new SelectListItem { Value = "No App", Text = "Please Choose App" },
                 };
            }
            else
            {
                var options = new List<SelectListItem>();

                if (result.withOmsAccess == true)
                {
                    options.Add(new SelectListItem { Value = "OMS", Text = "OMS" });
                }
                if (result.withRunnerAccess == true)
                {
                    options.Add(new SelectListItem { Value = "Runner", Text = "Runner" });
                }
                if (result.withPickerAccess == true)
                {
                    options.Add(new SelectListItem { Value = "Picker", Text = "Picker" });
                }
                if (result.withBoxerAccess == true)
                {
                    options.Add(new SelectListItem { Value = "Boxer", Text = "Packer" });
                }
                if (username == "autoreloadshopee@snrshopping.com")
                {
                    options.Add(new SelectListItem { Value = "AutoReloadShopee", Text = "AutoReloadShopee" });
                }
                if (username == "autoreloadlazada@snrshopping.com")
                {
                    options.Add(new SelectListItem { Value = "AutoReloadLazada", Text = "AutoReloadLazada" });
                }
                if (username == "autoreloadcatchshopee@snrshopping.com")
                {
                    options.Add(new SelectListItem { Value = "AutoReloadCatchShopee", Text = "AutoReloadCatchShopee" });
                }
                if (username == "autoreloadcatchlazada@snrshopping.com")
                {
                    options.Add(new SelectListItem { Value = "AutoReloadCatchLazada", Text = "AutoReloadCatchLazada" });
                }
                if (username == "autoreloadrerun@snrshopping.com")
                {
                    options.Add(new SelectListItem { Value = "AutoReloadReRun", Text = "AutoReloadReRun" });
                }

                return options;
            }
        }

        [HttpGet]
        public IActionResult YourAjaxAction(string username)
        {
            var model = new ChooseAppClass
            {
                Options = GetOptions(username),
                SelectedOptionId = Convert.ToInt32(1)
            };

            // Render a partial view or return a JSON object containing the updated options
            return Json(model.Options);
        }

        [HttpPost("login")]

        public async Task<IActionResult> Validate(string username, string password, string selectapp, string isActiveDirectory, string returnUrl)
        {
            username = username.Trim();
            if (username == null)
            {
                username = string.Empty;
            }
            if (password == null)
            {
                password = string.Empty;
            }
            if (selectapp == null || selectapp == null)
            {
                selectapp = string.Empty;
            }

            username = username + "@snrshopping.com";
            //Modified 
            //split username;
            if (username != null || password != null)
            {
                if (isActiveDirectory == "on")
                {

                    var csd = _configuration.GetConnectionString("Myconnection");
                    using var connsd = new SqlConnection(csd);
                    connsd.Open();


                    ViewData["ReturnUrl"] = returnUrl;
                    var adContext = new PrincipalContext(ContextType.Domain, "snrshopping.com");
                    if (adContext.ValidateCredentials(username, password))
                    {
                        var claims = new List<Claim>();
                        claims.Add(new Claim("username", username));
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, username));

                        string sql = "SELECT OmsSubModule FROM usersTable WHERE username='" + username + "'";
                        using var cmd = new SqlCommand(sql, connsd);
                        cmd.ExecuteNonQuery();
                        var subModule = cmd.ExecuteScalar();


                        var userTable = new UsersTable();
                        userTable = _userInfoConn.usersTable.Where(e => e.username == username).FirstOrDefault();

                        if (subModule != null)
                        {
                            var uinfos =
                            (from uinfo in _userInfoConn.usersTable
                             where uinfo.username == username
                             select new UserInfoClass { userRole = uinfo.userRole, userSubModule = uinfo.OmsSubModule }).ToArray();
                            claims.Add(new Claim("userRole", uinfos.ToList()[0].userRole));
                            string access = selectapp;
                            claims.Add(new Claim("userAccess", access));

                            for (var x = 0; x < uinfos.Count(); x++)
                            {
                                claims.Add(new Claim("userSubModule", uinfos[x].userSubModule));

                            }
                        }
                        else
                        {
                            claims.Add(new Claim("userSubModule", "No Module"));
                            claims.Add(new Claim("userRole", "Visitor"));
                        }

                        claims.Add(new Claim("userFullname", userTable.userFullname));


                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        await HttpContext.SignInAsync(claimsPrincipal);
                        connsd.Close();
                        return Redirect(returnUrl);
                    }
                    TempData["Error"] = "Failed";
                    connsd.Close();
                    return View("login");
                }
                else
                {
                    var userTable = new UsersTable();

                    userTable = _userInfoConn.usersTable.Where(e => e.username == username && e.password == EncryptorDecryptor.Encrypt(password)).FirstOrDefault();


                    if (userTable != null)
                    {
                        //if (username == "autoreload@snrshopping.com")
                        //{
                        //    string hostName = Dns.GetHostName();

                        //    // Get the IP addresses associated with the host
                        //    IPAddress[] localIPs = Dns.GetHostAddresses(hostName);
                        //    bool isIPApproved = false;
                        //    // Display the local IP addresses
                        //    foreach (IPAddress ipAddress in localIPs)
                        //    {
                        //        Console.WriteLine("Local IP Address: " + ipAddress);

                        //        var IPTable = new IPaddressForAutoReload();

                        //        IPTable = _userInfoConn.IPaddressForAutoReload.Where(e => e.IPaddress == ipAddress.ToString()).FirstOrDefault();


                        //        if (IPTable != null)
                        //        {
                        //            isIPApproved = true;
                        //        }

                        //    }
                        //    if (isIPApproved)
                        //    {
                        //        string access = selectapp;
                        //        string subModule = (userTable.OmsSubModule == null ? "No Module" : userTable.OmsSubModule.ToString());

                        //        var claims = new List<Claim>();
                        //        claims.Add(new Claim("username", username));
                        //        claims.Add(new Claim(ClaimTypes.NameIdentifier, username));
                        //        claims.Add(new Claim("userRole", userTable.userRole));
                        //        claims.Add(new Claim("userAccess", access));
                        //        claims.Add(new Claim("userSubModule", subModule));
                        //        claims.Add(new Claim("userFullname", userTable.userFullname));
                        //        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        //        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        //        await HttpContext.SignInAsync(claimsPrincipal);
                        //        return Redirect(returnUrl);
                        //    }
                        //    else
                        //    {
                        //        TempData["Error"] = "Failed";

                        //        ViewData["ReturnUrl"] = returnUrl;
                        //        return View("login");
                        //    }
                        //}
                        //else
                        //{
                        string access = selectapp;
                        string subModule = (userTable.OmsSubModule == null ? "No Module" : userTable.OmsSubModule.ToString());

                        var claims = new List<Claim>();
                        claims.Add(new Claim("username", username));
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, username));
                        claims.Add(new Claim("userRole", userTable.userRole));
                        claims.Add(new Claim("userAccess", access));
                        claims.Add(new Claim("userSubModule", subModule));
                        claims.Add(new Claim("userFullname", userTable.userFullname));
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        await HttpContext.SignInAsync(claimsPrincipal);
                        return Redirect(returnUrl);

                    }

                    TempData["Error"] = "Failed";

                    ViewData["ReturnUrl"] = returnUrl;
                    return View("login");
                }
            }
            else
            {
                TempData["Error"] = "Failed";
                ViewData["ReturnUrl"] = returnUrl;
                return View("login");
            }

        }
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public JsonResult ViewNIB()
        {
            try
            {

                var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
                var user = claims.Claims.ToList()[0].Value;

                if (claims.Claims.ToList()[2].Value.Contains("AutoReload"))
                {
                    if (claims.Claims.ToList()[2].Value.Contains("AutoReloadShopee"))
                    {
                        
                            return Json(new { user = "AutoReloadShopee" });

                        
                    }
                    else if (claims.Claims.ToList()[2].Value.Contains("AutoReloadLazada"))
                    {


                        return Json(new { user = "AutoReloadLazada" });


                    }
                    else if (claims.Claims.ToList()[2].Value.Contains("AutoReloadCatchLazada"))
                    {


                        return Json(new { user = "AutoReloadCatchLazada" });


                    }
                    else if (claims.Claims.ToList()[2].Value.Contains("AutoReloadCatchShopee"))
                    {


                        return Json(new { user = "AutoReloadCatchShopee" });


                    }
                    else
                    {

                        return Json(new { user = "AutoReloadReRun" });

                    }
                }

                else if (claims.Claims.ToList()[3].Value.Contains("OMS"))
                {
                    //var result = new List<OrderClass>();
                    //result = _userInfoConn.ordersTable.Where(u => u.exception == 1 && u.typeOfexception == "NIB").ToList();

                    //_userInfoConn.Dispose();


                    return Json(new { user = "OMS" });
                }
                else if (claims.Claims.ToList()[3].Value.Contains("Runner"))
                {
                    var result = new List<OrderClass>();
                    result = _userInfoConn.ordersTable.Where(u => u.exception == 1 && u.typeOfexception == "NIB" && u.runnerUser == user && u.runnerStatus != "Transferred").ToList();


                    if (result.Count > 0)
                    {

                        _userInfoConn.Dispose();
                        return Json(new { set = result, user = "Runner" });
                    }
                    else
                    {
                        result = _userInfoConn.ordersTable.Where(u => u.exception == 1 && u.typeOfexception == "NIB" && u.runnerUser == null).ToList();

                        _userInfoConn.Dispose();
                        return Json(new { set = result, user = "Runner" });
                    }



                }
                else if (claims.Claims.ToList()[3].Value.Contains("Picker"))
                {
                    var result = new List<ClearedOrders>();
                    var ordersTableHeader = _userInfoConn.orderTableHeader.Where(u => u.exception == 1).Select(x => x.orderId).ToArray();
                    result = _userInfoConn.clearedOrders.Where(e => e.pickerUser == user && e.pickerStatus != "Done" && !ordersTableHeader.Contains(e.orderId)).ToList();

                    if (result.Count > 0)
                    {

                        _userInfoConn.Dispose();
                        return Json(new { set = result, user = "Picker" });
                    }
                    else
                    {
                        result = _userInfoConn.clearedOrders.Where(u => u.pickerUser == null && u.pickerStatus == null && !ordersTableHeader.Contains(u.orderId)).ToList();

                        _userInfoConn.Dispose();
                        return Json(new { set = result, user = "Picker" });
                    }

                }
                else if (claims.Claims.ToList()[3].Value.Contains("Boxer"))
                {
                    var result = new List<BoxOrders>();
                    result = _userInfoConn.boxOrders.ToList();

                    _userInfoConn.Dispose();


                    return Json(new { set = result, user = "Boxer" });
                }
                else
                {
                    return Json(new { set = "NotRunner" });
                }

            }
            catch (Exception ex)
            {

                return Json(new { set = ex });
            }

        }

        public JsonResult CheckPasswords(string oldPass, string newPass, string confirmPass)
        {
            var userTable = new UsersTable();

            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var user = claims.Claims.ToList()[0].Value;

            userTable = _userInfoConn.usersTable.Where(e => e.username == user && e.password == EncryptorDecryptor.Encrypt(oldPass)).FirstOrDefault();

            if (userTable != null)
            {
                if (newPass == confirmPass)
                {

                    if (newPass != "Pw@12345")
                    {
                        userTable.password = EncryptorDecryptor.Encrypt(newPass);
                        userTable.newUser = false;
                        _userInfoConn.Update(userTable);


                        _userInfoConn.SaveChanges();



                        return Json(new { set = "Success" });
                    }
                    else
                    {
                        return Json(new { set = "DefaultPassword" });

                    }

                }
                else
                {

                    return Json(new { set = "ConfirmNotMatch" });
                }
            }
            else
            {
                return Json(new { set = "InvalidOldPassword" });

            }


        }


        public JsonResult ScannedQr(string result)
        {
            try
            {
                var csd = _configuration.GetConnectionString("Myconnection");
                using var connsd = new SqlConnection(csd);
                connsd.Open();

                string tubresult = $"EXEC [GetTubForBoxer] '{result}'";
                using var tub_result = new SqlCommand(tubresult, connsd);
                var resultTub = tub_result.ExecuteScalar();

                connsd.Close();

                return Json(new { set = resultTub});


                //var clearedOrders = new List<ClearedOrders>();
                //clearedOrders = _userInfoConn.clearedOrders.Where(e => e.boxQRCode == result).ToList();

                //if (clearedOrders.Count < 1)
                //{
                //    return Json(new
                //    {
                //        set = clearedOrders
                //    });
                //}
                //// 
                //var boxOrders = new BoxOrders();
                //var boxOrders2 = new BoxOrders();
                //for (int i = 0; i < clearedOrders.Count; i++)
                //{

                //    boxOrders = _userInfoConn.boxOrders.Where(e => e.boxerStatus == "Done" && e.orderId == clearedOrders[i].orderId).FirstOrDefault();

                //    boxOrders2 = _userInfoConn.boxOrders.Where(e => e.boxerStatus == "Discrepancy" && e.orderId == clearedOrders[i].orderId).FirstOrDefault();

                //    if (boxOrders == null)
                //    {

                //        return Json(new { set = clearedOrders, box = boxOrders, discrepancy = boxOrders2, orderId = clearedOrders[i].orderId });
                //    }



                //}



                //return Json(new { set = clearedOrders, box = boxOrders, discrepancy = boxOrders2 });

            }
            catch (Exception ex)
            {

                return Json(new { set = ex });
            }

        }

        public async Task<IActionResult> GetOMSDashboard(string condition, string dateFrom, string dateTo) =>
             Ok(await _dataRepository.GetOMSDashboard(condition, dateFrom, dateTo));

        public async Task<IActionResult> GetOrderDetails(string condition, string dateFrom, string dateTo)
        {
            try
            {
                IEnumerable<GridOrderHeaderClass> items = new List<GridOrderHeaderClass>();
                items = _dataAccess.ExecuteSP2<GridOrderHeaderClass, dynamic>("GetOrderDetails", new { condition, dateFrom, dateTo });


                return Json(new { set = items });
            }
            catch (Exception ex)
            {
                return Json(new { set = ex });
            }

        }

        public JsonResult GetItemOrder([FromQuery] string order_id)
        {
            var result = new List<OrderClass>();
            result = _userInfoConn.ordersTable.Where(p => p.orderId == order_id).ToList();
            return Json(new { set = result });


        }




        public JsonResult GetPickingTimePerPicker(string condition)
        {
            IEnumerable<OMSDashboardModel> items = new List<OMSDashboardModel>();
            items = _dataAccess.ExecuteSP2<OMSDashboardModel, dynamic>("GetPickingTimePerPicker", new { condition });


            return Json(new { set = items });
        }




        public JsonResult GetPackingTimePerPicker(string condition)
        {
            IEnumerable<OMSDashboardModel> items = new List<OMSDashboardModel>();
            items = _dataAccess.ExecuteSP2<OMSDashboardModel, dynamic>("GetPackingTimePerPicker", new { condition });


            return Json(new { set = items });
        }

        public async Task<IActionResult> GetInventoryItem(string dateFrom, string dateTo)
        {
            IEnumerable<InvetoryItemQtyViewModel> items = new List<InvetoryItemQtyViewModel>();
            items = _dataAccess.ExecuteSP2<InvetoryItemQtyViewModel, dynamic>("GetInventoryItem", new { dateFrom, dateTo });
            return Json(new { set = items });
        }

        public JsonResult GetOrdersPerHour(string dateFrom, string dateTo)
        {
            IEnumerable<OMSDashboardModel> items = new List<OMSDashboardModel>();
            items = _dataAccess.ExecuteSP2<OMSDashboardModel, dynamic>("GetOrdersPerHour", new { dateFrom, dateTo });


            return Json(new { set = items });
        }

        public JsonResult GetReadyToShipOrdersPerHour(string dateFrom, string dateTo)
        {
            IEnumerable<OMSDashboardModel> items = new List<OMSDashboardModel>();
            items = _dataAccess.ExecuteSP2<OMSDashboardModel, dynamic>("GetReadyToShipOrdersPerHour", new { dateFrom, dateTo });


            return Json(new { set = items });
        }


    }
}
