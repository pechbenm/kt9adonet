using KT9.Models;
using KT9.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace KT9.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);

        // после 5 неверных попыток учётная запись блокируется
        var result = await _signInManager.PasswordSignInAsync(
            model.Email.Trim(), model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            // IsLocalUrl защищает от open redirect 
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            var user = await _userManager.FindByNameAsync(model.Email.Trim());
            if (user is not null && await _userManager.IsInRoleAsync(user, AppRoles.Admin))
                return RedirectToAction("Index", "Home", new { area = "Admin" });

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        ModelState.AddModelError(string.Empty, result.IsLockedOut
            ? "Учётная запись временно заблокирована. Повторите попытку позже."
            : "Неверный email или пароль.");   
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home", new { area = "" });
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();
}