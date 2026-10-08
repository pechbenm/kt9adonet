using KT9.Models;
using KT9.Security;
using Microsoft.AspNetCore.Identity;

namespace KT9.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var config = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeeder");

        foreach (var role in new[] { AppRoles.Admin, AppRoles.User })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var email = config["Seed:AdminEmail"];
        var password = config["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("Seed:AdminEmail / Seed:AdminPassword не заданы: администратор не создан.");
            return;
        }

        var admin = await userManager.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = email,   
                Email = email,
                EmailConfirmed = true,
                FullName = "Администратор"
            };

            var result = await userManager.CreateAsync(admin, password);
            if (!result.Succeeded)
            {
                logger.LogError("Не удалось создать администратора: {Errors}",
                    string.Join("; ", result.Errors.Select(e => e.Description)));
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(admin, AppRoles.Admin))
            await userManager.AddToRoleAsync(admin, AppRoles.Admin);
    }
}