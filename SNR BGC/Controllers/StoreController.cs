using Microsoft.AspNetCore.Mvc;
using SNR_BGC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SNR_BGC.Controllers
{
    public class StoreController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AddStore()
        {
            return View();
        }
 
    }
}
