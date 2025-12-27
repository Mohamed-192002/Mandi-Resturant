using Core.Entities;
using Core.Interfaces;
using Core.ViewModels.CashierSaleReportVM;
using Core.ViewModels.EarnReportVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace SiteFront.Areas.Owner.Controllers
{
    [Area("Owner")]
    public class CashierSaleReportController : Controller
    {
        private readonly IRepository<SaleBill> _saleBillRepo;
        private readonly IRepository<SaleBillDetail> _saleBillDetailRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Role> _roleRepo;
        private readonly IRepository<UserRole> _userRoleRepo;
        private readonly UserManager<User> _userManager;

        public CashierSaleReportController(IRepository<SaleBill> saleBillRepo,
            IRepository<SaleBillDetail> saleBillDetailRepo,
            IRepository<Product> productRepo,
            IRepository<User> userRepo,
            IRepository<Role> roleRepo,
            IRepository<UserRole> userRoleRepo,
            UserManager<User> userManager)
        {
            _saleBillRepo = saleBillRepo;
            _saleBillDetailRepo = saleBillDetailRepo;
            _productRepo = productRepo;
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _userRoleRepo = userRoleRepo;
            _userManager = userManager;
        }

        [Authorize("Permissions.CashierSaleReportIndex")]
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
                    TotalProductEarn = s.Sum(p => p.Amount) * (s.Average(p => p.Price) - _productRepo.GetByIdAsync(s.Key).Result.CostPrice),
                    Nafr = _productRepo.GetByIdAsync(s.Key).Result.Nafr ?? 0,
                    HalfNafr = _productRepo.GetByIdAsync(s.Key).Result.HalfNafr ?? 0,
                    Dagag = _productRepo.GetByIdAsync(s.Key).Result.Dagag ?? 0,
                    HalfDagag = _productRepo.GetByIdAsync(s.Key).Result.HalfDagag ?? 0
                }).ToList();
            var roleId = _roleRepo.SingleOrDefaultAsync(r => r.Name == "Cashier").Result.Id;
            var userIds = _userRoleRepo.GetAllAsync(u => u.RoleId == roleId).Result.Select(u => u.UserId).ToList();
            var users = new List<User>();
            foreach (var userId in userIds)
            {
                var user = await _userRepo.SingleOrDefaultAsync(u => !u.IsDeleted && u.Id == userId, true);
                if (user != null)
                    users.Add(user);
            }
            var cashierSaleMainVM = new CashierSaleMainVM
            {
                FromDate = DateTime.Today,
                ToDate = DateTime.Today,
                EarnSaleReportVM = earnSaleReportVM,
                Users = users.ToList(),
                TotalSale = earnSaleReportVM.Sum(e => e.TotalSalePrice),
                TotalEarn = earnSaleReportVM.Sum(e => e.TotalProductEarn),
                TotalNafr = earnSaleReportVM.Sum(e => e.TotalNafrAmount),
                TotalHalfNafr = earnSaleReportVM.Sum(e => e.TotalHalfNafrAmount),
                TotalDagag = earnSaleReportVM.Sum(e => e.TotalDagagAmount)
            };
            return View(cashierSaleMainVM);
        }

        [Authorize("Permissions.CashierSaleReportIndex")]
        public async Task<IActionResult> Search(CashierSaleMainVM model)
        {
            var cashierSaleBills = await _saleBillRepo.GetAllAsync(s => !s.Temporary && !s.Gift && s.MoneyDelivered, true);
            if (model.UserId != null)
            {
                cashierSaleBills = cashierSaleBills.Where(s => s.CreatedUser == model.UserId);
            }
            if (model.FromDate != null)
            {
                cashierSaleBills = cashierSaleBills.Where(s => s.Date.Date >= model.FromDate.Value.Date);
            }
            if (model.ToDate != null)
            {
                cashierSaleBills = cashierSaleBills.Where(s => s.Date.Date <= model.ToDate.Value.Date);
            }
            if (model.BillType != null)
            {
                cashierSaleBills = cashierSaleBills.Where(s => s.BillType == model.BillType);
            }
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
                    TotalProductEarn = s.Sum(p => p.Amount) * (s.Average(p => p.Price) - _productRepo.GetByIdAsync(s.Key).Result.CostPrice),
                    Nafr = _productRepo.GetByIdAsync(s.Key).Result.Nafr ?? 0,
                    HalfNafr = _productRepo.GetByIdAsync(s.Key).Result.HalfNafr ?? 0,
                    Dagag = _productRepo.GetByIdAsync(s.Key).Result.Dagag ?? 0,
                    HalfDagag = _productRepo.GetByIdAsync(s.Key).Result.HalfDagag ?? 0
                }).ToList();
            var roleId = _roleRepo.SingleOrDefaultAsync(r => r.Name == "Cashier").Result.Id;
            var userIds = _userRoleRepo.GetAllAsync(u => u.RoleId == roleId).Result.Select(u => u.UserId).ToList();
            var users = new List<User>();
            foreach (var userId in userIds)
            {
                var user = await _userRepo.SingleOrDefaultAsync(u => !u.IsDeleted && u.Id == userId, true);
                if (user != null)
                    users.Add(user);
            }
            var cashierSaleMainVM = new CashierSaleMainVM
            {
                EarnSaleReportVM = earnSaleReportVM,
                Users = users.ToList(),
                FromDate = model.FromDate,
                ToDate = model.ToDate,
                UserId = model.UserId,
                BillType = model.BillType,
                TotalSale = earnSaleReportVM.Sum(e => e.TotalSalePrice),
                TotalEarn = earnSaleReportVM.Sum(e => e.TotalProductEarn),
                TotalNafr = earnSaleReportVM.Sum(e => e.TotalNafrAmount),
                TotalHalfNafr = earnSaleReportVM.Sum(e => e.TotalHalfNafrAmount),
                TotalDagag = earnSaleReportVM.Sum(e => e.TotalDagagAmount)
            };
            return View("Index", cashierSaleMainVM);
        }

    }
}
