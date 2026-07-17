using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class AdminAuthController : Controller
{
    private readonly IAuthService _auth;
    private readonly IAuditService _audit;

    public AdminAuthController(IAuthService auth, IAuditService audit)
    {
        _auth = auth;
        _audit = audit;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "AdminDashboard");
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError("", "Email and password are required.");
            return View();
        }

        var user = await _auth.ValidateAdminLoginAsync(email, password);
        if (user is null)
        {
            ModelState.AddModelError("", "Invalid email address or password.");
            return View();
        }

        var principal = _auth.CreatePrincipal(user);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

        await _auth.UpdateLastLoginAsync(user.Id);
        await _audit.LogAsync("Login", "ApplicationUser", user.Id, userId: user.Id);

        return RedirectToAction("Index", "AdminDashboard");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst("UserId")?.Value;
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (int.TryParse(userId, out int uid))
            await _audit.LogAsync("Logout", "ApplicationUser", uid, userId: uid);
        return RedirectToAction("Login");
    }
}
