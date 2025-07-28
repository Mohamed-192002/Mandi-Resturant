using AutoMapper;
using Core.Common;
using Core.Common.Enums;
using Core.Entities;
using Core.Interfaces;
using Core.ViewModels.ChickenFillingVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace SiteFront.Areas.Owner.Controllers
{
    [Area("Owner")]
    public class ChickenFillingController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private readonly IRepository<Hole> _holeRepo;
        private readonly IRepository<ChickenFilling> _chickenFillingRepo;
        private readonly IRepository<ChickenHoleMovement> _chickenHoleMovementRepo;
        private readonly UserManager<User> _userManager;

        public ChickenFillingController(IMapper mapper,
            IToastNotification toastNotification,
            IRepository<Hole> holeRepo,
            IRepository<ChickenFilling> chickenFillingRepo,
            IRepository<ChickenHoleMovement> chickenHoleMovementRepo,
            UserManager<User> userManager)
        {
            _mapper = mapper;
            _toastNotification = toastNotification;
            _holeRepo = holeRepo;
            _chickenFillingRepo = chickenFillingRepo;
            _chickenHoleMovementRepo = chickenHoleMovementRepo;
            _userManager = userManager;
        }

        [Authorize("Permissions.ChickenFillingIndex")]
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var chickenFillings = await _chickenFillingRepo.GetAllAsync(c => c.IsDeleted == false && c.Date.Date == today.Date, true, c => c.Hole);
            var chickenFillingGetVM = _mapper.Map<List<ChickenFillingGetVM>>(chickenFillings);
            var holes = await _holeRepo.GetAllAsync(c => c.IsDeleted == false && c.HoleType == HoleType.دجاج, true);
            var chickenFillingRegisterVM = new ChickenFillingRegisterVM
            {
                Holes = _mapper.Map<List<CommonDrop>>(holes),
            };
            var chickenFillingMainVM = new ChickenFillingMainVM
            {
                ChickenFillingRegisterVM = chickenFillingRegisterVM,
                ChickenFillingGetVM = chickenFillingGetVM
            };
            return View(chickenFillingMainVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("Permissions.ChickenFillingCreate")]
        public async Task<IActionResult> Create(ChickenFillingMainVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var chickenFillingDb = _mapper.Map<ChickenFilling>(model.ChickenFillingRegisterVM);
                    chickenFillingDb.CreatedDate = DateTime.Now;
                    chickenFillingDb.LastEditDate = DateTime.Now;
                    chickenFillingDb.CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                    chickenFillingDb.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                    _chickenFillingRepo.Add(chickenFillingDb);
                    await _chickenFillingRepo.SaveAllAsync();

                    //ChickenHoleMovement
                    var chickenHoleMovement = new ChickenHoleMovement
                    {
                        HoleId = model.ChickenFillingRegisterVM.HoleId,
                        Date = model.ChickenFillingRegisterVM.Date,
                        AmountIn = model.ChickenFillingRegisterVM.Amount,
                        AmountOut = 0,
                        HoleMovementType = HoleMovementType.Fill,
                        HoleMovementTypeId = chickenFillingDb.Id,
                        CreatedDate = DateTime.Now,
                        LastEditDate = DateTime.Now,
                        CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id,
                        LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id
                    };
                    _chickenHoleMovementRepo.Add(chickenHoleMovement);
                    await _chickenHoleMovementRepo.SaveAllAsync();
                    _toastNotification.AddSuccessToastMessage("تم الإضافة بنجاح");
                }
                catch
                {
                    _toastNotification.AddErrorToastMessage("حدث خطأ برجاء إعادة المحاولة لاحقا");
                }
            }
            else
            {
                _toastNotification.AddErrorToastMessage("تأكد من صحة البيانات ثم أعد المحاولة");
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize("Permissions.ChickenFillingEdit")]
        public async Task<IActionResult> GetData(int id)
        {
            var chickenFillingById = await _chickenFillingRepo.GetByIdAsync(id);
            if (chickenFillingById == null)
                return NotFound();
            var allHoles = await _holeRepo.GetAllAsync(c => c.IsDeleted == false && c.HoleType == HoleType.دجاج, true);
            return Json(new
            {
                id = chickenFillingById.Id,
                date = chickenFillingById.Date,
                holes = allHoles.Select(c => new { holeId = c.Id, holeName = c.Name }),
                holeId = chickenFillingById.HoleId,
                amount = chickenFillingById.Amount,
                endTime = chickenFillingById.EndTime
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("Permissions.ChickenFillingEdit")]
        public async Task<IActionResult> Edit(ChickenFillingMainVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var chickenFillingById = await _chickenFillingRepo.GetByIdAsync((int)model.ChickenFillingRegisterVM.Id);
                    if (chickenFillingById == null)
                        return NotFound();
                    model.ChickenFillingRegisterVM.CreatedUser = chickenFillingById.CreatedUser;
                    model.ChickenFillingRegisterVM.CreatedDate = chickenFillingById.CreatedDate;
                    model.ChickenFillingRegisterVM.LastEditDate = DateTime.Now;
                    model.ChickenFillingRegisterVM.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                    var chickenFillingDb = _mapper.Map(model.ChickenFillingRegisterVM, chickenFillingById);
                    _chickenFillingRepo.Update(chickenFillingDb);

                    //ChickenHoleMovement
                    var chickenHoleMovementById = await _chickenHoleMovementRepo.SingleOrDefaultAsync(c => c.HoleMovementType == HoleMovementType.Fill && c.HoleMovementTypeId == chickenFillingById.Id);
                    if (chickenHoleMovementById != null)
                    {
                        chickenHoleMovementById.LastEditDate = DateTime.Now;
                        chickenHoleMovementById.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                        chickenHoleMovementById.HoleId = model.ChickenFillingRegisterVM.HoleId;
                        chickenHoleMovementById.AmountIn = model.ChickenFillingRegisterVM.Amount;
                        _chickenHoleMovementRepo.Update(chickenHoleMovementById);
                    }
                    else
                    {
                        var chickenHoleMovement = new ChickenHoleMovement
                        {
                            HoleId = model.ChickenFillingRegisterVM.HoleId,
                            Date = model.ChickenFillingRegisterVM.Date,
                            AmountIn = model.ChickenFillingRegisterVM.Amount,
                            AmountOut = 0,
                            HoleMovementType = HoleMovementType.Fill,
                            HoleMovementTypeId = chickenFillingDb.Id,
                            CreatedDate = DateTime.Now,
                            LastEditDate = DateTime.Now,
                            CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id,
                            LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id
                        };
                        _chickenHoleMovementRepo.Add(chickenHoleMovement);
                    }

                    await _chickenFillingRepo.SaveAllAsync();
                    _toastNotification.AddSuccessToastMessage("تم التعديل بنجاح");
                }
                catch
                {
                    _toastNotification.AddErrorToastMessage("حدث خطأ برجاء إعادة المحاولة لاحقا");
                }
            }
            else
            {
                _toastNotification.AddErrorToastMessage("تأكد من صحة البيانات ثم أعد المحاولة");
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize("Permissions.ChickenFillingDelete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var chickenFillingById = await _chickenFillingRepo.GetByIdAsync(id);
                if (chickenFillingById == null)
                    return NotFound();
                var chickenHoleMovementById = await _chickenHoleMovementRepo.SingleOrDefaultAsync(c => c.HoleMovementType == HoleMovementType.Fill && c.HoleMovementTypeId == chickenFillingById.Id);
                if (chickenHoleMovementById != null)
                {
                    _chickenHoleMovementRepo.Delete(chickenHoleMovementById);
                }
                _chickenFillingRepo.Delete(chickenFillingById);
                await _chickenFillingRepo.SaveAllAsync();
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
