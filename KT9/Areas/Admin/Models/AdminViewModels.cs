using KT9.Models;
using KT9.Security;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KT9.Areas.Admin.Models;

public record DashboardViewModel(int TotalUsers, int Admins, int NewLast30Days);

public record SettingsViewModel(
    string Environment, int PasswordMinLength, bool RequireDigit, bool RequireUpper,
    bool RequireLower, bool RequireSpecial, int MaxFailedAttempts, int LockoutMinutes);

public record RoleStat(string Role, int Count);
public record MonthStat(string Month, int Count);

public class ReportsViewModel
{
    public List<RoleStat> ByRole { get; set; } = new();
    public List<MonthStat> ByMonth { get; set; } = new();
}

public class UserViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
    public bool IsLockedOut { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserListViewModel
{
    public string? Search { get; set; }
    public List<UserViewModel> Users { get; set; } = new();
}

public class CreateUserViewModel
{
    [Required(ErrorMessage = Msg.Required), EmailAddress(ErrorMessage = Msg.Email), MaxLength(256)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    [Display(Name = "Полное имя")]
    public string? FullName { get; set; }

    [Required(ErrorMessage = Msg.Required), DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = Msg.Required), DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = Msg.PasswordsDiffer)]
    [Display(Name = "Подтверждение пароля")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = Msg.Required)]
    [Display(Name = "Роль")]
    public string Role { get; set; } = AppRoles.User;

    [BindNever, ValidateNever]
    public List<SelectListItem> RoleItems { get; set; } = new();
}

public class EditUserViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = Msg.Required), EmailAddress(ErrorMessage = Msg.Email), MaxLength(256)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    [Display(Name = "Полное имя")]
    public string? FullName { get; set; }

    [Required(ErrorMessage = Msg.Required)]
    [Display(Name = "Роль")]
    public string Role { get; set; } = AppRoles.User;

    [DataType(DataType.Password)]
    [Display(Name = "Новый пароль")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = Msg.PasswordsDiffer)]
    [Display(Name = "Подтверждение нового пароля")]
    public string? ConfirmNewPassword { get; set; }

    [BindNever, ValidateNever]
    public List<SelectListItem> RoleItems { get; set; } = new();
}