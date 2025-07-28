using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Core.ViewModels.DriverVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace SiteFront.Areas.Owner.Controllers
{
    [Area("Owner")]
    public class DriverController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private readonly IRepository<Driver> _driverRepo;
        private readonly UserManager<User> _userManager;

        public DriverController(IMapper mapper,
            IToastNotification toastNotification,
            IRepository<Driver> driverRepo,
            UserManager<User> userManager)
        {
            _mapper = mapper;
            _toastNotification = toastNotification;
            _driverRepo = driverRepo;
            _userManager = userManager;
        }

        [Authorize("Permissions.DriverIndex")]
        public async Task<IActionResult> Index()
        {
            var drivers = await _driverRepo.GetAllAsync(g => g.IsDeleted == false, true);
            var driverGetVM = _mapper.Map<List<DriverGetVM>>(drivers);
            var driverMainVM = new DriverMainVM
            {
                DriverGetVM = driverGetVM
            };
            return View(driverMainVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("Permissions.DriverCreate")]
        public async Task<IActionResult> Create(DriverMainVM model)
        {
            if (ModelState.IsValid)
            {
                var driverDb = _mapper.Map<Driver>(model.DriverRegisterVM);
                driverDb.CreatedDate = DateTime.Now;
                driverDb.LastEditDate = DateTime.Now;
                driverDb.CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                driverDb.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                _driverRepo.Add(driverDb);
                await _driverRepo.SaveAllAsync();
                _toastNotification.AddSuccessToastMessage("تم الإضافة بنجاح");
            }
            else
            {
                _toastNotification.AddErrorToastMessage("تأكد من إكمال البيانات ثم اعد المحاولة");
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize("Permissions.DriverEdit")]
        public async Task<IActionResult> GetData(int id)
        {
            var driverById = await _driverRepo.GetByIdAsync(id);
            if (driverById == null)
                return NotFound();
            return Json(new { id = driverById.Id, name = driverById.Name, phone = driverById.Phone });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("Permissions.DriverEdit")]
        public async Task<IActionResult> Edit(DriverMainVM model)
        {
            if (ModelState.IsValid)
            {
                var driverById = await _driverRepo.GetByIdAsync((int)model.DriverRegisterVM.Id);
                if (driverById == null)
                    return NotFound();
                model.DriverRegisterVM.CreatedDate = driverById.CreatedDate;
                model.DriverRegisterVM.CreatedUser = driverById.CreatedUser;
                var driverDb = _mapper.Map(model.DriverRegisterVM, driverById);
                driverDb.LastEditDate = DateTime.Now;
                driverDb.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                _driverRepo.Update(driverDb);
                await _driverRepo.SaveAllAsync();
                _toastNotification.AddSuccessToastMessage("تم التعديل بنجاح");
            }
            else
            {
                _toastNotification.AddErrorToastMessage("تأكد من إكمال البيانات ثم اعد المحاولة");
            }
            return RedirectToAction("Index");
        }

        [Authorize("Permissions.DriverDelete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var driverById = await _driverRepo.GetByIdAsync(id);
                if (driverById == null)
                    return NotFound();
                _driverRepo.Delete(driverById);
                await _driverRepo.SaveAllAsync();
                _toastNotification.AddSuccessToastMessage("تم الحذف بنجاح");

            }
            catch
            {
                _toastNotification.AddErrorToastMessage("لم يتم الحذف");
            }
            return RedirectToAction("Index");
        }
    }
}
