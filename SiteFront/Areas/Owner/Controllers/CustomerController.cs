using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Core.ViewModels.CustomerVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace SiteFront.Areas.Owner.Controllers
{
    [Area("Owner")]
    public class CustomerController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private readonly IRepository<Customer> _customerRepo;
        private readonly IRepository<SaleBill> _saleBillRepo;
        private readonly IRepository<DeliveryBill> _deliveryBillRepo;
        private readonly UserManager<User> _userManager;

        public CustomerController(IMapper mapper,
            IToastNotification toastNotification,
            IRepository<Customer> customerRepo,
            IRepository<SaleBill> saleBillRepo,
            IRepository<DeliveryBill> deliveryBillRepo,
            UserManager<User> userManager)
        {
            _mapper = mapper;
            _toastNotification = toastNotification;
            _customerRepo = customerRepo;
            _saleBillRepo = saleBillRepo;
            _deliveryBillRepo = deliveryBillRepo;
            _userManager = userManager;
        }

        [Authorize("Permissions.CustomerIndex")]
        public async Task<IActionResult> Index()
        {
            var customrs = await _customerRepo.GetAllAsync(g => g.IsDeleted == false, true);
            var customerGetVM = _mapper.Map<List<CustomerGetVM>>(customrs);
            var customerMainVM = new CustomerMainVM
            {
                CustomerGetVM = customerGetVM
            };
            return View(customerMainVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("Permissions.CustomerCreate")]
        public async Task<IActionResult> Create(CustomerMainVM model)
        {
            if (ModelState.IsValid)
            {
                var checkCustomer = await _customerRepo.SingleOrDefaultAsync(s => s.Phone == model.CustomerRegisterVM.Phone);
                if(checkCustomer != null)
                {
                    _toastNotification.AddInfoToastMessage("هذا العميل موجود بالفعل");
                    return RedirectToAction("Index");
                }
                var customerDb = _mapper.Map<Customer>(model.CustomerRegisterVM);
                customerDb.CreatedDate = DateTime.Now;
                customerDb.LastEditDate = DateTime.Now;
                customerDb.CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                customerDb.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                _customerRepo.Add(customerDb);
                await _customerRepo.SaveAllAsync();
                _toastNotification.AddSuccessToastMessage("تم الإضافة بنجاح");
            }
            else
            {
                _toastNotification.AddErrorToastMessage("تأكد من إكمال البيانات ثم اعد المحاولة");
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize("Permissions.CustomerEdit")]
        public async Task<IActionResult> GetData(int id)
        {
            var customerById = await _customerRepo.GetByIdAsync(id);
            if (customerById == null)
                return NotFound();
            return Json(new
            {
                id = customerById.Id,
                name = customerById.Name,
                phone = customerById.Phone,
                anotherPhone = customerById.AnotherPhone,
                address = customerById.Address,
                address2 = customerById.Address2,
                address3 = customerById.Address3,
                address4 = customerById.Address4,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("Permissions.CustomerEdit")]
        public async Task<IActionResult> Edit(CustomerMainVM model)
        {
            if (ModelState.IsValid)
            {
                var customerById = await _customerRepo.GetByIdAsync((int)model.CustomerRegisterVM.Id);
                if (customerById == null)
                    return NotFound();
                model.CustomerRegisterVM.CreatedDate = customerById.CreatedDate;
                model.CustomerRegisterVM.CreatedUser = customerById.CreatedUser;
                var customerDb = _mapper.Map(model.CustomerRegisterVM, customerById);
                customerDb.LastEditDate = DateTime.Now;
                customerDb.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                _customerRepo.Update(customerDb);
                await _customerRepo.SaveAllAsync();
                _toastNotification.AddSuccessToastMessage("تم التعديل بنجاح");
            }
            else
            {
                _toastNotification.AddErrorToastMessage("تأكد من إكمال البيانات ثم اعد المحاولة");
            }
            return RedirectToAction("Index");
        }

        [Authorize("Permissions.CustomerDelete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var customerById = await _customerRepo.GetByIdAsync(id);
                if (customerById == null)
                    return NotFound();
                var saleBills = await _saleBillRepo.GetAllAsync(s => s.CustomerId == id);
                var deliveryBills = await _deliveryBillRepo.GetAllAsync(d => d.CustomerId == id);
                if (saleBills.Count() == 0 && deliveryBills.Count() == 0)
                {
                    _customerRepo.Delete(customerById);
                    await _customerRepo.SaveAllAsync();
                    _toastNotification.AddSuccessToastMessage("تم الحذف بنجاح");
                }
                else
                {
                    _toastNotification.AddErrorToastMessage("لا يمكن حذف هذا العميل");
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
