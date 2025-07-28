using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SiteFront.Areas.Auth.Controllers
{
    [Area("Auth")]
    [AllowAnonymous]
    public class AccessDeniedController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

    }
}
