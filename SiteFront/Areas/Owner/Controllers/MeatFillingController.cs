using AutoMapper;
using Core.Common;
using Core.Common.Enums;
using Core.Entities;
using Core.Interfaces;
using Core.ViewModels.MeatFillingVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace SiteFront.Areas.Owner.Controllers
{
    [Area("Owner")]
    public class MeatFillingController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private readonly IRepository<MeatFilling> _meatFillingRepo;
        private readonly IRepository<MeatHoleMovement> _meatHoleMovementRepo;
        private readonly IRepository<Hole> _holeRepo;
        private readonly UserManager<User> _userManager;

        public MeatFillingController(IMapper mapper,
            IToastNotification toastNotification,
            IRepository<MeatFilling> meatFillingRepo,
            IRepository<MeatHoleMovement> meatHoleMovementRepo,
            IRepository<Hole> holeRepo,
            UserManager<User> userManager)
        {
            _mapper = mapper;
            _toastNotification = toastNotification;
            _meatFillingRepo = meatFillingRepo;
            _meatHoleMovementRepo = meatHoleMovementRepo;
            _holeRepo = holeRepo;
            _userManager = userManager;
        }

        [Authorize("Permissions.MeatFillingIndex")]
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var meatFillings = await _meatFillingRepo.GetAllAsync(c => c.IsDeleted == false && c.Date.Date == today.Date, true, c => c.Hole);
            var meatFillingGetVM = _mapper.Map<List<MeatFillingGetVM>>(meatFillings);
            var holes = await _holeRepo.GetAllAsync(c => c.IsDeleted == false && c.HoleType == HoleType.لحم, true);
            var meatFillingRegisterVM = new MeatFillingRegisterVM
            {
                Holes = _mapper.Map<List<CommonDrop>>(holes),
            };
            var meatFillingMainVM = new MeatFillingMainVM
            {
                MeatFillingRegisterVM = meatFillingRegisterVM,
                MeatFillingGetVM = meatFillingGetVM
            };
            return View(meatFillingMainVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("Permissions.MeatFillingCreate")]
        public async Task<IActionResult> Create(MeatFillingMainVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var meatFillingDb = _mapper.Map<MeatFilling>(model.MeatFillingRegisterVM);
                    meatFillingDb.CreatedDate = DateTime.Now;
                    meatFillingDb.LastEditDate = DateTime.Now;
                    meatFillingDb.CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                    meatFillingDb.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                    _meatFillingRepo.Add(meatFillingDb);
                    await _meatFillingRepo.SaveAllAsync();

                    //MeatHoleMovement
                    var meatHoleMovement = new MeatHoleMovement
                    {
                        HoleId = model.MeatFillingRegisterVM.HoleId,
                        Date = model.MeatFillingRegisterVM.Date,
                        NafrAmountIn = model.MeatFillingRegisterVM.Nafr,
                        NafrAmountOut = 0,
                        HalfNafrAmountIn = model.MeatFillingRegisterVM.HalfNafr,
                        HalfNafrAmountOut = 0,
                        HoleMovementType = HoleMovementType.Fill,
                        HoleMovementTypeId = meatFillingDb.Id,
                        CreatedDate = DateTime.Now,
                        LastEditDate = DateTime.Now,
                        CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id,
                        LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id
                    };
                    _meatHoleMovementRepo.Add(meatHoleMovement);
                    await _meatHoleMovementRepo.SaveAllAsync();
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

        [Authorize("Permissions.MeatFillingEdit")]
        public async Task<IActionResult> GetData(int id)
        {
            var meatFillingById = await _meatFillingRepo.GetByIdAsync(id);
            if (meatFillingById == null)
                return NotFound();
            var allHoles = await _holeRepo.GetAllAsync(c => c.IsDeleted == false && c.HoleType == HoleType.لحم, true);
            return Json(new
            {
                id = meatFillingById.Id,
                date = meatFillingById.Date,
                holes = allHoles.Select(c => new { holeId = c.Id, holeName = c.Name }),
                holeId = meatFillingById.HoleId,
                nafr = meatFillingById.Nafr,
                halfNafr = meatFillingById.HalfNafr,
                endTime = meatFillingById.EndTime
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("Permissions.MeatFillingEdit")]
        public async Task<IActionResult> Edit(MeatFillingMainVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var meatFillingById = await _meatFillingRepo.GetByIdAsync((int)model.MeatFillingRegisterVM.Id);
                    if (meatFillingById == null)
                        return NotFound();
                    model.MeatFillingRegisterVM.CreatedUser = meatFillingById.CreatedUser;
                    model.MeatFillingRegisterVM.CreatedDate = meatFillingById.CreatedDate;
                    model.MeatFillingRegisterVM.LastEditDate = DateTime.Now;
                    model.MeatFillingRegisterVM.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                    var chickenFillingDb = _mapper.Map(model.MeatFillingRegisterVM, meatFillingById);
                    _meatFillingRepo.Update(chickenFillingDb);

                    //MeatHoleMovement
                    var meatHoleMovementById = await _meatHoleMovementRepo.SingleOrDefaultAsync(c => c.HoleMovementType == HoleMovementType.Fill && c.HoleMovementTypeId == meatFillingById.Id);
                    if (meatHoleMovementById != null)
                    {
                        meatHoleMovementById.LastEditDate = DateTime.Now;
                        meatHoleMovementById.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                        meatHoleMovementById.HoleId = model.MeatFillingRegisterVM.HoleId;
                        meatHoleMovementById.NafrAmountIn = model.MeatFillingRegisterVM.Nafr;
                        meatHoleMovementById.HalfNafrAmountIn = model.MeatFillingRegisterVM.HalfNafr;
                        _meatHoleMovementRepo.Update(meatHoleMovementById);
                    }
                    else
                    {
                        var meatHoleMovement = new MeatHoleMovement
                        {
                            HoleId = model.MeatFillingRegisterVM.HoleId,
                            Date = model.MeatFillingRegisterVM.Date,
                            NafrAmountIn = model.MeatFillingRegisterVM.Nafr,
                            NafrAmountOut = 0,
                            HalfNafrAmountIn = model.MeatFillingRegisterVM.HalfNafr,
                            HalfNafrAmountOut = 0,
                            HoleMovementType = HoleMovementType.Fill,
                            HoleMovementTypeId = meatFillingById.Id,
                            CreatedDate = DateTime.Now,
                            LastEditDate = DateTime.Now,
                            CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id,
                            LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id
                        };
                        _meatHoleMovementRepo.Add(meatHoleMovement);
                    }

                    await _meatFillingRepo.SaveAllAsync();
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

        [Authorize("Permissions.MeatFillingDelete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var meatFillingById = await _meatFillingRepo.GetByIdAsync(id);
                if (meatFillingById == null)
                    return NotFound();
                var meatHoleMovementById = await _meatHoleMovementRepo.SingleOrDefaultAsync(c => c.HoleMovementType == HoleMovementType.Fill && c.HoleMovementTypeId == meatFillingById.Id);
                if (meatHoleMovementById != null)
                {
                    _meatHoleMovementRepo.Delete(meatHoleMovementById);
                }
                _meatFillingRepo.Delete(meatFillingById);
                await _meatFillingRepo.SaveAllAsync();
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
