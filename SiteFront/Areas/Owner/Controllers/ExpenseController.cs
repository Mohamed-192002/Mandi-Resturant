using AutoMapper;
using Core.Common;
using Core.Entities;
using Core.Interfaces;
using Core.ViewModels.ExpenseVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace SiteFront.Areas.Owner.Controllers
{
    [Area("Owner")]
    public class ExpenseController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private readonly IRepository<Expense> _expenseRepo;
        private readonly IRepository<ExpenseType> _expenseTypeRepo;
        private readonly UserManager<User> _userManager;

        public ExpenseController(IMapper mapper,
            IToastNotification toastNotification,
            IRepository<Expense> expenseRepo,
            IRepository<ExpenseType> expenseTypeRepo,
            UserManager<User> userManager
            )
        {
            _mapper = mapper;
            _toastNotification = toastNotification;
            _expenseRepo = expenseRepo;
            _expenseTypeRepo = expenseTypeRepo;
            _userManager = userManager;
        }

        [Authorize("Permissions.ExpenseIndex")]
        public async Task<IActionResult> Index()
        {
            var expenses = await _expenseRepo.GetAllAsync(c => c.IsDeleted == false, true, c => c.ExpenseType);
            var expenseGetVM = _mapper.Map<List<ExpenseGetVM>>(expenses);
            var expenseTypes = await _expenseTypeRepo.GetAllAsync(c => c.IsDeleted == false, true);
            var expenseRegisterVM = new ExpenseRegisterVM
            {
                ExpenseTypes = _mapper.Map<List<CommonDrop>>(expenseTypes)
            };
            var expenseModelVM = new ExpenseMainVM
            {
                ExpenseRegisterVM = expenseRegisterVM,
                ExpenseGetVM = expenseGetVM,
            };
            return View(expenseModelVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("Permissions.ExpenseCreate")]
        public async Task<IActionResult> Create(ExpenseMainVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var expenseDb = _mapper.Map<Expense>(model.ExpenseRegisterVM);
                    expenseDb.CreatedDate = DateTime.Now;
                    expenseDb.LastEditDate = DateTime.Now;
                    expenseDb.CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                    expenseDb.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                    _expenseRepo.Add(expenseDb);
                    await _expenseRepo.SaveAllAsync();
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

        [Authorize("Permissions.ExpenseEdit")]
        public async Task<IActionResult> GetData(int id)
        {
            var expenseById = await _expenseRepo.GetByIdAsync(id);
            if (expenseById == null)
                return NotFound();
            var allExpenseTypes = await _expenseTypeRepo.GetAllAsync(c => c.IsDeleted == false, true);
            return Json(new
            {
                id = expenseById.Id,
                date = expenseById.Date,
                expenseTypes = allExpenseTypes.Select(c => new { typeId = c.Id, typeName = c.Name }),
                expenseTypeId = expenseById.ExpenseTypeId,
                payment = expenseById.Payment,
                notes = expenseById.Notes
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("Permissions.ExpenseEdit")]
        public async Task<IActionResult> Edit(ExpenseMainVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var expenseById = await _expenseRepo.GetByIdAsync((int)model.ExpenseRegisterVM.Id);
                    if (expenseById == null)
                        return NotFound();
                    model.ExpenseRegisterVM.CreatedUser = expenseById.CreatedUser;
                    model.ExpenseRegisterVM.CreatedDate = expenseById.CreatedDate;
                    model.ExpenseRegisterVM.LastEditDate = DateTime.Now;
                    model.ExpenseRegisterVM.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                    var expenseDb = _mapper.Map(model.ExpenseRegisterVM, expenseById);
                    _expenseRepo.Update(expenseDb);
                    await _expenseRepo.SaveAllAsync();
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

        [Authorize("Permissions.ExpenseDelete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var expenseById = await _expenseRepo.GetByIdAsync(id);
                if (expenseById == null)
                    return NotFound();
                _expenseRepo.Delete(expenseById);
                await _expenseRepo.SaveAllAsync();
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
