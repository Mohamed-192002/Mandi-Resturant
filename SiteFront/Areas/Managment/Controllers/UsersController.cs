using AutoMapper;
using Core.Common;
using Core.Dtos;
using Core.Dtos.UserDto;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using System.Net.Mail;

namespace SiteFront.Areas.Managment.Controllers
{
    [Area("Managment")]
    //[AllowAnonymous]
    public class UsersController : Controller
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

        public List<string> RolesSelect { get; set; } = new List<string>();
        public List<Role> roles { get; set; }
        public List<AuthenticationScheme> ExternalLogins { get; private set; }
        public UsersController(
            IRepository<User> UserRepo,
            UserManager<User> userManager,
            RoleManager<Role> RoleManager,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            IMapper mapper,
            IRepository<UserRole> UserRoleRepo,
            IToastNotification toastNotification,
            IRepository<DeviceRegistration> deviceRegistrationRepo)
        {
            _UserRepo = UserRepo;
            _mapper = mapper;
            _UserRoleRepo = UserRoleRepo;
            _toastNotification = toastNotification;
            _roleManager = RoleManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _deviceRegistrationRepo = deviceRegistrationRepo;
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
        [Authorize("Permissions.UsersIndex")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //var UsersDb = _userManager.Users.Where(u=>u.IsDeleted == false).Select(User => new UserGetDto
            //{
            //    Id = User.Id,
            //    Name = User.Name,
            //    EmployeeName = _employeeRepo.GetByIdAsync(User.EmployeeId).Result.FirstName + " " + _employeeRepo.GetByIdAsync(User.EmployeeId).Result.SecondName,
            //    Email = User.Email,
            //    PhoneNumber = User.PhoneNumber,
            //    Roles = _userManager.GetRolesAsync(User).Result
            //}).Where(u => u.Email != "mohamed@gmail.com").ToList();

            var users = await _userManager.Users.Where(u => u.IsDeleted == false).ToListAsync();
            var userDtos = new List<UserGetDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userDto = new UserGetDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles
                };

                if (userDto.Email != "mohamed@gmail.com")
                {
                    userDtos.Add(userDto);
                }
            }
            return View(userDtos);
        }

        [Authorize("Permissions.UsersCreate")]
        public async Task<IActionResult> Create()
        {
            var Roles = await _roleManager.Roles.ToListAsync();
            UserRegisterDto userRegisterDto = new UserRegisterDto();
            userRegisterDto.Roles = Roles.Select(n => new CommonDto
            {
                Id = n.Id,
                Name = n.Name,
                Isselected = false

            }).ToList();
            return View(userRegisterDto);
        }

        [Authorize("Permissions.UsersCreate")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserRegisterDto model)
        {

            if (ModelState.IsValid)
            {
                //Check Roles
                if (model.Roles.Where(n => n.Isselected).Count() == 0)
                {
                    var Roles = await _roleManager.Roles.ToListAsync();
                    UserRegisterDto userRegisterDto = new UserRegisterDto
                    {
                        ConfirmPassword = model.ConfirmPassword,
                        Email = model.Email,
                        Name = model.Name,
                        PhoneNumber = model.PhoneNumber,
                        Password = model.Password
                    };
                    userRegisterDto.Roles = Roles.Select(n => new CommonDto
                    {
                        Id = n.Id,
                        Name = n.Name,
                        Isselected = false

                    }).ToList();
                    ModelState.AddModelError("Roles", "اختر علي الاقل مجموعة واحدة");
                    return View("Create", userRegisterDto);
                }
                //Add User
                User user = _mapper.Map<User>(model);
                user.UserName = new MailAddress(model.Email).User;
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    user = await _userManager.FindByIdAsync(user.Id.ToString());

                    foreach (var item in model.Roles.Where(n => n.Isselected))
                    {
                        UserRole userRole = new UserRole()
                        {
                            RoleId = item.Id,
                            UserId = user.Id
                        };

                        _UserRoleRepo.Add(userRole);
                        await _UserRoleRepo.SaveAllAsync();
                        _toastNotification.AddSuccessToastMessage("تمت الاضافة بنجاح");
                    }

                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else
            {
                var Roles = await _roleManager.Roles.ToListAsync();
                UserRegisterDto userRegisterDto = new UserRegisterDto
                {
                    ConfirmPassword = model.ConfirmPassword,
                    Email = model.Email,
                    Name = model.Name,
                    PhoneNumber = model.PhoneNumber,
                    Password = model.Password
                };
                userRegisterDto.Roles = Roles.Select(n => new CommonDto
                {
                    Id = n.Id,
                    Name = n.Name,
                    Isselected = false

                }).ToList();
                _toastNotification.AddErrorToastMessage("بيانات غير صحيحة");
                return View("Create", userRegisterDto);
            }
            return RedirectToAction("Index");
        }


        [Authorize("Permissions.UsersEdit")]
        public async Task<IActionResult> Edit(Guid? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var user = await _UserRepo.GetByIdAsync((Guid)Id);
            if (user == null)
            {
                return NotFound();
            }
            RolesSelect = (List<string>)await _userManager.GetRolesAsync(user);
            var AllRolesUser = await _roleManager.Roles.ToListAsync();
            var rolesUser = _mapper.Map<List<CommonDto>>(AllRolesUser);
            setSelected(ref rolesUser);

            UserEditDto viemodel = _mapper.Map<UserEditDto>(user);
            viemodel.Roles = rolesUser;

            return View(viemodel);
        }

        public void setSelected(ref List<CommonDto> roles)
        {
            roles.ForEach(e =>
            {
                if (RolesSelect.Contains(e.Name))
                {
                    e.Isselected = true;
                }
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("Permissions.UsersEdit")]
        public async Task<IActionResult> Edit(UserEditDto model)
        {
            if (ModelState.IsValid)
            {
                var UserDb = await _UserRepo.GetByIdAsync(model.Id);
                if (UserDb == null)
                    return NotFound();
                var newEmail = model.Email;
                var OldEmail = UserDb.Email;
                var userDBMapped = _mapper.Map(model, UserDb);
                userDBMapped.UserName = new MailAddress(model.Email).User;
                var result = await _userManager.UpdateAsync(userDBMapped);
                if (result.Succeeded)
                {
                    List<UserRole> UserRules = (List<UserRole>)await _UserRoleRepo.GetAllAsync(n => n.UserId == model.Id);
                    _UserRoleRepo.DeletelistRange(UserRules);
                    foreach (var item in model.Roles.Where(n => n.Isselected))
                    {
                        UserRole userRole = new UserRole()
                        {
                            RoleId = item.Id,
                            UserId = model.Id
                        };
                        _UserRoleRepo.Add(userRole);
                        await _UserRoleRepo.SaveAllAsync();
                    }

                }
                if (newEmail != OldEmail)
                {
                    // _toastNotification.AddSuccessToastMessage("تم التعديل بنجاح أعد تسجيل الدخول");
                    await HttpContext.SignOutAsync("Identity.Application");
                    return RedirectToAction("Index", "Login", new { area = "Auth" });
                }
                else
                {
                    _toastNotification.AddSuccessToastMessage("تم التعديل بنجاح");
                    return RedirectToAction("Index");
                }

            }
            else
            {
                _toastNotification.AddSuccessToastMessage("تأكد من صحة البيانات");
                return View("Index", model);
            }

        }

        [Authorize("Permissions.UsersDelete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var user = await _UserRepo.GetByIdAsync(id);
                user.IsDeleted = true;
                _UserRepo.Update(user);
                await _UserRepo.SaveAllAsync();
                _toastNotification.AddSuccessToastMessage("تم الحذف بنجاح");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("لا يمكن حذف هذا المستخدم");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        [Authorize("Permissions.UsersEditPassword")]
        public IActionResult ChangePassword(Guid id)
        {
            var changeUserPasswordDto = new ChangeUserPasswordDto();
            changeUserPasswordDto.Id = id;
            return View(changeUserPasswordDto);
        }

        [HttpPost]
        [Authorize("Permissions.UsersEditPassword")]
        public async Task<IActionResult> ChangePassword(ChangeUserPasswordDto model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id.ToString());
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    _toastNotification.AddSuccessToastMessage("تم تغيير الباسوورد بنجاح");
                    ModelState.Clear();
                    return RedirectToAction("Index");

                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    _toastNotification.AddErrorToastMessage("كلمة السر غير صحيحة حاول مرة اخري");
                }
            }
            return View();
        }


    }
}
