using AutoMapper;
using Core.Common.Enums;
using Core.Common;
using Core.Entities;
using Core.Interfaces;
using Core.ViewModels.DeliveryBillVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SiteFront.Areas.Owner.Controllers
{
    [Area("Owner")]
    public class DriverBillReportController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IRepository<SaleBill> _saleBillRepo;
        private readonly IRepository<Driver> _driverRepo;

        public DriverBillReportController(IMapper mapper,
            IRepository<SaleBill> saleBillRepo,
            IRepository<Driver> driverRepo)
        {
            _mapper = mapper;
            _saleBillRepo = saleBillRepo;
            _driverRepo = driverRepo;
        }

        [Authorize("Permissions.DriverBillReportIndex")]
        public async Task<IActionResult> Index()
        {
            var deliveryBills = await _saleBillRepo.GetAllAsync(d => !d.IsDeleted && d.BillType == BillType.Driver, true, d => d.Driver);
            var deliveryBillGetVM = _mapper.Map<List<DeliveryBillGetVM>>(deliveryBills);
            var drivers = await _driverRepo.GetAllAsync(d => !d.IsDeleted, true);
            var driverBillReportVM = new DriverBillReportVM
            {
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                DeliveryBillGetVM = deliveryBillGetVM,
                Drivers = _mapper.Map<List<CommonDrop>>(drivers),
                TotalPaid = deliveryBillGetVM.Where(d => d.MoneyDelivered).Sum(d => d.FinalTotal),
                TotalUnPaid = deliveryBillGetVM.Where(d => !d.MoneyDelivered).Sum(d => d.FinalTotal)
            };
            return View(driverBillReportVM);
        }

        [Authorize("Permissions.DriverBillReportIndex")]
        public async Task<IActionResult> Search(DriverBillReportVM model)
        {
            var deliveryBills = await _saleBillRepo.GetAllAsync(d => !d.IsDeleted  && d.BillType == BillType.Driver, true, d => d.Driver);
            var deliveryBillGetVM = _mapper.Map<List<DeliveryBillGetVM>>(deliveryBills);
            if (model.DriverId != null)
            {
                deliveryBillGetVM = deliveryBillGetVM.Where(d => d.DriverId == model.DriverId).ToList();
            }
            if (model.FromDate != null)
            {
                deliveryBillGetVM = deliveryBillGetVM.Where(d => d.Date.Date >= model.FromDate.Value.Date).ToList();
            }
            if (model.ToDate != null)
            {
                deliveryBillGetVM = deliveryBillGetVM.Where(d => d.Date.Date <= model.ToDate.Value.Date).ToList();
            }
            var drivers = await _driverRepo.GetAllAsync(d => !d.IsDeleted, true);
            var driverBillReportVM = new DriverBillReportVM
            {
                DeliveryBillGetVM = deliveryBillGetVM,
                Drivers = _mapper.Map<List<CommonDrop>>(drivers),
                TotalPaid = deliveryBillGetVM.Where(d => d.MoneyDelivered).Sum(d => d.FinalTotal),
                TotalUnPaid = deliveryBillGetVM.Where(d => !d.MoneyDelivered).Sum(d => d.FinalTotal),
                DriverId = model.DriverId,
                FromDate = model.FromDate,
                ToDate = model.ToDate
            };
            return View("Index", driverBillReportVM);
        }
    }
}
