using Microsoft.AspNetCore.Identity;

namespace KT9.Security;

public class RussianIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DuplicateUserName(string userName) =>
        new() { Code = nameof(DuplicateUserName), Description = $"Пользователь «{userName}» уже существует." };

    public override IdentityError DuplicateEmail(string email) =>
        new() { Code = nameof(DuplicateEmail), Description = $"Пользователь «{email}» уже существует." };

    public override IdentityError InvalidEmail(string? email) =>
        new() { Code = nameof(InvalidEmail), Description = "Некорректный email." };

    public override IdentityError InvalidUserName(string? userName) =>
        new() { Code = nameof(InvalidUserName), Description = "Некорректный email." };

    public override IdentityError PasswordTooShort(int length) =>
        new() { Code = nameof(PasswordTooShort), Description = $"Пароль должен содержать не менее {length} символов." };

    public override IdentityError PasswordRequiresDigit() =>
        new() { Code = nameof(PasswordRequiresDigit), Description = "Пароль должен содержать хотя бы одну цифру." };

    public override IdentityError PasswordRequiresLower() =>
        new() { Code = nameof(PasswordRequiresLower), Description = "Пароль должен содержать строчную букву." };

    public override IdentityError PasswordRequiresUpper() =>
        new() { Code = nameof(PasswordRequiresUpper), Description = "Пароль должен содержать заглавную букву." };

    public override IdentityError PasswordRequiresNonAlphanumeric() =>
        new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Пароль должен содержать спецсимвол (например, !)." };

    public override IdentityError ConcurrencyFailure() =>
        new() { Code = nameof(ConcurrencyFailure), Description = "Данные были изменены другим пользователем. Обновите страницу." };
}