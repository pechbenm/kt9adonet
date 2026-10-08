using KT9.Security;
using KT9.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KT9.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = AppPolicies.AdminPanel)]
public abstract class AdminControllerBase : Controller
{
}