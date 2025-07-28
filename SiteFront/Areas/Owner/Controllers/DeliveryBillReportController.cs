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
    public class DeliveryBillReportController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IRepository<SaleBill> _saleBillRepo;
        private readonly IRepository<Delivery> _deliveryRepo;

        public DeliveryBillReportController(IMapper mapper,
            IRepository<SaleBill> saleBillRepo,
            IRepository<Delivery> deliveryRepo)
        {
            _mapper = mapper;
            _saleBillRepo = saleBillRepo;
            _deliveryRepo = deliveryRepo;
        }

        [Authorize("Permissions.DeliveryBillReportIndex")]
        public async Task<IActionResult> Index()
        {
            var deliveryBills = await _saleBillRepo.GetAllAsync(d => !d.IsDeleted && d.BillType == BillType.Delivery, true, d => d.Delivery);
            var deliveryBillGetVM=_mapper.Map<List<DeliveryBillGetVM>>(deliveryBills);
            var deliveries = await _deliveryRepo.GetAllAsync(d => !d.IsDeleted, true);
            var deliveryBillReportVM = new DeliveryBillReportVM
            {
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                DeliveryBillGetVM = deliveryBillGetVM,
                Deliveries = _mapper.Map<List<CommonDrop>>(deliveries),
                TotalPaid = deliveryBillGetVM.Where(d => d.MoneyDelivered).Sum(d => d.FinalTotal),
                TotalUnPaid = deliveryBillGetVM.Where(d => !d.MoneyDelivered).Sum(d => d.FinalTotal)
            };
            return View(deliveryBillReportVM);
        }

        [Authorize("Permissions.DeliveryBillReportIndex")]
        public async Task<IActionResult> Search(DeliveryBillReportVM model)
        {
            var deliveryBills = await _saleBillRepo.GetAllAsync(d => !d.IsDeleted && d.BillType == BillType.Delivery, true, d => d.Delivery);
            var deliveryBillGetVM = _mapper.Map<List<DeliveryBillGetVM>>(deliveryBills);
            if(model.DeliveryId != null)
            {
                deliveryBillGetVM = deliveryBillGetVM.Where(d => d.DeliveryId == model.DeliveryId).ToList();
            }
            if (model.FromDate != null)
            {
                deliveryBillGetVM = deliveryBillGetVM.Where(d => d.Date.Date >= model.FromDate.Value.Date).ToList();
            }
            if (model.ToDate != null)
            {
                deliveryBillGetVM = deliveryBillGetVM.Where(d => d.Date.Date <= model.ToDate.Value.Date).ToList();
            }
            var deliveries = await _deliveryRepo.GetAllAsync(d => !d.IsDeleted, true);
            var deliveryBillReportVM = new DeliveryBillReportVM
            {
                DeliveryBillGetVM = deliveryBillGetVM,
                Deliveries = _mapper.Map<List<CommonDrop>>(deliveries),
                TotalPaid = deliveryBillGetVM.Where(d => d.MoneyDelivered).Sum(d => d.FinalTotal),
                TotalUnPaid = deliveryBillGetVM.Where(d => !d.MoneyDelivered).Sum(d => d.FinalTotal),
                DeliveryId = model.DeliveryId,
                FromDate = model.FromDate,
                ToDate = model.ToDate
            };
            return View("Index", deliveryBillReportVM);
        }
    }
}
