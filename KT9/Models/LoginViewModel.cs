using System.ComponentModel.DataAnnotations;

namespace KT9.Models;

public static class Msg
{
    public const string Required = "Поле «{0}» обязательно.";
    public const string Email = "Введите корректный email.";
    public const string PasswordsDiffer = "Пароли не совпадают.";
}

public class LoginViewModel
{
    [Required(ErrorMessage = Msg.Required), EmailAddress(ErrorMessage = Msg.Email)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = Msg.Required), DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Запомнить меня")]
    public bool RememberMe { get; set; }
}