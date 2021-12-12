using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCart.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        // GET / admin / roles
        public IActionResult Index() => View(_roleManager.Roles);

        // GET / admin / roles / create
        public IActionResult Create() => View();
        
        //Post / admin / roles / create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([MinLength(2),Required] string name)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await _roleManager.CreateAsync( new IdentityRole(name));
                if (result.Succeeded)
                {
                    TempData["Success"] = "The role has been created!";
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (IdentityError error in result.Errors) 
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            ModelState.AddModelError("", "Minimum length is 2");
            return View();
        }


        // GET / admin / roles / edit
        public async Task<IActionResult> Edit(string id)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(id);

            List<AppUser> members = new List<AppUser>();
            List<AppUser> nonMembers = new List<AppUser>();

            foreach (AppUser user in _userManager.Users)
            {
                var list = await _userManager.IsInRoleAsync(user, role.Name) ? members : nonMembers;
                list.Add(user);
            }

            return View(new RoleEdit
            {
                Role = role,
                Members = members,
                NonMember = nonMembers
            });
        }

        // Post / admin / roles / edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoleEdit roleEdit)
        {
            IdentityResult result;
            foreach (string userId in roleEdit.AddIds ?? new string[] { })
            {
                AppUser user = await _userManager.FindByIdAsync(userId);
                result = await _userManager.AddToRoleAsync(user,roleEdit.RoleName);
            }
            foreach (string userId in roleEdit.DeleteIds ?? new string[] { })
            {
                AppUser user = await _userManager.FindByIdAsync(userId);
                result = await _userManager.RemoveFromRoleAsync(user,roleEdit.RoleName);
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
