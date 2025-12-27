using AutoMapper;
using Core.Common;
using Core.Common.Enums;
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
        private readonly IRepository<SaleBillDetail> _saleBillDetailRepo;
        private readonly IRepository<Product> _productRepo;

        public DriverBillReportController(IMapper mapper,
            IRepository<SaleBill> saleBillRepo,
            IRepository<Driver> driverRepo,
            IRepository<SaleBillDetail> saleBillDetailRepo,
            IRepository<Product> productRepo)
        {
            _mapper = mapper;
            _saleBillRepo = saleBillRepo;
            _driverRepo = driverRepo;
            _saleBillDetailRepo = saleBillDetailRepo;
            _productRepo = productRepo;
        }

        [Authorize("Permissions.DriverBillReportIndex")]
        public async Task<IActionResult> Index()
        {
            var deliveryBills = await _saleBillRepo.GetAllAsync(d => !d.IsDeleted && d.BillType == BillType.Driver, true, d => d.Driver);
            var deliveryBillGetVM = _mapper.Map<List<DeliveryBillGetVM>>(deliveryBills);

            foreach (var bill in deliveryBillGetVM)
            {
                var details = await _saleBillDetailRepo.GetAllAsync(d => d.SaleBillId == bill.Id);
                foreach (var detail in details)
                {
                    var product = await _productRepo.GetByIdAsync(detail.ProductId);
                    if (product != null)
                    {
                        bill.TotalNafr += (product.Nafr ?? 0) * detail.Amount;
                        bill.TotalHalfNafr += (product.HalfNafr ?? 0) * detail.Amount;
                        bill.TotalDagag += (product.Dagag ?? 0) * detail.Amount;
                    }
                }
            }

            var drivers = await _driverRepo.GetAllAsync(d => !d.IsDeleted, true);
            var driverBillReportVM = new DriverBillReportVM
            {
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                DeliveryBillGetVM = deliveryBillGetVM,
                Drivers = _mapper.Map<List<CommonDrop>>(drivers),
                TotalPaid = deliveryBillGetVM.Where(d => d.MoneyDelivered).Sum(d => d.FinalTotal),
                TotalUnPaid = deliveryBillGetVM.Where(d => !d.MoneyDelivered).Sum(d => d.FinalTotal),
                TotalTotalNafr = deliveryBillGetVM.Sum(d => d.TotalNafr),
                TotalTotalHalfNafr = deliveryBillGetVM.Sum(d => d.TotalHalfNafr),
                TotalTotalDagag = deliveryBillGetVM.Sum(d => d.TotalDagag)
            };
            return View(driverBillReportVM);
        }

        [Authorize("Permissions.DriverBillReportIndex")]
        public async Task<IActionResult> Search(DriverBillReportVM model)
        {
            var deliveryBills = await _saleBillRepo.GetAllAsync(d => !d.IsDeleted && d.BillType == BillType.Driver, true, d => d.Driver);
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

            foreach (var bill in deliveryBillGetVM)
            {
                var details = await _saleBillDetailRepo.GetAllAsync(d => d.SaleBillId == bill.Id);
                foreach (var detail in details)
                {
                    var product = await _productRepo.GetByIdAsync(detail.ProductId);
                    if (product != null)
                    {
                        bill.TotalNafr += (product.Nafr ?? 0) * detail.Amount;
                        bill.TotalHalfNafr += (product.HalfNafr ?? 0) * detail.Amount;
                        bill.TotalDagag += (product.Dagag ?? 0) * detail.Amount;
                    }
                }
            }
            var drivers = await _driverRepo.GetAllAsync(d => !d.IsDeleted, true);
            var driverBillReportVM = new DriverBillReportVM
            {
                DeliveryBillGetVM = deliveryBillGetVM,
                Drivers = _mapper.Map<List<CommonDrop>>(drivers),
                TotalPaid = deliveryBillGetVM.Where(d => d.MoneyDelivered).Sum(d => d.FinalTotal),
                TotalUnPaid = deliveryBillGetVM.Where(d => !d.MoneyDelivered).Sum(d => d.FinalTotal),
                TotalTotalNafr = deliveryBillGetVM.Sum(d => d.TotalNafr),
                TotalTotalHalfNafr = deliveryBillGetVM.Sum(d => d.TotalHalfNafr),
                TotalTotalDagag = deliveryBillGetVM.Sum(d => d.TotalDagag),
                DriverId = model.DriverId,
                FromDate = model.FromDate,
                ToDate = model.ToDate
            };
            return View("Index", driverBillReportVM);
        }
    }
}
