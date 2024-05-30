using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SNR_BGC.DataAccess;
using SNR_BGC.Models;
using SNR_BGC.Models.ViewModels;
using SNR_BGC.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SNR_BGC.Controllers
{
    public class DispatchOrderController : Controller
    {
        private readonly IDataRepository _dataRepository;

        public DispatchOrderController(IDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetCourierTypes() =>
            Ok(await _dataRepository.GetCourierTypes());

        public async Task<IActionResult> GetFleetTypes() =>
           Ok(await _dataRepository.GetFleetTypes());

        public async Task<IActionResult> GetBoxOrdersByTrackingNo(string trackingNo)=>
            Ok(await _dataRepository.GetBoxOrdersByTrackingNo(trackingNo));
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DispatchOrderViewModel model)
        {
            try
            {
                var rConfig = new MapperConfiguration(cfg => cfg.CreateMap<DispatchOrderModel, DispatchOrders>());
                var rdConfig = new MapperConfiguration(cfg => cfg.CreateMap<DispatchOrderDetailModel, DispatchOrderDetails>());

                var rMapper = new Mapper(rConfig);
                var rdMapper = new Mapper(rdConfig);

                var rModel = rMapper.Map<DispatchOrders>(model.DispatchOrders);
                List<DispatchOrderDetails> rdModel = new List<DispatchOrderDetails>();

                if (model.DispatchOrderDetails != null && model.DispatchOrderDetails.Count > 0)
                    foreach (var item in model.DispatchOrderDetails)
                        rdModel.Add(rdMapper.Map<DispatchOrderDetails>(item));

                await _dataRepository.CreateDispatchOrder(rModel, rdModel);
                return Ok();
            }
            catch(Exception ex)
            {
                throw ex;
            }
            //auto mapping
          



            
        }


    }
}
