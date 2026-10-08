using KT9.Areas.Admin.Controllers;
using KT9.Areas.Admin.Models;
using KT9.Models;
using KT9.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KT9.Areas.Admin.Controllers;

public class HomeController : AdminControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    public HomeController(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<IActionResult> Index()
    {
        var total = await _userManager.Users.CountAsync();
        var admins = (await _userManager.GetUsersInRoleAsync(AppRoles.Admin)).Count;
        var since = DateTime.UtcNow.AddDays(-30);
        var recent = await _userManager.Users.CountAsync(u => u.CreatedAt >= since);

        return View(new DashboardViewModel(total, admins, recent));
    }
}