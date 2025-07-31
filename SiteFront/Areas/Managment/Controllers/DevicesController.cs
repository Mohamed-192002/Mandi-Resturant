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

        public DevicesController(IMapper mapper, IToastNotification toastNotification, SignInManager<User> signInManager, UserManager<User> userManager, RoleManager<Role> roleManager, IRepository<User> userRepo, ILogger<RegisterModel> logger, IRepository<UserRole> userRoleRepo, IRepository<DeviceRegistration> deviceRegistrationRepo)
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
    }
}
