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
        private readonly IRepository<SaleBillDetail> _saleBillDetailRepo;
        private readonly IRepository<Product> _productRepo;

        public DeliveryBillReportController(IMapper mapper,
            IRepository<SaleBill> saleBillRepo,
            IRepository<Delivery> deliveryRepo,
            IRepository<SaleBillDetail> saleBillDetailRepo,
            IRepository<Product> productRepo)
        {
            _mapper = mapper;
            _saleBillRepo = saleBillRepo;
            _deliveryRepo = deliveryRepo;
            _saleBillDetailRepo = saleBillDetailRepo;
            _productRepo = productRepo;
        }

        [Authorize("Permissions.DeliveryBillReportIndex")]
        public async Task<IActionResult> Index()
        {
            var deliveryBills = await _saleBillRepo.GetAllAsync(d => !d.IsDeleted && d.BillType == BillType.Delivery, true, d => d.Delivery);
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

            var deliveries = await _deliveryRepo.GetAllAsync(d => !d.IsDeleted, true);
            var deliveryBillReportVM = new DeliveryBillReportVM
            {
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                DeliveryBillGetVM = deliveryBillGetVM,
                Deliveries = _mapper.Map<List<CommonDrop>>(deliveries),
                TotalPaid = deliveryBillGetVM.Where(d => d.MoneyDelivered).Sum(d => d.FinalTotal),
                TotalUnPaid = deliveryBillGetVM.Where(d => !d.MoneyDelivered).Sum(d => d.FinalTotal),
                TotalTotalNafr = deliveryBillGetVM.Sum(d => d.TotalNafr),
                TotalTotalHalfNafr = deliveryBillGetVM.Sum(d => d.TotalHalfNafr),
                TotalTotalDagag = deliveryBillGetVM.Sum(d => d.TotalDagag)
            };
            return View(deliveryBillReportVM);
        }

        [Authorize("Permissions.DeliveryBillReportIndex")]
        public async Task<IActionResult> Search(DeliveryBillReportVM model)
        {
            var deliveryBills = await _saleBillRepo.GetAllAsync(d => !d.IsDeleted && d.BillType == BillType.Delivery, true, d => d.Delivery);
            var deliveryBillGetVM = _mapper.Map<List<DeliveryBillGetVM>>(deliveryBills);
            if (model.DeliveryId != null)
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
            var deliveries = await _deliveryRepo.GetAllAsync(d => !d.IsDeleted, true);
            var deliveryBillReportVM = new DeliveryBillReportVM
            {
                DeliveryBillGetVM = deliveryBillGetVM,
                Deliveries = _mapper.Map<List<CommonDrop>>(deliveries),
                TotalPaid = deliveryBillGetVM.Where(d => d.MoneyDelivered).Sum(d => d.FinalTotal),
                TotalUnPaid = deliveryBillGetVM.Where(d => !d.MoneyDelivered).Sum(d => d.FinalTotal),
                TotalTotalNafr = deliveryBillGetVM.Sum(d => d.TotalNafr),
                TotalTotalHalfNafr = deliveryBillGetVM.Sum(d => d.TotalHalfNafr),
                TotalTotalDagag = deliveryBillGetVM.Sum(d => d.TotalDagag),
                DeliveryId = model.DeliveryId,
                FromDate = model.FromDate,
                ToDate = model.ToDate
            };
            return View("Index", deliveryBillReportVM);
        }

        [HttpPost]
        [Authorize("Permissions.DeliveryBillReportIndex")]
        public async Task<IActionResult> MarkAsPaid(int billId)
        {
            try
            {
                var bill = await _saleBillRepo.GetByIdAsync(billId);
                if (bill == null)
                {
                    return Json(new { success = false, message = "الفاتورة غير موجودة" });
                }

                bill.MoneyDelivered = true;
                bill.LastEditDate = DateTime.Now;
                _saleBillRepo.Update(bill);

                return Json(new { success = true, message = "تم تحديث حالة الدفع بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء تحديث حالة الدفع" });
            }
        }
    }
}
