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
    public class CancelledReportController : Controller

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



        public CancelledReportController(IConfiguration configuration, UserClass tokenInfo, IWebHostEnvironment webHostEnvironment, IDbAccess dbAccess)
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


        public IActionResult CancelledReport()
        {

            return View();
        }

        public JsonResult GetExceptionReportHeader()
        {
            //if(Type == "null")
            //{
            //    Type = null;
            //}
            //IEnumerable<ExceptionReportClass> items = new List<ExceptionReportClass>();
            //items = _dbAccess.ExecuteSP2<ExceptionReportClass, dynamic>("GetCancelledReport", new {});
            //return Json(new { set = items });

            var cancelledHeader = new List<OrdersCancelledHeader>();
            cancelledHeader = _userInfoConn.OrdersCancelledHeader.ToList();


            return Json(new { set = cancelledHeader });

        }

        public JsonResult GetExceptionReportDetails(string orderId)
        {
            //if(Type == "null")
            //{
            //    Type = null;
            //}
            //IEnumerable<ExceptionReportClass> items = new List<ExceptionReportClass>();
            //items = _dbAccess.ExecuteSP2<ExceptionReportClass, dynamic>("GetCancelledReport", new {});
            //return Json(new { set = items });

            var cancelledDetails = new List<OrdersCancelledDetails>();
            cancelledDetails = _userInfoConn.OrdersCancelledDetails.Where(e => e.orderId == orderId).ToList();


            return Json(new { set = cancelledDetails });

        }


        
    }
}
