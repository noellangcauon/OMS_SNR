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
    public class OIDTubInquiryController : Controller

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



        public OIDTubInquiryController(IConfiguration configuration, UserClass tokenInfo, IWebHostEnvironment webHostEnvironment, IDbAccess dbAccess)
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


        public IActionResult ExceptionReport()
        {

            return View();
        }
        public IActionResult ExceptionReportDiscrepancy()
        {

            return View();
        }

        public JsonResult GetInquiries(string search)
        {
            try
            {
                IEnumerable<OIDInquiriesClass> items = new List<OIDInquiriesClass>();
                items = _dbAccess.ExecuteSP2<OIDInquiriesClass, dynamic>("sp_GetInquiries", new { search });
                return Json(new { set = items });
            }
            catch (Exception ex)
            {

            }
            return Json(new { set = "" });

        }

        public JsonResult GetExceptionReportDiscrepancy(DateTime? DateFrom, DateTime? DateTo)
        {
            
            IEnumerable<ExceptionReportDiscrepancyClass> items = new List<ExceptionReportDiscrepancyClass>();
            items = _dbAccess.ExecuteSP2<ExceptionReportDiscrepancyClass, dynamic>("GetDiscrepancyReport", new { DateFrom, DateTo });
            return Json(new { set = items });

        }
        public JsonResult GetExceptionReportDiscrepancyDetails(string orderId)
        {
            //if(Type == "null")
            //{
            //    Type = null;
            //}
            //IEnumerable<ExceptionReportClass> items = new List<ExceptionReportClass>();
            //items = _dbAccess.ExecuteSP2<ExceptionReportClass, dynamic>("GetCancelledReport", new {});
            //return Json(new { set = items });
             
            IEnumerable<ExceptionReportDiscrepancyDtlClass> items = new List<ExceptionReportDiscrepancyDtlClass>();
            items = _dbAccess.ExecuteSP2<ExceptionReportDiscrepancyDtlClass, dynamic>("GetExceptionReportDiscrepancyDtl", new { orderId });
            return Json(new { set = items });


        }


    }
}
