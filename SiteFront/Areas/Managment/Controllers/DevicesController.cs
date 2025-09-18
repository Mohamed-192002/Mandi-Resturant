using AutoMapper;
using Core.Common;
using Core.Dtos.UserDto;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace SiteFront.Areas.Managment.Controllers
{
    [Area("Managment")]
    public class DevicesController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IRepository<User> _UserRepo;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IRepository<UserRole> _UserRoleRepo;
        private readonly IRepository<DeviceRegistration> _deviceRegistrationRepo;
        private readonly IRepository<PrinterRegistration> _printerRegistrationRepo;

        public DevicesController(IMapper mapper, IToastNotification toastNotification, SignInManager<User> signInManager, UserManager<User> userManager, RoleManager<Role> roleManager, IRepository<User> userRepo, ILogger<RegisterModel> logger, IRepository<UserRole> userRoleRepo, IRepository<DeviceRegistration> deviceRegistrationRepo, IRepository<PrinterRegistration> printerRegistrationRepo)
        {
            _mapper = mapper;
            _toastNotification = toastNotification;
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _UserRepo = userRepo;
            _logger = logger;
            _UserRoleRepo = userRoleRepo;
            _deviceRegistrationRepo = deviceRegistrationRepo;
            _printerRegistrationRepo = printerRegistrationRepo;
        }
        public IActionResult Index()
        {
            return View();
        }
        #region DeviceRegistration
        [Authorize("Permissions.GetDeviceRegistrations")]
        [HttpGet]
        public async Task<IActionResult> GetDeviceRegistrations()
        {
            var devices = await _deviceRegistrationRepo.GetAllAsync(d => !d.IsDeleted);
            var users = await _userManager.Users
                .Where(u => !u.IsDeleted && u.UserRole.Any(x => x.Role.Name == "Cashier"))
                .ToListAsync();

            var deviceRegistrationModelDto = new DeviceRegistrationModelDto
            {
                DeviceRegistrationGetDtos = devices,
                DeviceRegistrationRegisterDto = new DeviceRegistrationRegisterDto()
                {
                    Users = _mapper.Map<List<CommonUserDrop>>(users),
                }
            };
            return View(deviceRegistrationModelDto);
        }
        [Authorize("Permissions.DeleteDeviceRegistration")]
        public async Task<IActionResult> DeleteDeviceRegistration(int id)
        {
            try
            {
                var device = await _deviceRegistrationRepo.GetByIdAsync(id);
                device.IsDeleted = true;
                _deviceRegistrationRepo.Update(device);
                await _deviceRegistrationRepo.SaveAllAsync();
                _toastNotification.AddSuccessToastMessage("تم الحذف بنجاح");
            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("لا يمكن حذف هذا الهاتف");
            }
            return RedirectToAction(nameof(GetDeviceRegistrations));
        }
        [Authorize("Permissions.AddDeviceRegistration")]
        [HttpPost]
        public async Task<IActionResult> RegisterDevice(DeviceRegistrationRegisterDto model)
        {
            if (ModelState.IsValid)
            {
                var device = new DeviceRegistration
                {
                    UserId = model.UserId,
                    AccessCode = model.AccessCode,
                    IsDeleted = false
                };
                _deviceRegistrationRepo.Add(device);
                await _deviceRegistrationRepo.SaveAllAsync();
                _toastNotification.AddSuccessToastMessage("تم تسجيل الجهاز بنجاح");
                return RedirectToAction(nameof(GetDeviceRegistrations));
            }
            else
            {
                _toastNotification.AddErrorToastMessage("تأكد من صحة البيانات");
                var devices = await _deviceRegistrationRepo.GetAllAsync(d => !d.IsDeleted);
                var users = await _userManager.Users
                    .Where(u => !u.IsDeleted && u.UserRole.Any(x => x.Role.Name == "Cashier"))
                    .ToListAsync();

                var deviceRegistrationModelDto = new DeviceRegistrationModelDto
                {
                    DeviceRegistrationGetDtos = devices,
                    DeviceRegistrationRegisterDto = new DeviceRegistrationRegisterDto()
                    {
                        Users = _mapper.Map<List<CommonUserDrop>>(users),
                    }
                };
                // إعادة تحميل البيانات اللازمة لتعبئة الموديل
                var fullModel = new DeviceRegistrationModelDto
                {
                    DeviceRegistrationRegisterDto = model,
                    DeviceRegistrationGetDtos = devices // تأكد من تضمين الـ User
                };

                // املأ قائمة المستخدمين من جديد
                fullModel.DeviceRegistrationRegisterDto.Users = _mapper.Map<List<CommonUserDrop>>(users);

                return View("GetDeviceRegistrations", fullModel);
            }
        }

        #endregion

        #region Printers Registration
        [Authorize("Permissions.GetPrintersRegistrations")]
        [HttpGet]
        public async Task<IActionResult> GetPrintersRegistrations()
        {
            var printers = await _printerRegistrationRepo.GetAllAsync(d => !d.IsDeleted);
            var users = await _userManager.Users
                .Where(u => !u.IsDeleted && u.UserRole.Any(x => x.Role.Name == "Cashier"))
                .ToListAsync();
            var printerRegistrationModelDto = new PrinterRegistrationModelDto
            {
                PrinterRegistrationGetDtos = printers,
                PrinterRegistrationRegisterDto = new PrinterRegistrationRegisterDto()
                {
                    Users = _mapper.Map<List<CommonUserDrop>>(users),
                }
            };
            return View(printerRegistrationModelDto);
        }
        [Authorize("Permissions.DeletePrinterRegistration")]
        [HttpGet]
        public async Task<IActionResult> DeletePrinterRegistration(int id)
        {
            try
            {
                var printer = await _printerRegistrationRepo.GetByIdAsync(id);
                printer.IsDeleted = true;
                _printerRegistrationRepo.Update(printer);
                await _printerRegistrationRepo.SaveAllAsync();
                _toastNotification.AddSuccessToastMessage("تم الحذف بنجاح");
            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("لا يمكن حذف هذا الطابعة");
            }
            return RedirectToAction(nameof(GetPrintersRegistrations));
        }
        [Authorize("Permissions.AddPrinterRegistration")]
        [HttpPost]
        public async Task<IActionResult> RegisterPrinter(PrinterRegistrationRegisterDto model)
        {
            if (ModelState.IsValid)
            {
                var existingPrinter = await _printerRegistrationRepo.SingleOrDefaultAsync(p => p.UserId == model.UserId && !p.IsDeleted);
                if (existingPrinter != null)
                {
                    existingPrinter.Name = model.Name;
                    _printerRegistrationRepo.Update(existingPrinter);
                    await _printerRegistrationRepo.SaveAllAsync();
                    _toastNotification.AddSuccessToastMessage("تم تحديث الطابعة بنجاح");
                }
                else
                {
                    var printer = new PrinterRegistration
                    {
                        UserId = model.UserId,
                        Name = model.Name,
                        IsDeleted = false
                    };
                    _printerRegistrationRepo.Add(printer);
                    await _printerRegistrationRepo.SaveAllAsync();
                    _toastNotification.AddSuccessToastMessage("تم تسجيل الطابعة بنجاح");
                }
                return RedirectToAction(nameof(GetPrintersRegistrations));
            }
            else
            {
                _toastNotification.AddErrorToastMessage("تأكد من صحة البيانات");
                var printers = await _printerRegistrationRepo.GetAllAsync(d => !d.IsDeleted);
                var users = await _userManager.Users
                    .Where(u => !u.IsDeleted && u.UserRole.Any(x => x.Role.Name == "Cashier"))
                    .ToListAsync();
                var printerRegistrationModelDto = new PrinterRegistrationModelDto
                {
                    PrinterRegistrationGetDtos = printers,
                    PrinterRegistrationRegisterDto = new PrinterRegistrationRegisterDto()
                    {
                        Users = _mapper.Map<List<CommonUserDrop>>(users),
                    }
                };
                // إعادة تحميل البيانات اللازمة لتعبئة الموديل
                var fullModel = new PrinterRegistrationModelDto
                {
                    PrinterRegistrationRegisterDto = model,
                    PrinterRegistrationGetDtos = printers // تأكد من تضمين الـ User
                };
                // املأ قائمة المستخدمين من جديد
                fullModel.PrinterRegistrationRegisterDto.Users = _mapper.Map<List<CommonUserDrop>>(users);
                return View("GetPrintersRegistrations", fullModel);
            }
        }
        #endregion
    }
}
