using AutoMapper;
using Core.Dtos.AuthDto;
using Core.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace SiteFront.Areas.Auth.Controllers
{

    [Area("Auth")]
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class LoginController : Controller
    {

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IToastNotification _toastNotification;

        private readonly IMapper _mapper;

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public LoginController(
           IMapper mapper, SignInManager<User> signInManager, ILogger<LoginModel> logger
            , UserManager<User> userManager, IToastNotification toastNotification)
        {
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _toastNotification = toastNotification;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginDto model, string? RedirectUrl)
        {
            try
            {
                var msg = "";
                if (ModelState.IsValid)
                {
                    var check = model.Email.Contains('@');
                    User user = null;
                    if (check)
                        user = await _userManager.FindByEmailAsync(model.Email);
                    else
                        user = await _userManager.FindByNameAsync(model.Email);

                    if (user == null)
                    {
                        msg = "خطأ بالإسم أو كلمة المرور !!";
                        ViewBag.Message = msg;
                        return View(model);
                    }
                    RedirectUrl = RedirectUrl ?? "~/Home/index";
                    var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, true, false);
                    if (result.Succeeded)
                    {
                        if (Url.IsLocalUrl(RedirectUrl))
                        {
                            _toastNotification.AddSuccessToastMessage("مستخدم صحيح");
                            return Redirect(RedirectUrl);
                        }
                        else
                        {
                            _toastNotification.AddSuccessToastMessage("مستخدم صحيح");
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        msg = "خطأ بالإسم أو كلمة المرور !!";
                        ViewBag.Message = msg;
                        return View(model);
                    }
                }
                else
                {
                    msg = "أكمل البيانات أولا !!";
                    ViewBag.Message = msg;
                    return View();
                }
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage(ex.Message);
                //ViewBag.Message = ex.Message;
                return View();
            }
        }

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync("Identity.Application");
            return RedirectToAction("Index");
        }

    }
}
