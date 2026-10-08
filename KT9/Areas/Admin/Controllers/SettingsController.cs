using KT9.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using KT9.Areas.Admin.Models;

namespace KT9.Areas.Admin.Controllers;

public class SettingsController : AdminControllerBase
{
    private readonly IdentityOptions _identity;
    private readonly IWebHostEnvironment _env;

    public SettingsController(IOptions<IdentityOptions> identity, IWebHostEnvironment env)
    {
        _identity = identity.Value;
        _env = env;
    }

    public IActionResult Index() => View(new SettingsViewModel(
        _env.EnvironmentName,
        _identity.Password.RequiredLength,
        _identity.Password.RequireDigit,
        _identity.Password.RequireUppercase,
        _identity.Password.RequireLowercase,
        _identity.Password.RequireNonAlphanumeric,
        _identity.Lockout.MaxFailedAccessAttempts,
        (int)_identity.Lockout.DefaultLockoutTimeSpan.TotalMinutes));
}