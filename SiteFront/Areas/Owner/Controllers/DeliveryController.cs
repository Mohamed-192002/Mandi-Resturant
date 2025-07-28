using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Core.ViewModels.DeliveryVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace SiteFront.Areas.Owner.Controllers
{
    [Area("Owner")]
    public class DeliveryController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private readonly IRepository<Delivery> _deliveryRepo;
        private readonly IRepository<SaleBill> _saleBillRepo;
        private readonly IRepository<DeliveryBill> _deliveryBillRepo;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _hosting;

        public DeliveryController(IMapper mapper,
            IToastNotification toastNotification,
            IRepository<Delivery> deliveryRepo,
            IRepository<SaleBill> saleBillRepo,
            IRepository<DeliveryBill> deliveryBillRepo,
            UserManager<User> userManager,
            IWebHostEnvironment hosting)
        {
            _mapper = mapper;
            _toastNotification = toastNotification;
            _deliveryRepo = deliveryRepo;
            _saleBillRepo = saleBillRepo;
            _deliveryBillRepo = deliveryBillRepo;
            _userManager = userManager;
            _hosting = hosting;
        }

        [Authorize("Permissions.DeliveryIndex")]
        public async Task<IActionResult> Index()
        {
            var deliveries = await _deliveryRepo.GetAllAsync(g => g.IsDeleted == false, true);
            var deliveryGetVM = _mapper.Map<List<DeliveryGetVM>>(deliveries);
            var deliveryMainVM = new DeliveryMainVM
            {
                DeliveryGetVM = deliveryGetVM
            };
            return View(deliveryMainVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("Permissions.DeliveryCreate")]
        public async Task<IActionResult> Create(DeliveryMainVM model)
        {
            if (ModelState.IsValid)
            {
                var deliveryDb = _mapper.Map<Delivery>(model.DeliveryRegisterVM);
                deliveryDb.CreatedDate = DateTime.Now;
                deliveryDb.LastEditDate = DateTime.Now;
                deliveryDb.CreatedUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                deliveryDb.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                if (model.DeliveryRegisterVM.ImageFile != null)
                {
                    var upload = Path.Combine(_hosting.WebRootPath, "SystemImages/Deliveries/");
                    if (!Directory.Exists(upload))
                        Directory.CreateDirectory(upload);
                    var fileName = Guid.NewGuid().ToString() + '.' + model.DeliveryRegisterVM.ImageFile.FileName.Split('.')[1].ToString();
                    deliveryDb.ImageUrl = fileName;
                    var fullName = Path.Combine(upload, fileName);
                    model.DeliveryRegisterVM.ImageFile.CopyTo(new FileStream(fullName, FileMode.Create));
                }
                _deliveryRepo.Add(deliveryDb);
                await _deliveryRepo.SaveAllAsync();
                _toastNotification.AddSuccessToastMessage("تم الإضافة بنجاح");
            }
            else
            {
                _toastNotification.AddErrorToastMessage("تأكد من إكمال البيانات ثم اعد المحاولة");
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize("Permissions.DeliveryEdit")]
        public async Task<IActionResult> GetData(int id)
        {
            var deliveryById = await _deliveryRepo.GetByIdAsync(id);
            if (deliveryById == null)
                return NotFound();
            return Json(new
            {
                id = deliveryById.Id,
                name = deliveryById.Name,
                color = deliveryById.Color
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("Permissions.DeliveryEdit")]
        public async Task<IActionResult> Edit(DeliveryMainVM model)
        {
            if (ModelState.IsValid)
            {
                var deliveryById = await _deliveryRepo.GetByIdAsync((int)model.DeliveryRegisterVM.Id);
                if (deliveryById == null)
                    return NotFound();
                model.DeliveryRegisterVM.CreatedDate = deliveryById.CreatedDate;
                model.DeliveryRegisterVM.CreatedUser = deliveryById.CreatedUser;
                if (model.DeliveryRegisterVM.ImageFile != null)
                {
                    var upload = Path.Combine(_hosting.WebRootPath, "SystemImages/Deliveries/");
                    if (!Directory.Exists(upload))
                        Directory.CreateDirectory(upload);
                    var fileName = Guid.NewGuid().ToString() + '.' + model.DeliveryRegisterVM.ImageFile.FileName.Split('.')[1].ToString();
                    model.DeliveryRegisterVM.ImageUrl = fileName;
                    var fullName = Path.Combine(upload, fileName);
                    model.DeliveryRegisterVM.ImageFile.CopyTo(new FileStream(fullName, FileMode.Create));
                }
                else
                {
                    model.DeliveryRegisterVM.ImageUrl = deliveryById.ImageUrl;
                }
                var deliveryDb = _mapper.Map(model.DeliveryRegisterVM, deliveryById);
                deliveryDb.LastEditDate = DateTime.Now;
                deliveryDb.LastEditUser = _userManager.GetUserAsync(HttpContext.User).Result.Id;
                _deliveryRepo.Update(deliveryDb);
                await _deliveryRepo.SaveAllAsync();
                _toastNotification.AddSuccessToastMessage("تم التعديل بنجاح");
            }
            else
            {
                _toastNotification.AddErrorToastMessage("تأكد من إكمال البيانات ثم اعد المحاولة");
            }
            return RedirectToAction("Index");
        }

        [Authorize("Permissions.DeliveryDelete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deliveryById = await _deliveryRepo.GetByIdAsync(id);
                if (deliveryById == null)
                    return NotFound();
                var saleBills = await _saleBillRepo.GetAllAsync(s => s.DeliveryId == id);
                var deliveryBills = await _deliveryBillRepo.GetAllAsync(d => d.DeliveryId == id);
                if (saleBills.Count() == 0 && deliveryBills.Count() == 0)
                {
                    _deliveryRepo.Delete(deliveryById);
                    await _deliveryRepo.SaveAllAsync();
                    _toastNotification.AddSuccessToastMessage("تم الحذف بنجاح");
                }
                else
                {
                    _toastNotification.AddErrorToastMessage("لا يمكن حذف هذه الشركة");
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
