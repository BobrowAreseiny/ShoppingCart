using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCart.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")] 
    public class UsersController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public UsersController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }
        
        public IActionResult Index() => View(_userManager.Users);
    }
}
