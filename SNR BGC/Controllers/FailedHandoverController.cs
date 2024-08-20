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

namespace SNR_BGC.Controllers
{
    public class FailedHandoverController : Controller

    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly IConfiguration _configuration;

        private readonly int _pageSize;

        private readonly string _baseUrl;

        private readonly string _baseUrlorderDetails;

        private int _currentPage = 0;

        private readonly Models.UserClass _userInfoConn;
        private static readonly Models.UserClass _userInfoConn2;

        private readonly IDbAccess _dbAccess;



        public FailedHandoverController(IConfiguration configuration, UserClass tokenInfo, IWebHostEnvironment webHostEnvironment, IDbAccess dbAccess)
        {
            _configuration = configuration;

            _webHostEnvironment = webHostEnvironment;

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            _pageSize = _configuration.GetValue<int>("ShopeeApi:v1:EndPoints:Item:GetItemsList:MaxPageSize");

            _baseUrl = _configuration["ShopeeApi:v1:EndPoints:Orders:GetOrderList:Url"];

            _baseUrlorderDetails = _configuration["ShopeeApi:v1:EndPoints:Orders:GetOrderDetails:Url"];
            _dbAccess = dbAccess;

            _userInfoConn = tokenInfo;


        }
        public IActionResult Index()
        {
            return View();
        }

        public JsonResult GetFailedHandover(DateTime? DateFrom, DateTime? DateTo)
        {
            try
            {
                if (DateTo.HasValue)
                {
                    DateTo = DateTo.Value.Date.AddDays(1).AddSeconds(-1); // Sets time to 23:59:59
                }

                IEnumerable<FailedHandoverClass> items = new List<FailedHandoverClass>();
                items = _dbAccess.ExecuteSP2<FailedHandoverClass, dynamic>("sp_GetFailedHandover", new { DateFrom, DateTo });
                return Json(new { set = items });
            }
            catch (Exception ex)
            {

            }
            return Json(new { set = "" });

        }


    }
}
