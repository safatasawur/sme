using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SMElevate.Web.Areas.EndUser.ViewModels;
using SMElevate.Web.Data;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace SMElevate.Web.Areas.EndUser.Controllers;

[Area("EndUser")]
public class EndUserAccountController : Controller
{
    private readonly IAuthService _auth;
    private readonly IUserService _users;
    private readonly IAuditService _audit;
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;

    public EndUserAccountController(IAuthService auth, IUserService users,
        IAuditService audit, ApplicationDbContext db, IConfiguration config)
    { _auth = auth; _users = users; _audit = audit; _db = db; _config = config; }

    // ── Manual login ─────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true) return Redirect(GetCurrentStep());
        return View(new EndUserLoginViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(EndUserLoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var user = await _users.GetUserByEmailAsync(vm.Email);
        if (user is null || user.UserType != UserType.SME || !_auth.VerifyPassword(vm.Password, user.PasswordHash))
        { ModelState.AddModelError("", "Invalid email address or password."); return View(vm); }
        if (!user.IsActive) { ModelState.AddModelError("", "Your account is inactive."); return View(vm); }
        await SignIn(user);
        return Redirect(await GetCurrentStepForUser(user.Id));
    }

    // ── Register ─────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true) return Redirect(GetCurrentStep());
        return View(new EndUserRegisterViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(EndUserRegisterViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        if (!vm.AgreeTerms) { ModelState.AddModelError("AgreeTerms", "You must agree to the terms and conditions."); return View(vm); }
        if (await _users.EmailExistsAsync(vm.Email)) { ModelState.AddModelError("Email", "This email address is already registered."); return View(vm); }
        var endUserRoleId = await _db.Roles
            .Where(r => r.RoleName == "EndUser")
            .Select(r => (int?)r.Id)
            .FirstOrDefaultAsync();

        var user = new ApplicationUser { FullName = vm.FullName, EmailAddress = vm.Email, MobileNo = vm.MobileNo, UserType = UserType.SME, RoleId = endUserRoleId, IsActive = true, AuthenticationMode = "Manual" };
        await _users.CreateUserAsync(user, vm.Password);
        await SignIn(user);
        return RedirectToAction("Complete", "EndUserProfile");
    }

    // ── External login — challenge ────────────────────────────────────────────

    [HttpGet]
    public IActionResult ExternalLogin(string provider)
    {
        if (User.Identity?.IsAuthenticated == true) return Redirect(GetCurrentStep());

        if (!IsProviderConfigured(provider))
        {
            TempData["Error"] = provider switch
            {
                "Google"    => "Google login is not configured.",
                "Apple"     => "Apple login is not configured.",
                "Microsoft" => "Microsoft login is not configured.",
                _           => $"{provider} login is not configured."
            };
            return RedirectToAction("Login");
        }

        var callbackUrl = Url.Action("ExternalLoginCallback", "EndUserAccount",
            new { area = "EndUser" }, Request.Scheme);

        var props = new AuthenticationProperties
        {
            RedirectUri = callbackUrl,
            Items       = { [".provider"] = provider }
        };

        return Challenge(props, provider);
    }

    // ── External login — callback (GET after OAuth middleware processes it) ───

    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback()
    {
        // Read the external identity the OAuth middleware stored in the "External" cookie
        var result = await HttpContext.AuthenticateAsync("External");
        if (!result.Succeeded || result.Principal is null)
        {
            TempData["Error"] = "External login failed. Please try again.";
            return RedirectToAction("Login");
        }

        // Delete the intermediate External cookie — no longer needed
        await HttpContext.SignOutAsync("External");

        var principal = result.Principal;
        string? providerRaw = null;
        result.Properties?.Items.TryGetValue(".provider", out providerRaw);
        var provider = providerRaw ?? "";

        var email    = principal.FindFirstValue(ClaimTypes.Email);
        var nameId   = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var fullName = BuildName(principal);

        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["Error"] = "Email address was not returned by the login provider.";
            return RedirectToAction("Login");
        }

        // ── Find existing external login link ─────────────────────────────
        var extLogin = await _db.UserExternalLogins
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.Provider == provider && l.ProviderUserId == nameId);

        ApplicationUser? user;

        if (extLogin is not null)
        {
            // Known provider+user combo — sign in directly
            user = extLogin.User;
        }
        else
        {
            // Check for existing account by email (avoid duplicate accounts)
            user = await _users.GetUserByEmailAsync(email);

            if (user is not null && user.UserType != UserType.SME)
            {
                TempData["Error"] = "This email belongs to a non-EndUser account and cannot use social login here.";
                return RedirectToAction("Login");
            }

            if (user is null)
            {
                var endUserRoleId = await _db.Roles
                    .Where(r => r.RoleName == "EndUser")
                    .Select(r => (int?)r.Id)
                    .FirstOrDefaultAsync();

                // Create a new EndUser account
                user = new ApplicationUser
                {
                    FullName           = fullName ?? email,
                    EmailAddress       = email,
                    UserType           = UserType.SME,
                    RoleId             = endUserRoleId,
                    IsActive           = true,
                    IsEmailVerified    = true,
                    AuthenticationMode = provider,
                    PasswordHash       = ""
                };
                await _users.CreateUserAsync(user, Guid.NewGuid().ToString("N"));
            }

            // Link the provider to this user account
            _db.UserExternalLogins.Add(new UserExternalLogin
            {
                UserId         = user.Id,
                Provider       = provider,
                ProviderUserId = nameId,
                ProviderEmail  = email,
                CreatedAt      = DateTime.UtcNow,
                LastLoginAt    = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        if (!user.IsActive)
        {
            TempData["Error"] = "Your account is inactive. Please contact support.";
            return RedirectToAction("Login");
        }

        // Update last login timestamp on the link record
        if (extLogin is not null)
        {
            extLogin.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        await SignIn(user);
        return Redirect(await GetCurrentStepForUser(user.Id));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private bool IsProviderConfigured(string provider) => provider switch
    {
        "Google"    => !string.IsNullOrWhiteSpace(_config["Authentication:Google:ClientId"])
                    && !string.IsNullOrWhiteSpace(_config["Authentication:Google:ClientSecret"]),
        "Apple"     => !string.IsNullOrWhiteSpace(_config["Authentication:Apple:ClientId"])
                    && !string.IsNullOrWhiteSpace(_config["Authentication:Apple:TeamId"])
                    && !string.IsNullOrWhiteSpace(_config["Authentication:Apple:KeyId"])
                    && !string.IsNullOrWhiteSpace(_config["Authentication:Apple:PrivateKey"]),
        "Microsoft" => !string.IsNullOrWhiteSpace(_config["Authentication:Microsoft:ClientId"])
                    && !string.IsNullOrWhiteSpace(_config["Authentication:Microsoft:ClientSecret"]),
        _           => false
    };

    private static string? BuildName(ClaimsPrincipal principal)
    {
        var name = principal.FindFirstValue(ClaimTypes.Name);
        if (!string.IsNullOrWhiteSpace(name)) return name;
        var given  = principal.FindFirstValue(ClaimTypes.GivenName) ?? "";
        var family = principal.FindFirstValue(ClaimTypes.Surname)   ?? "";
        var full   = $"{given} {family}".Trim();
        return string.IsNullOrWhiteSpace(full) ? null : full;
    }

    private async Task SignIn(ApplicationUser user)
    {
        var principal = _auth.CreatePrincipal(user);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });
        await _auth.UpdateLastLoginAsync(user.Id);
        await _audit.LogAsync("Login", "ApplicationUser", user.Id, userId: user.Id);
    }

    private string GetCurrentStep()
    {
        if (User.Identity?.IsAuthenticated != true) return "/EndUser/EndUserAccount/Login";
        if (!int.TryParse(User.FindFirst("UserId")?.Value, out var userId)) return "/EndUser/EndUserAccount/Login";
        var profile = _db.EndUserProfiles.FirstOrDefault(p => p.UserId == userId);
        if (profile is null) return "/EndUser/EndUserProfile/Complete";
        if (!profile.IsVerified) return "/EndUser/EndUserProfile/Verify";
        return "/EndUser/EndUserDashboard/Index";
    }

    private async Task<string> GetCurrentStepForUser(int userId)
    {
        var profile = await _db.EndUserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile is null) return "/EndUser/EndUserProfile/Complete";
        if (!profile.IsVerified) return "/EndUser/EndUserProfile/Verify";
        return "/EndUser/EndUserDashboard/Index";
    }
}
