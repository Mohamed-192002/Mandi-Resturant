using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastracture.Services.Permission
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IRepository<RoleClaims> _RoleClaimsRepo;
        private readonly IRepository<UserRole> _UserRoleRepo;

        public PermissionAuthorizationHandler(IRepository<RoleClaims> RoleClaimsRepo
            , IRepository<UserRole> UserRoleRepo
            )
        {
            _RoleClaimsRepo = RoleClaimsRepo;
            _UserRoleRepo = UserRoleRepo;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User == null)
                return;

            //var canAccess = context.User.Claims.Any(c => c.Type == "Permissions" && c.Value == requirement.Permission && c.Issuer == "LOCAL AUTHORITY");

         //   var UserRoles = _UserRoleRepo.GetAllAsync(n => n.User.Email == context.User.Identity.Name).Result.Select(n => n.RoleId).ToList();
            var UserRoles = _UserRoleRepo.GetAllAsync(n => n.User.UserName == context.User.Identity.Name).Result.Select(n => n.RoleId).ToList();

            var UserClaim = await _RoleClaimsRepo.GetAllAsync(n => UserRoles.Contains(n.RoleId));
           
            var canAccess = UserClaim.Any(c => c.ClaimType == "Permissions" && c.ClaimValue == requirement.Permission);



            if (canAccess)
            {
                context.Succeed(requirement);
                return;
            }
        }
    }

}