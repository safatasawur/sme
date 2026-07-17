using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SMElevate.Web.Areas.Banks.ViewModels;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Areas.Banks.Controllers;

[Area("Banks")]
public class BankAuthController : Controller
{
    private readonly IAuthService _auth;
    private readonly IOtpService _otp;
    private readonly IEmailService _email;
    private readonly IAuditService _audit;

    public BankAuthController(IAuthService auth, IOtpService otp, IEmailService email, IAuditService audit)
    { _auth = auth; _otp = otp; _email = email; _audit = audit; }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "BankDashboard");
        return View(new BankLoginViewModel());
    }

    // AJAX endpoint — returns JSON so login page can stay on one screen (matching HTML behavior)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendOtp([FromBody] BankLoginViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(vm?.Email))
            return Json(new { ok = false, error = "Email address is required." });

        if (!IsValidEmail(vm.Email))
            return Json(new { ok = false, error = "Please enter a valid email address." });

        var user = await _auth.ValidateBankEmailAsync(vm.Email);
        if (user is null)
            return Json(new { ok = false, error = "This email is not registered as an active Bank user with a bank assignment." });

        var code = _otp.GenerateOtp();
        _otp.StoreOtp(vm.Email, code);

        // Store in session as backup
        HttpContext.Session.SetString("PendingBankEmail", vm.Email);

        await _email.SendFromTemplateAsync("BANK_OTP_LOGIN", vm.Email,
            new() { ["FullName"] = user.FullName, ["OTP"] = code });

        return Json(new { ok = true, message = "OTP has been sent to your registered bank email address." });
    }

    // AJAX endpoint — verify OTP and sign in
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtpAjax([FromBody] OtpVerifyViewModel vm)
    {
        var email = vm?.PendingEmail ?? HttpContext.Session.GetString("PendingBankEmail") ?? "";
        if (string.IsNullOrEmpty(email))
            return Json(new { ok = false, error = "Session expired. Please enter your email and request a new OTP." });

        if (string.IsNullOrWhiteSpace(vm?.Otp))
            return Json(new { ok = false, error = "Please enter the OTP sent to your email." });

        if (!_otp.ValidateOtp(email, vm.Otp))
            return Json(new { ok = false, error = "Invalid OTP. Please try again or click Resend OTP." });

        _otp.ClearOtp(email);
        HttpContext.Session.Remove("PendingBankEmail");

        var user = await _auth.ValidateBankEmailAsync(email);
        if (user is null) return Json(new { ok = false, error = "User not found. Please try again." });

        var principal = _auth.CreatePrincipal(user);
        var claims = principal.Claims.ToList();
        if (user.Bank is not null)
            claims.Add(new System.Security.Claims.Claim("BankName", user.Bank.BankName));
        var identity = new System.Security.Claims.ClaimsIdentity(claims,
            CookieAuthenticationDefaults.AuthenticationScheme);
        principal = new System.Security.Claims.ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

        await _auth.UpdateLastLoginAsync(user.Id);
        await _audit.LogAsync("Login", "ApplicationUser", user.Id, userId: user.Id);

        return Json(new { ok = true, redirect = Url.Action("Index", "BankDashboard") });
    }

    // AJAX endpoint — resend OTP (uses email from body since session may be unreliable)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendOtp([FromBody] BankLoginViewModel vm)
    {
        var email = vm?.Email ?? HttpContext.Session.GetString("PendingBankEmail") ?? "";
        if (string.IsNullOrEmpty(email))
            return Json(new { ok = false, error = "Email not found. Please start again." });

        var user = await _auth.ValidateBankEmailAsync(email);
        if (user is null) return Json(new { ok = false, error = "User not found." });

        var code = _otp.GenerateOtp();
        _otp.StoreOtp(email, code);
        HttpContext.Session.SetString("PendingBankEmail", email);

        await _email.SendFromTemplateAsync("BANK_OTP_LOGIN", email,
            new() { ["FullName"] = user.FullName, ["OTP"] = code });

        return Json(new { ok = true, message = "OTP has been sent to your registered bank email address." });
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

    private static bool IsValidEmail(string email) =>
        System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$");
}
