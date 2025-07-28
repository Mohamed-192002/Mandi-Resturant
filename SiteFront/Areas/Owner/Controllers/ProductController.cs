using AutoMapper;
using Core.Common;
using Core.Entities;
using Core.Interfaces;
using Core.ViewModels.ProductVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace SiteFront.Areas.Owner.Controllers
{
    [Area("Owner")]
    public class ProductController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<SaleBillDetail> _saleBillDetailRepo;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _hosting;

        public ProductController(IMapper mapper,
            IToastNotification toastNotification,
            IRepository<Product> productRepo,
            IRepository<SaleBillDetail> saleBillDetailRepo,
            UserManager<User> userManager,
            IWebHostEnvironment hosting)
        {
            _mapper = mapper;
            _toastNotification = toastNotification;
            _productRepo = productRepo;
            _saleBillDetailRepo = saleBillDetailRepo;
            _userManager = userManager;
            _hosting = hosting;
        }

        [Authorize("Permissions.ProductIndex")]
        public async Task<IActionResult> Index()
        {
            var products = await _productRepo.GetAllAsync(g => g.IsDeleted == false, true);
            var productGetVM = _mapper.Map<List<ProductGetVM>>(products);
            var productMainVM = new ProductMainVM
            {
                ProductGetVM = productGetVM
            };
            return View(productMainVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("Permissions.ProductCreate")]
        public async Task<IActionResult> Create(ProductMainVM model)
        {
            if (ModelState.IsValid)
            {
                var productDb = _mapper.Map<Product>(model.ProductRegisterVM);
                productDb.CreatedDate = DateTime.Now;
                productDb.LastEditDate = DateTime.Now;
                productDb.CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                productDb.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                if (model.ProductRegisterVM.ImageFile != null)
                {
                    var upload = Path.Combine(_hosting.WebRootPath, "SystemImages/Products/");
                    if (!Directory.Exists(upload))
                        Directory.CreateDirectory(upload);
                    var fileName = Guid.NewGuid().ToString() + '.' + model.ProductRegisterVM.ImageFile.FileName.Split('.')[1].ToString();
                    productDb.ImageUrl = fileName;
                    var fullName = Path.Combine(upload, fileName);
                    model.ProductRegisterVM.ImageFile.CopyTo(new FileStream(fullName, FileMode.Create));
                }
                _productRepo.Add(productDb);
                await _productRepo.SaveAllAsync();
                _toastNotification.AddSuccessToastMessage("تم الإضافة بنجاح");
            }
            else
            {
                _toastNotification.AddErrorToastMessage("تأكد من إكمال البيانات ثم اعد المحاولة");
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize("Permissions.ProductEdit")]
        public async Task<IActionResult> GetData(int id)
        {
            var productById = await _productRepo.GetByIdAsync(id);
            if (productById == null)
                return NotFound();
            return Json(new
            {
                id = productById.Id,
                name = productById.Name,
                costPrice = productById.CostPrice,
                sellingPrice = productById.SellingPrice,
                nafr = productById.Nafr,
                halfNafr = productById.HalfNafr,
                dagag = productById.Dagag,
                halfDagag = productById.HalfDagag
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("Permissions.ProductEdit")]
        public async Task<IActionResult> Edit(ProductMainVM model)
        {
            if (ModelState.IsValid)
            {
                var productById = await _productRepo.GetByIdAsync((int)model.ProductRegisterVM.Id);
                if (productById == null)
                    return NotFound();
                model.ProductRegisterVM.CreatedDate = productById.CreatedDate;
                model.ProductRegisterVM.CreatedUser = productById.CreatedUser;
                if (model.ProductRegisterVM.ImageFile != null)
                {
                    var upload = Path.Combine(_hosting.WebRootPath, "SystemImages/Products/");
                    if (!Directory.Exists(upload))
                        Directory.CreateDirectory(upload);
                    var fileName = Guid.NewGuid().ToString() + '.' + model.ProductRegisterVM.ImageFile.FileName.Split('.')[1].ToString();
                    model.ProductRegisterVM.ImageUrl = fileName;
                    var fullName = Path.Combine(upload, fileName);
                    model.ProductRegisterVM.ImageFile.CopyTo(new FileStream(fullName, FileMode.Create));
                }
                else
                {
                    model.ProductRegisterVM.ImageUrl = productById.ImageUrl;
                }
                var productDb = _mapper.Map(model.ProductRegisterVM, productById);
                productDb.LastEditDate = DateTime.Now;
                productDb.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                _productRepo.Update(productDb);
                await _productRepo.SaveAllAsync();
                _toastNotification.AddSuccessToastMessage("تم التعديل بنجاح");
            }
            else
            {
                _toastNotification.AddErrorToastMessage("تأكد من إكمال البيانات ثم اعد المحاولة");
            }
            return RedirectToAction("Index");
        }

        [Authorize("Permissions.ProductDelete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var productById = await _productRepo.GetByIdAsync(id);
                if (productById == null)
                    return NotFound();
                var saleBillDetails = await _saleBillDetailRepo.GetAllAsync(s => s.ProductId == id);
                if (saleBillDetails.Count() == 0)
                {
                    _productRepo.Delete(productById);
                    await _productRepo.SaveAllAsync();
                    _toastNotification.AddSuccessToastMessage("تم الحذف بنجاح");
                }
                else
                {
                    _toastNotification.AddErrorToastMessage("لا يمكن حذف هذا المنتج");
                }
            }
            catch
            {
                _toastNotification.AddErrorToastMessage("لم يتم الحذف");
            }
            return RedirectToAction("Index");
        }

    }
}
