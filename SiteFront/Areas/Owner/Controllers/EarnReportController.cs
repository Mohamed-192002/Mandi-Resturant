using Core.Entities;
using Core.Interfaces;
using Core.ViewModels.EarnReportVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SiteFront.Areas.Owner.Controllers
{
    [Area("Owner")]
    public class EarnReportController : Controller
    {
        private readonly IRepository<SaleBillDetail> _saleBillDetailRepo;
        private readonly IRepository<SaleBill> _saleBillRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<Expense> _expenseRepo;

        public EarnReportController(IRepository<SaleBillDetail> saleBillDetailRepo,
            IRepository<SaleBill> saleBillRepo,
            IRepository<Product> productRepo,
            IRepository<Expense> expenseRepo
            )
        {
            _saleBillDetailRepo = saleBillDetailRepo;
            _saleBillRepo = saleBillRepo;
            _productRepo = productRepo;
            _expenseRepo = expenseRepo;
        }

        [Authorize("Permissions.EarnReportIndex")]
        public async Task<IActionResult> Index()
        {
            var cashierSaleBills = await _saleBillRepo.GetAllAsync(s => !s.Temporary && !s.Gift && s.MoneyDelivered, true);
            var cashierSaleBillIds = cashierSaleBills.Select(c => c.Id);
            var earnSaleReportVM = _saleBillDetailRepo.GetAllAsync(s => cashierSaleBillIds.Contains(s.SaleBillId)).Result
                .GroupBy(s => s.ProductId).Select(s => new EarnSaleReportVM
                {
                    ProductId = s.Key,
                    ProductName = _productRepo.GetByIdAsync(s.Key).Result.Name,
                    CostPrice = _productRepo.GetByIdAsync(s.Key).Result.CostPrice,
                    SalePrice = s.Average(p => p.Price),
                    ProductEarn = s.Average(p => p.Price) - _productRepo.GetByIdAsync(s.Key).Result.CostPrice,
                    TotalAmount = s.Sum(p => p.Amount),
                    TotalSalePrice = s.Average(p => p.Price) * s.Sum(p => p.Amount),
                    TotalProductEarn = s.Sum(p => p.Amount) * (s.Average(p => p.Price) - _productRepo.GetByIdAsync(s.Key).Result.CostPrice)
                }).ToList();
            var expenses = await _expenseRepo.GetAllAsync(e => !e.IsDeleted, true);
            var earnReportMainVM = new EarnReportMainVM
            {
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                EarnSaleReportVM = earnSaleReportVM,
                TotalSaleEarn = earnSaleReportVM.Sum(e => e.TotalProductEarn),
                TotalExpenses = expenses.Sum(e => e.Payment),
                FinalEarn = earnSaleReportVM.Sum(e => e.TotalProductEarn) - expenses.Sum(e => e.Payment)
            };
            return View(earnReportMainVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("Permissions.EarnReportIndex")]
        public async Task<IActionResult> Search(EarnReportMainVM model)
        {
            var cashierSaleBills = await _saleBillRepo.GetAllAsync(s => !s.Temporary && !s.Gift && s.MoneyDelivered, true);
            var cashierSaleBillIds = cashierSaleBills.Select(c => c.Id);
            var earnSaleReportVM = _saleBillDetailRepo.GetAllAsync(s => cashierSaleBillIds.Contains(s.SaleBillId)).Result
                .Where(s => s.CreatedDate.Date >= model.FromDate.Value.Date && s.CreatedDate.Date <= model.ToDate.Value.Date)
                .GroupBy(s => s.ProductId).Select(s => new EarnSaleReportVM
                {
                    ProductId = s.Key,
                    ProductName = _productRepo.GetByIdAsync(s.Key).Result.Name,
                    CostPrice = _productRepo.GetByIdAsync(s.Key).Result.CostPrice,
                    SalePrice = s.Average(p => p.Price),
                    ProductEarn = s.Average(p => p.Price) - _productRepo.GetByIdAsync(s.Key).Result.CostPrice,
                    TotalAmount = s.Sum(p => p.Amount),
                    TotalSalePrice = s.Average(p => p.Price) * s.Sum(p => p.Amount),
                    TotalProductEarn = s.Sum(p => p.Amount) * (s.Average(p => p.Price) - _productRepo.GetByIdAsync(s.Key).Result.CostPrice)
                }).ToList();
            var expenses = _expenseRepo.GetAllAsync(e => !e.IsDeleted, true).Result.Where(s => s.Date.Date >= model.FromDate.Value.Date && s.Date.Date <= model.ToDate.Value.Date);
            var earnReportMainVM = new EarnReportMainVM
            {
                FromDate = model.FromDate,
                ToDate = model.ToDate,
                EarnSaleReportVM = earnSaleReportVM,
                TotalSaleEarn = earnSaleReportVM.Sum(e => e.TotalProductEarn),
                TotalExpenses = expenses.Sum(e => e.Payment),
                FinalEarn = earnSaleReportVM.Sum(e => e.TotalProductEarn) - expenses.Sum(e => e.Payment)
            };
            return View("Index", earnReportMainVM);
        }

    }
}
