using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Core.ViewModels.HoleVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace SiteFront.Areas.Owner.Controllers
{
    [Area("Owner")]
    public class HoleController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private readonly IRepository<Hole> _holeRepo;
        private readonly IRepository<ChickenFilling> _chickenFillingRepo;
        private readonly IRepository<MeatFilling> _meatFillingRepo;
        private readonly UserManager<User> _userManager;

        public HoleController(IMapper mapper,
            IToastNotification toastNotification,
            IRepository<Hole> holeRepo,
            IRepository<ChickenFilling> chickenFillingRepo,
            IRepository<MeatFilling> meatFillingRepo,
            UserManager<User> userManager)
        {
            _mapper = mapper;
            _toastNotification = toastNotification;
            _holeRepo = holeRepo;
            _chickenFillingRepo = chickenFillingRepo;
            _meatFillingRepo = meatFillingRepo;
            _userManager = userManager;
        }

        [Authorize("Permissions.HoleIndex")]
        public async Task<IActionResult> Index()
        {
            var holes = await _holeRepo.GetAllAsync(g => g.IsDeleted == false, true);
            var holeGetVM = _mapper.Map<List<HoleGetVM>>(holes);
            var holeMainVM = new HoleMainVM
            {
                HoleGetVM = holeGetVM
            };
            return View(holeMainVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("Permissions.HoleCreate")]
        public async Task<IActionResult> Create(HoleMainVM model)
        {
            if (ModelState.IsValid)
            {
                var holeDb = _mapper.Map<Hole>(model.HoleRegisterVM);
                holeDb.CreatedDate = DateTime.Now;
                holeDb.LastEditDate = DateTime.Now;
                holeDb.CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                holeDb.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                _holeRepo.Add(holeDb);
                await _holeRepo.SaveAllAsync();
                _toastNotification.AddSuccessToastMessage("تم الإضافة بنجاح");
            }
            else
            {
                _toastNotification.AddErrorToastMessage("تأكد من إكمال البيانات ثم اعد المحاولة");
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize("Permissions.HoleEdit")]
        public async Task<IActionResult> GetData(int id)
        {
            var holeById = await _holeRepo.GetByIdAsync(id);
            if (holeById == null)
                return NotFound();
            return Json(new { id = holeById.Id, name = holeById.Name, holeType = holeById.HoleType });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("Permissions.HoleEdit")]
        public async Task<IActionResult> Edit(HoleMainVM model)
        {
            if (ModelState.IsValid)
            {
                var holeById = await _holeRepo.GetByIdAsync((int)model.HoleRegisterVM.Id);
                if (holeById == null)
                    return NotFound();
                model.HoleRegisterVM.CreatedDate = holeById.CreatedDate;
                model.HoleRegisterVM.CreatedUser = holeById.CreatedUser;
                var holeDb = _mapper.Map(model.HoleRegisterVM, holeById);
                holeDb.LastEditDate = DateTime.Now;
                holeDb.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                _holeRepo.Update(holeDb);
                await _holeRepo.SaveAllAsync();
                _toastNotification.AddSuccessToastMessage("تم التعديل بنجاح");
            }
            else
            {
                _toastNotification.AddErrorToastMessage("تأكد من إكمال البيانات ثم اعد المحاولة");
            }
            return RedirectToAction("Index");
        }

        [Authorize("Permissions.HoleDelete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var holeById = await _holeRepo.GetByIdAsync(id);
                if (holeById == null)
                    return NotFound();
                var chickenFillings = await _chickenFillingRepo.GetAllAsync(s => s.HoleId == id);
                var meatFillings = await _meatFillingRepo.GetAllAsync(s => s.HoleId == id);
                if (chickenFillings.Count() == 0 && meatFillings.Count() == 0)
                {
                    _holeRepo.Delete(holeById);
                    await _holeRepo.SaveAllAsync();
                    _toastNotification.AddSuccessToastMessage("تم الحذف بنجاح");
                }
                else
                {
                    _toastNotification.AddErrorToastMessage("لا يمكن حذف هذه الحفرة");
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
