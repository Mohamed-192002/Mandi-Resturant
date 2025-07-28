using AutoMapper;
using Core.Dtos.RolesDto;
using Core.Entities;
using Core.Interfaces;
using Infrastracture.Services.Permission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace SiteFront.Areas.Managment.Controllers
{
    [Area("Managment")]
    //[AllowAnonymous]
    public class RolesController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private readonly IRepository<Role> _RoleRepo;
        private readonly IRepository<RoleClaims> _RoleClaimRepoRepo;

        private readonly RoleManager<Role> _roleManager;
        public List<RolesGetDto> RolesGetmodel { get; set; }
        public RolesGetDto DeletRolesGetmodel { get; set; }
        public RolesRegisterDto RolesRegisterModel { get; set; }
        public List<string> RoleClaimSelect { get; set; } = new List<string>();
        public List<RoleClaims> RoleClaims { get; set; }

        public RolesController(IMapper mapper,
                               IRepository<Role> RoleRepo,
                               IRepository<RoleClaims> RoleClaimRepo,
                               IToastNotification toastNotification,
                               RoleManager<Role> RoleManager)
        {
            _roleManager = RoleManager;
            _mapper = mapper;
            _RoleRepo = RoleRepo;
            _RoleClaimRepoRepo = RoleClaimRepo;
            _toastNotification = toastNotification;
        }

        [Authorize("Permissions.RolesIndex")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var RolsDb = await _RoleRepo.GetAllAsync();

            RolesGetmodel = _mapper.Map<List<RolesGetDto>>(RolsDb);
            var rolesModelDto = new RolesModelDto
            {
                RolesGetDtos = RolesGetmodel
            };
            return View(rolesModelDto);
        }

        [Authorize("Permissions.RolesCreate")]
        [HttpGet]
        public IActionResult Create()
        {
            var rolesRegisterDto = new RolesRegisterDto();
            return View(rolesRegisterDto);
        }

        [Authorize("Permissions.RolesCreate")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RolesRegisterDto model)
        {
            if (ModelState.IsValid)
            {
                var RolesDb = _mapper.Map<Role>(model);
                _RoleRepo.Add(RolesDb);
                await _RoleRepo.SaveAllAsync();
                _toastNotification.AddSuccessToastMessage("تمت الاضافة بنجاح");
            }
            else
            {
                _toastNotification.AddErrorToastMessage("تأكد من إكمال البيانات ثم أعد المحاولة");
            }
            return RedirectToAction(nameof(Index));
        }

        //[Authorize("Permissions.RolesEdit")]
        //public async Task<IActionResult> Edit(Guid? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }
        //    var RoleDb = await _RoleRepo.GetByIdAsync((Guid)id);

        //    RolesRegisterModel = _mapper.Map<RolesRegisterDto>(RoleDb);

        //    if (RolesRegisterModel == null)
        //    {
        //        return NotFound();
        //    }
        //    return View("Create", RolesRegisterModel);
        //}

        [Authorize("Permissions.RolesEdit")]
        public async Task<IActionResult> GetData(Guid id)
        {
            var RoleDb = await _RoleRepo.GetByIdAsync(id);
            if (RoleDb == null)
                return NotFound();

            RolesRegisterModel = _mapper.Map<RolesRegisterDto>(RoleDb);
            return PartialView("_PartialRoles", RolesRegisterModel);
        }

        [Authorize("Permissions.RolesEdit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RolesRegisterDto model)
        {
            if (ModelState.IsValid)
            {
                var RolesById = await _RoleRepo.GetByIdAsync((Guid)model.Id);
                if (RolesById == null)
                    return NotFound();

                var RolesDBMapped = _mapper.Map(model, RolesById);
                _RoleRepo.Update(RolesDBMapped);
                await _RoleRepo.SaveAllAsync();
                _toastNotification.AddSuccessToastMessage("تم التعديل بنجاح");
            }
            else
            {
                _toastNotification.AddErrorToastMessage("تأكد من إكمال البيانات ثم أعد المحاولة");
            }
            return RedirectToAction(nameof(Index));
        }


        [Authorize("Permissions.RolesDelete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var role = await _RoleRepo.GetByIdAsync((Guid)id);
            var RoleClaims = await _RoleClaimRepoRepo.GetAllAsync(n => n.RoleId == id);
            _RoleClaimRepoRepo.DeletelistRange(RoleClaims.ToList());
            await _RoleClaimRepoRepo.SaveAllAsync();
            _RoleRepo.Delete(role);
            await _RoleRepo.SaveAllAsync();
            _toastNotification.AddSuccessToastMessage("تم الحذف بنجاح");
            return RedirectToAction(nameof(Index));
        }

        private bool RoleExists(Guid id)
        {
            return _RoleRepo.GetByIdAsync(id) == null ? false : true;
        }


        [Authorize("Permissions.MangeRolePermition")]
        public async Task<IActionResult> MangeRolePermition(Guid Id)
        {
            //المجموعه
            var Role = await _RoleRepo.GetByIdAsync(Id);

            if (Role == null)
            {
                return NotFound();
            }
            //الصلاحيات
            var RolesClaims = _RoleClaimRepoRepo.GetAllAsync(n => n.RoleId == Id).Result.Select(n => n.ClaimValue).ToList();


            var Allclaim = Permissions.GenerateAllPermissionsForModule();

            var AllPermition = Allclaim.Select(n => new ClaimViewModel { ClaimValue = n.en, ArName = n.ar }).ToList();

            foreach (var item in AllPermition)
            {
                if (RolesClaims.Any(n => n == item.ClaimValue))
                {
                    item.Isselected = true;
                }
            }

            EditRolesClaimDto viemodel = _mapper.Map<EditRolesClaimDto>(Role);

            viemodel.Claims = AllPermition;

            return View(viemodel);
        }


        [Authorize("Permissions.MangeRolePermition")]
        [HttpPost]
        public async Task<IActionResult> MangeRolePermition(EditRolesClaimDto model)
        {
            if (ModelState.IsValid)
            {
                var RoleClaims = await _RoleClaimRepoRepo.GetAllAsync(n => n.RoleId == model.Id);
                _RoleClaimRepoRepo.DeletelistRange(RoleClaims.ToList());

                foreach (var item in model.Claims.Where(n => n.Isselected))
                {
                    RoleClaims roleClaims = new RoleClaims()
                    {
                        RoleId = model.Id,
                        ClaimType = "Permissions",
                        ClaimValue = item.ClaimValue

                    };
                    _RoleClaimRepoRepo.Add(roleClaims);
                }

                await _RoleClaimRepoRepo.SaveAllAsync();
                _toastNotification.AddSuccessToastMessage("تم تعديل صلاحيات المجموعه");

            }
            return RedirectToAction("Index");
        }



    }
}
