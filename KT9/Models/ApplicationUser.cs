using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace KT9.Models;

public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string? FullName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
