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
using Microsoft.EntityFrameworkCore;

namespace SNR_BGC.Controllers
{
    public class NIBItemsController : Controller

    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly IConfiguration _configuration;

        private readonly int _pageSize;

        private readonly string _baseUrl;

        private readonly string _baseUrlorderDetails;

        private int _currentPage = 0;

        private readonly Models.UserClass _userInfoConn;
        private static readonly Models.UserClass _userInfoConn2;



        public NIBItemsController(IConfiguration configuration, UserClass tokenInfo, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;

            _webHostEnvironment = webHostEnvironment;

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            _pageSize = _configuration.GetValue<int>("ShopeeApi:v1:EndPoints:Item:GetItemsList:MaxPageSize");

            _baseUrl = _configuration["ShopeeApi:v1:EndPoints:Orders:GetOrderList:Url"];

            _baseUrlorderDetails = _configuration["ShopeeApi:v1:EndPoints:Orders:GetOrderDetails:Url"];


            _userInfoConn = tokenInfo;


        }
        public IActionResult Index()
        {
            return View();
        }


        public IActionResult ViewNIBItems()
        {

            return View();
        }

        public JsonResult GetNIBItems()
        {
            

            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var csd = _configuration.GetConnectionString("Myconnection");
            using var connsd = new SqlConnection(csd);
            connsd.Open();


            var user = claims.Claims.ToList()[0].Value;
            string sqld = $"EXEC GetNIBItems";
            using var cmdd = new SqlCommand(sqld, connsd);
            SqlDataReader result_clear = cmdd.ExecuteReader();

            List<NIBClass> items = new List<NIBClass>();

            while (result_clear.Read())
            {
                NIBClass item = new NIBClass();
                item.sku_id = result_clear["sku_id"].ToString();
                item.item_description = result_clear["item_description"].ToString();
                item.Quantity = (int)result_clear["Quantity"];
                item.dateProcess = (DateTime?)result_clear["dateProcess"];
                item.onhand = (int)result_clear["onhand"];
                item.onhand_basement = (int)result_clear["onhand_basement"];
                items.Add(item);

            }
            connsd.Close();


            List<RemarksTable> remarksData = new List<RemarksTable>();

            return Json(new { set = items, remarksData = remarksData });


        }

        public JsonResult ClearItems(NIBViewModel NIBClass)
        {

            var csd = _configuration.GetConnectionString("Myconnection");
            using var connsd = new SqlConnection(csd);
            connsd.Open();


            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var user = claims.Claims.ToList()[0].Value;
            for (int y = 0; y < NIBClass.NIBClasses.Count; y++)
            {
                var ordersTable = new List<OrderClass>();
                ordersTable = _userInfoConn.ordersTable.Where(e => e.sku_id == NIBClass.NIBClasses[y].sku_id && e.typeOfexception == "NIB").ToList();
                var orderId = string.Empty;


                for (int x = 0; x < ordersTable.Count; x++)
                {


                    ordersTable[x].exception = 0;
                    ordersTable[x].typeOfexception = "";
                    //ordersTable[x].NIBRemarks = NIBClass.NIBClasses[y].Remarks;
                    _userInfoConn.Update(ordersTable[x]);



                }
                _userInfoConn.SaveChanges();

                for (int x = 0; x < ordersTable.Count; x++)
                {
                    orderId = ordersTable[x].orderId;
                    var ordersTableException = new List<OrderClass>();
                    ordersTableException = _userInfoConn.ordersTable.Where(e => e.orderId == orderId && e.exception == 1).ToList();


                    if (ordersTableException.Count < 1)
                    {
                        var ordersTableHeader = new OrderHeaderClass();
                        ordersTableHeader = _userInfoConn.orderTableHeader.Where(e => e.orderId == orderId).FirstOrDefault();

                        ordersTableHeader.exception = 0;
                        ordersTableHeader.status = "cleared";
                        _userInfoConn.Update(ordersTableHeader);

                        _userInfoConn.SaveChanges();

                        var clearedTable = new List<ClearedOrders>();
                        clearedTable = _userInfoConn.clearedOrders.Where(e => e.orderId == orderId).ToList();
                        if (clearedTable.Count < 1)
                        {
                            string sqld1_add = $"INSERT INTO clearedOrders (deductedStockEcom, deductedStock2017, dateProcess, skuId, orderId, module, processBy, isFreeItem, isNIB, isFromNIB) SELECT 1, 1, GETDATE(), sku_id, orderId, module, 'System', 0, 0, 0 FROM ordersTable WHERE orderId = '{orderId}' AND platform_status <> 'canceled'";
                            using var cmdd1_add = new SqlCommand(sqld1_add, connsd);
                            cmdd1_add.ExecuteNonQuery();
                        }
                        else
                        {

                            var clearedTable2 = new List<ClearedOrders>();
                            clearedTable2 = _userInfoConn.clearedOrders.Where(e => e.orderId == orderId && e.isNIB == true).ToList();
                            for(int j = 0; j < clearedTable2.Count; j++)
                            {
                                clearedTable2[j].isFromNIB = true;
                                _userInfoConn.Update(clearedTable2[j]);

                            }



                            for (int i = 0; i < clearedTable.Count; i++)
                            {
                                clearedTable[i].isNIB = false;
                                clearedTable[i].pickerUser = null;
                                _userInfoConn.Update(clearedTable[i]);


                            }

                            _userInfoConn.SaveChanges();
                        }

                    }
                }

            }






            return Json(new { set = NIBClass });

        }
        public JsonResult UpdateItems(NIBViewModel NIBClass)
        {


            var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var user = claims.Claims.ToList()[0].Value;
            if (NIBClass.NIBClasses != null)
            {
                for (int y = 0; y < NIBClass.NIBClasses.Count; y++)
                {
                    var ordersTable = new List<OrderClass>();
                    ordersTable = _userInfoConn.ordersTable.Where(e => e.sku_id == NIBClass.NIBClasses[y].sku_id && e.typeOfexception == "NIB").ToList();
                    var orderId = string.Empty;

                    for (int x = 0; x < ordersTable.Count; x++)
                    {


                        //ordersTable[x].NIBRemarks = NIBClass.NIBClasses[y].Remarks;
                        _userInfoConn.Update(ordersTable[x]);

                    }
                    _userInfoConn.SaveChanges();



                }
            }





            return Json(new { set = NIBClass });

        }

    }
}
