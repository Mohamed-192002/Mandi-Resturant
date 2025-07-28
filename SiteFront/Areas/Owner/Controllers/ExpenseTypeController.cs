using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Core.ViewModels.ExpenseTypeVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace SiteFront.Areas.Owner.Controllers
{
    [Area("Owner")]
    public class ExpenseTypeController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private readonly IRepository<ExpenseType> _expenseTypeRepo;
        private readonly IRepository<Expense> _expenseRepo;
        private readonly UserManager<User> _userManager;

        public ExpenseTypeController(IMapper mapper,
            IToastNotification toastNotification,
            IRepository<ExpenseType> expenseTypeRepo,
            IRepository<Expense> expenseRepo,
            UserManager<User> userManager)
        {
            _mapper = mapper;
            _toastNotification = toastNotification;
            _expenseTypeRepo = expenseTypeRepo;
            _expenseRepo = expenseRepo;
            _userManager = userManager;
        }

        [Authorize("Permissions.ExpenseTypeIndex")]
        public async Task<IActionResult> Index()
        {
            var expenseTypes = await _expenseTypeRepo.GetAllAsync(c => c.IsDeleted == false, true);
            var expenseTypeGetVM = _mapper.Map<List<ExpenseTypeGetVM>>(expenseTypes);
            var expenseTypeModelVM = new ExpenseTypeMainVM
            {
                ExpenseTypeGetVM = expenseTypeGetVM
            };
            return View(expenseTypeModelVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("Permissions.ExpenseTypeCreate")]
        public async Task<IActionResult> Create(ExpenseTypeMainVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var expenseTypeDb = _mapper.Map<ExpenseType>(model.ExpenseTypeRegisterVM);
                    expenseTypeDb.CreatedDate = DateTime.Now;
                    expenseTypeDb.LastEditDate = DateTime.Now;
                    expenseTypeDb.CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                    expenseTypeDb.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                    _expenseTypeRepo.Add(expenseTypeDb);
                    await _expenseTypeRepo.SaveAllAsync();
                    _toastNotification.AddSuccessToastMessage("تم الإضافة بنجاح");
                }
                catch
                {
                    _toastNotification.AddErrorToastMessage("حدث خطأ برجاء إعادة تسجيل الدخول");
                }
            }
            else
            {
                _toastNotification.AddErrorToastMessage("تأكد من صحة البيانات ثم أعد المحاولة");
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize("Permissions.ExpenseTypeEdit")]
        public async Task<IActionResult> GetData(int id)
        {
            var expenseTypeById = await _expenseTypeRepo.GetByIdAsync(id);
            if (expenseTypeById == null)
                return NotFound();
            return Json(new { id = expenseTypeById.Id, name = expenseTypeById.Name });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("Permissions.ExpenseTypeEdit")]
        public async Task<IActionResult> Edit(ExpenseTypeMainVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var expenseTypeById = await _expenseTypeRepo.GetByIdAsync((int)model.ExpenseTypeRegisterVM.Id);
                    if (expenseTypeById == null)
                        return NotFound();
                    model.ExpenseTypeRegisterVM.CreatedUser = expenseTypeById.CreatedUser;
                    model.ExpenseTypeRegisterVM.CreatedDate = expenseTypeById.CreatedDate;
                    model.ExpenseTypeRegisterVM.LastEditDate = DateTime.Now;
                    model.ExpenseTypeRegisterVM.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                    var expenseTypeDb = _mapper.Map(model.ExpenseTypeRegisterVM, expenseTypeById);
                    _expenseTypeRepo.Update(expenseTypeDb);
                    await _expenseTypeRepo.SaveAllAsync();
                    _toastNotification.AddSuccessToastMessage("تم التعديل بنجاح");
                }
                catch
                {
                    _toastNotification.AddErrorToastMessage("حدث خطأ برجاء إعادة تسجيل الدخول");
                }
            }
            else
            {
                _toastNotification.AddErrorToastMessage("تأكد من صحة البيانات ثم أعد المحاولة");
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize("Permissions.ExpenseTypeDelete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var expenseTypeById = await _expenseTypeRepo.GetByIdAsync(id);
                if (expenseTypeById == null)
                    return NotFound();
                var expenses = await _expenseRepo.GetAllAsync(e => e.ExpenseTypeId == id);
                if (expenses.Count() == 0)
                {
                    _expenseTypeRepo.Delete(expenseTypeById);
                    await _expenseTypeRepo.SaveAllAsync();
                    _toastNotification.AddSuccessToastMessage("تم الحذف بنجاح");
                }
                else
                {
                    _toastNotification.AddErrorToastMessage("لا يمكن حذف هذا النوع");
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
