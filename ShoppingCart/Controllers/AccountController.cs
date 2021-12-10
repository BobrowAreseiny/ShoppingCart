using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCart.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _singInManager;
        private IPasswordHasher<AppUser> _passwordHasher;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> singInManager,IPasswordHasher<AppUser> passwordHasher)
        {
            _userManager = userManager;
            _singInManager = singInManager;
            _passwordHasher = passwordHasher;
        }

        // GET /account / register
        [AllowAnonymous]
        public IActionResult Register() => View();


        // GET / account / register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                AppUser appUser = new AppUser()
                {
                    UserName = user.UserName,
                    Email = user.Email
                };

                IdentityResult result = await _userManager.CreateAsync(appUser, user.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("Login");
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("",error.Description);
                    }
                }
            }
            return View(user);
        }

        //GET / account / Login
        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            Login login = new Login()
            {
                ReturnUrl = returnUrl
            };

            return View(login);
        }

        //Post / account / Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login login)
        {
            if (ModelState.IsValid)
            {
                AppUser appUser = await _userManager.FindByEmailAsync(login.Email);
                if (appUser != null)
                {
                    Microsoft.AspNetCore.Identity.SignInResult result = await _singInManager.PasswordSignInAsync(appUser, login.Password, false, false);
                    if (result.Succeeded)
                    {
                        return Redirect(login.ReturnUrl ?? "/");
                    }
                }
                ModelState.AddModelError("", "Login failed, wrong credentials.");
            }

            return View(login);
        }

        // GET / account / logout
        public async Task<IActionResult> Logout()
        {
            await _singInManager.SignOutAsync();

            return Redirect("/");
        }

        // GET / account / edit
        public async Task<IActionResult> Edit()
        {
            AppUser appUser = await _userManager.FindByNameAsync(User.Identity.Name);

            UserEdit user = new UserEdit(appUser);

            return View(user);
        }


        // PAst / account / edit

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserEdit user)
        {
            AppUser appUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (ModelState.IsValid)
            {
                appUser.Email = user.Email;
                if(user.Password != null)
                {
                    appUser.PasswordHash = _passwordHasher.HashPassword(appUser, user.Password);
                }

                IdentityResult result = await _userManager.UpdateAsync(appUser);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Your information has been edited!";
                }
            }
            return View();
        }
    }
}
