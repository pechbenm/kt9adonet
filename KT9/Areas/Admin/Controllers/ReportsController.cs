using KT9.Areas.Admin.Controllers;
using KT9.Data;
using KT9.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KT9.Areas.Admin.Models;

namespace KT9.Areas.Admin.Controllers;

public class ReportsController : AdminControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReportsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        //пользователи по ролям (роли без пользователей тоже попадут с нулём)
        var roleRows = await (from r in _db.Roles
                              join ur in _db.UserRoles on r.Id equals ur.RoleId into g
                              orderby r.Name
                              select new { r.Name, Count = g.Count() }).ToListAsync();

        //регистрации за последние 6 месяцев (группируем в памяти: надёжно для любой СУБД)
        var now = DateTime.UtcNow;
        var from = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
        var dates = await _userManager.Users.AsNoTracking()
            .Where(u => u.CreatedAt >= from)
            .Select(u => u.CreatedAt)
            .ToListAsync();

        var byMonth = Enumerable.Range(0, 6)
            .Select(i => from.AddMonths(i))
            .Select(m => new MonthStat(m.ToString("MM.yyyy"),
                                       dates.Count(d => d.Year == m.Year && d.Month == m.Month)))
            .ToList();

        return View(new ReportsViewModel
        {
            ByRole = roleRows.Select(x => new RoleStat(x.Name ?? "", x.Count)).ToList(),
            ByMonth = byMonth
        });
    }
}