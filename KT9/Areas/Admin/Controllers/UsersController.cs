using KT9.Areas.Admin.Controllers;
using KT9.Areas.Admin.Models;
using KT9.Data;
using KT9.Models;
using KT9.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KT9.Areas.Admin.Controllers;

public class UsersController : AdminControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _db;

    public UsersController(UserManager<ApplicationUser> userManager,
                           RoleManager<IdentityRole> roleManager,
                           ApplicationDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }

    // GET /Admin/Users?search=ivan
    public async Task<IActionResult> Index(string? search)
    {
        var query = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(u => EF.Functions.Like(u.Email!, pattern)
                                  || EF.Functions.Like(u.FullName ?? "", pattern));
        }

        var users = await query.OrderBy(u => u.Email).ToListAsync();
        var ids = users.Select(u => u.Id).ToList();

        var rolesByUser = (await (from ur in _db.UserRoles
                                  join r in _db.Roles on ur.RoleId equals r.Id
                                  where ids.Contains(ur.UserId)
                                  select new { ur.UserId, RoleName = r.Name! }).ToListAsync())
                          .ToLookup(x => x.UserId, x => x.RoleName);

        return View(new UserListViewModel
        {
            Search = search,
            Users = users.Select(u => ToViewModel(u, rolesByUser[u.Id].OrderBy(r => r))).ToList()
        });
    }

    //просмотр одного пользователя 
    public async Task<IActionResult> Details(string? id)
    {
        var user = await FindUserAsync(id);
        if (user is null) return NotFound();

        return View(ToViewModel(user, await _userManager.GetRolesAsync(user)));
    }

    //добавление
    [HttpGet]
    public async Task<IActionResult> Create()
        => View(new CreateUserViewModel { RoleItems = await GetRoleItemsAsync() });

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        var roles = await GetRoleNamesAsync();
        if (!roles.Contains(model.Role))
            ModelState.AddModelError(nameof(model.Role), "Выберите существующую роль.");

        if (!ModelState.IsValid)
        {
            model.RoleItems = ToItems(roles);
            return View(model);
        }

        await using var tx = await _db.Database.BeginTransactionAsync();

        var email = model.Email.Trim();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = model.FullName?.Trim(),
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
            result = await _userManager.AddToRoleAsync(user, model.Role);

        if (!result.Succeeded)
        {
            AddErrors(result);
            model.RoleItems = ToItems(roles);
            return View(model);   
        }

        await tx.CommitAsync();
        TempData["Success"] = $"Пользователь {email} создан.";
        return RedirectToAction(nameof(Index));
    }

    //редактирование
    [HttpGet]
    public async Task<IActionResult> Edit(string? id)
    {
        var user = await FindUserAsync(id);
        if (user is null) return NotFound();

        var userRoles = await _userManager.GetRolesAsync(user);
        return View(new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Role = userRoles.FirstOrDefault() ?? AppRoles.User,
            RoleItems = await GetRoleItemsAsync()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        var user = await FindUserAsync(model.Id);
        if (user is null) return NotFound();

        var roles = await GetRoleNamesAsync();
        if (!roles.Contains(model.Role))
            ModelState.AddModelError(nameof(model.Role), "Выберите существующую роль.");

        if (model.Role != AppRoles.Admin && await IsLastAdminAsync(user))
            ModelState.AddModelError(nameof(model.Role),
                "Нельзя снять роль администратора с последнего администратора.");

        if (!ModelState.IsValid)
        {
            model.RoleItems = ToItems(roles);
            return View(model);
        }

        await using var tx = await _db.Database.BeginTransactionAsync();

        var email = model.Email.Trim();
        user.Email = email;
        user.UserName = email;
        user.FullName = model.FullName?.Trim();

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
            result = await SetSingleRoleAsync(user, model.Role);

        if (result.Succeeded && !string.IsNullOrEmpty(model.NewPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
        }

        if (!result.Succeeded)
        {
            AddErrors(result);
            model.RoleItems = ToItems(roles);
            return View(model);   // откат всех шагов
        }

        await tx.CommitAsync();
        TempData["Success"] = $"Данные пользователя {email} сохранены.";
        return RedirectToAction(nameof(Index));
    }

    //удаление 
    [HttpGet]
    public async Task<IActionResult> Delete(string? id)
    {
        var user = await FindUserAsync(id);
        if (user is null) return NotFound();

        return View(ToViewModel(user, await _userManager.GetRolesAsync(user)));
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(string? id)
    {
        var user = await FindUserAsync(id);
        if (user is null) return NotFound();

        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "Нельзя удалить собственную учётную запись.";
            return RedirectToAction(nameof(Index));
        }

        if (await IsLastAdminAsync(user))
        {
            TempData["Error"] = "Нельзя удалить последнего администратора.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
            TempData["Success"] = $"Пользователь {user.Email} удалён.";
        else
            TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));

        return RedirectToAction(nameof(Index));
    }

    //вспомогательные методы
    private async Task<ApplicationUser?> FindUserAsync(string? id)
        => string.IsNullOrEmpty(id) ? null : await _userManager.FindByIdAsync(id);

    private async Task<List<string>> GetRoleNamesAsync()
        => await _roleManager.Roles.OrderBy(r => r.Name).Select(r => r.Name!).ToListAsync();

    private async Task<List<SelectListItem>> GetRoleItemsAsync()
        => ToItems(await GetRoleNamesAsync());

    private static List<SelectListItem> ToItems(IEnumerable<string> roles)
        => roles.Select(r => new SelectListItem(r, r)).ToList();

    private async Task<IdentityResult> SetSingleRoleAsync(ApplicationUser user, string role)
    {
        var current = await _userManager.GetRolesAsync(user);
        if (current.Count == 1 && current[0] == role) return IdentityResult.Success;

        if (current.Count > 0)
        {
            var removed = await _userManager.RemoveFromRolesAsync(user, current);
            if (!removed.Succeeded) return removed;
        }
        return await _userManager.AddToRoleAsync(user, role);
    }

    private async Task<bool> IsLastAdminAsync(ApplicationUser user)
    {
        if (!await _userManager.IsInRoleAsync(user, AppRoles.Admin)) return false;
        var admins = await _userManager.GetUsersInRoleAsync(AppRoles.Admin);
        return admins.Count <= 1;
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var description in result.Errors.Select(e => e.Description).Distinct())
            ModelState.AddModelError(string.Empty, description);
    }

    private static UserViewModel ToViewModel(ApplicationUser u, IEnumerable<string> roles) => new()
    {
        Id = u.Id,
        Email = u.Email ?? string.Empty,
        FullName = u.FullName,
        Roles = roles.ToList(),
        IsLockedOut = u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow,
        CreatedAt = u.CreatedAt
    };
}