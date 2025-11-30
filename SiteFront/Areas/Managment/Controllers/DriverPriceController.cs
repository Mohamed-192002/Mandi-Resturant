using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Core.ViewModels.DriverPriceVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace SiteFront.Areas.Managment.Controllers
{
    [Area("Managment")]
    public class DriverPriceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private readonly IRepository<DriverPrice> _driverPriceRepo;
        private readonly UserManager<User> _userManager;

        public DriverPriceController(IMapper mapper,
            IToastNotification toastNotification,
            IRepository<DriverPrice> driverPriceRepo,
            UserManager<User> userManager)
        {
            _mapper = mapper;
            _toastNotification = toastNotification;
            _driverPriceRepo = driverPriceRepo;
            _userManager = userManager;
        }

        [Authorize("Permissions.DriverPriceIndex")]
        public async Task<IActionResult> Index()
        {
            var driverPrices = await _driverPriceRepo.GetAllAsync(g => g.IsDeleted == false, true);
            var driverPriceGetVM = _mapper.Map<List<DriverPriceGetVM>>(driverPrices);
            var driverPriceMainVM = new DriverPriceMainVM
            {
                DriverPriceGetVM = driverPriceGetVM
            };
            return View(driverPriceMainVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("Permissions.DriverPriceCreate")]
        public async Task<IActionResult> Create(DriverPriceMainVM model)
        {
            if (ModelState.IsValid)
            {
                var driverPriceDb = _mapper.Map<DriverPrice>(model.DriverPriceRegisterVM);
                driverPriceDb.CreatedDate = DateTime.Now;
                driverPriceDb.LastEditDate = DateTime.Now;
                driverPriceDb.CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                driverPriceDb.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                _driverPriceRepo.Add(driverPriceDb);
                await _driverPriceRepo.SaveAllAsync();
                _toastNotification.AddSuccessToastMessage("تم الإضافة بنجاح");
            }
            else
            {
                _toastNotification.AddErrorToastMessage("تأكد من إكمال البيانات ثم اعد المحاولة");
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize("Permissions.DriverPriceEdit")]
        public async Task<IActionResult> GetData(int id)
        {
            var driverPriceById = await _driverPriceRepo.GetByIdAsync(id);
            if (driverPriceById == null)
                return NotFound();
            return Json(new { id = driverPriceById.Id, region = driverPriceById.Region, deliveryPrice = driverPriceById.DeliveryPrice });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("Permissions.DriverPriceEdit")]
        public async Task<IActionResult> Edit(DriverPriceMainVM model)
        {
            if (ModelState.IsValid)
            {
                var driverPriceById = await _driverPriceRepo.GetByIdAsync((int)model.DriverPriceRegisterVM.Id);
                if (driverPriceById == null)
                    return NotFound();
                model.DriverPriceRegisterVM.CreatedDate = driverPriceById.CreatedDate;
                model.DriverPriceRegisterVM.CreatedUser = driverPriceById.CreatedUser;
                var driverPriceDb = _mapper.Map(model.DriverPriceRegisterVM, driverPriceById);
                driverPriceDb.LastEditDate = DateTime.Now;
                driverPriceDb.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                _driverPriceRepo.Update(driverPriceDb);
                await _driverPriceRepo.SaveAllAsync();
                _toastNotification.AddSuccessToastMessage("تم التعديل بنجاح");
            }
            else
            {
                _toastNotification.AddErrorToastMessage("تأكد من إكمال البيانات ثم اعد المحاولة");
            }
            return RedirectToAction("Index");
        }

        [Authorize("Permissions.DriverPriceDelete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var driverPriceById = await _driverPriceRepo.GetByIdAsync(id);
                if (driverPriceById == null)
                    return NotFound();
                _driverPriceRepo.Delete(driverPriceById);
                await _driverPriceRepo.SaveAllAsync();
                _toastNotification.AddSuccessToastMessage("تم الحذف بنجاح");

            }
            catch
            {
                _toastNotification.AddErrorToastMessage("لم يتم الحذف");
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAreas()
        {
            try
            {
                //  var areas = await _driverPriceRepo.GetAllAsync(g => g.IsDeleted == false, true);
                var areas = new List<DriverPrice>
                {
                    new DriverPrice { Id = 1, Region = "السعر 1", DeliveryPrice = 1000m },
                    new DriverPrice { Id = 1, Region = "السعر 2", DeliveryPrice = 2000m },
                    new DriverPrice { Id = 1, Region = "السعر 3", DeliveryPrice = 3000m },
                    new DriverPrice { Id = 1, Region = "السعر 4", DeliveryPrice = 4000m },
                    new DriverPrice { Id = 1, Region = "السعر 5", DeliveryPrice = 5000m },
                    new DriverPrice { Id = 3, Region = "مجانا", DeliveryPrice = 0m }
                };
                var areasData = areas.Select(a => new
                {
                    id = a.Id,
                    region = a.Region,
                    deliveryPrice = a.DeliveryPrice
                }).ToList();

                return Json(areasData);
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }
    }
}
