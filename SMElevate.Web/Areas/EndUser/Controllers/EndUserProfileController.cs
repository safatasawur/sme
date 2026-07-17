using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Areas.EndUser.ViewModels;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Areas.EndUser.Controllers;

[Area("EndUser")]
[Authorize(Policy = "EndUserOnly")]
public class EndUserProfileController : Controller
{
    private readonly IUserService _users;
    private readonly IEmailService _email;
    private readonly INotificationService _notifications;
    private readonly IConfiguration _config;
    private readonly Data.ApplicationDbContext _db;

    public EndUserProfileController(IUserService users, IEmailService email, INotificationService notifications, IConfiguration config, Data.ApplicationDbContext db)
    { _users = users; _email = email; _notifications = notifications; _config = config; _db = db; }

    private int UserId => int.Parse(User.FindFirst("UserId")!.Value);

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Profile"; ViewData["PreTitle"] = "EndUser Portal"; ViewData["ActiveNav"] = "Profile";
        var user = await _users.GetUserByIdAsync(UserId);
        var profile = user?.Profile;
        if (profile is null) return RedirectToAction("Complete");
        return View(profile);
    }

    [HttpGet]
    public async Task<IActionResult> Complete()
    {
        ViewData["Title"] = "Complete Your Profile"; ViewData["PreTitle"] = "EndUser Portal";
        var user = await _users.GetUserByIdAsync(UserId);
        var p = user?.Profile;
        if (p is not null)
            return View(new ProfileCompleteViewModel { FirstName = p.FirstName, LastName = p.LastName, MobileNo = p.MobileNo, CNIC = p.CNIC, BusinessEmailAddress = p.BusinessEmailAddress, GenderOfProprietor = p.GenderOfProprietor });
        return View(new ProfileCompleteViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(ProfileCompleteViewModel vm)
    {
        ViewData["Title"] = "Complete Your Profile"; ViewData["PreTitle"] = "EndUser Portal";
        if (!ModelState.IsValid) return View(vm);
        var user = await _users.GetUserByIdAsync(UserId);
        if (user is null) return RedirectToAction("Login", "EndUserAccount");

        // CNIC uniqueness check
        var cnic = vm.CNIC?.Trim() ?? "";
        if (!string.IsNullOrEmpty(cnic))
        {
            var cnicTaken = await _db.EndUserProfiles
                .AnyAsync(p => p.CNIC == cnic && p.UserId != UserId);
            if (cnicTaken)
            {
                ModelState.AddModelError("CNIC", "CNIC already exists. Please use a different CNIC.");
                return View(vm);
            }
        }
        vm.CNIC = cnic;

        var token = Guid.NewGuid().ToString("N")[..6].ToUpper();
        var profile = user.Profile ?? new EndUserProfile { UserId = UserId };
        profile.FirstName = vm.FirstName; profile.LastName = vm.LastName; profile.MobileNo = vm.MobileNo;
        profile.CNIC = vm.CNIC; profile.BusinessEmailAddress = vm.BusinessEmailAddress; profile.GenderOfProprietor = vm.GenderOfProprietor;
        profile.VerificationToken = token;
        profile.VerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
        profile.UpdatedAt = DateTime.UtcNow;

        if (user.Profile is null) _db.EndUserProfiles.Add(profile);
        else _db.EndUserProfiles.Update(profile);
        await _db.SaveChangesAsync();

        await _email.SendFromTemplateAsync("ENDUSER_PROFILE_VERIFICATION", vm.BusinessEmailAddress, new() { ["FullName"] = vm.FirstName, ["VerificationToken"] = token, ["PortalUrl"] = _config["AppSettings:PortalUrl"] ?? "" });
        await _notifications.CreateAsync(UserId, "Verify your account", $"Verification token sent to {vm.BusinessEmailAddress}. Token: {token}", NotificationType.Info);

        return RedirectToAction("Verify");
    }

    [HttpGet]
    public IActionResult Verify()
    {
        ViewData["Title"] = "Verify Your Account"; ViewData["PreTitle"] = "EndUser Portal";
        return View(new TokenVerifyViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify(TokenVerifyViewModel vm)
    {
        if (!ModelState.IsValid) { ViewData["Title"] = "Verify Your Account"; ViewData["PreTitle"] = "EndUser Portal"; return View(vm); }
        var profile = await _db.EndUserProfiles.FirstOrDefaultAsync(p => p.UserId == UserId);
        if (profile is null || profile.VerificationToken != vm.Token || profile.VerificationTokenExpiry < DateTime.UtcNow)
        { ModelState.AddModelError("", profile?.VerificationTokenExpiry < DateTime.UtcNow ? "Token has expired. Please request a new one." : "Invalid token."); ViewData["Title"] = "Verify Your Account"; ViewData["PreTitle"] = "EndUser Portal"; return View(vm); }
        profile.IsVerified = true; profile.VerificationToken = null; profile.VerificationTokenExpiry = null;
        var user = await _users.GetUserByIdAsync(UserId);
        if (user is not null) { user.IsEmailVerified = true; await _users.UpdateUserAsync(user); }
        await _db.SaveChangesAsync();
        await _notifications.CreateAsync(UserId, "Account Verified", "Your SMElevate account has been verified successfully.", NotificationType.Success);
        await _email.SendFromTemplateAsync("ENDUSER_PROFILE_VERIFIED", profile.BusinessEmailAddress, new() { ["FullName"] = profile.FirstName, ["PortalUrl"] = _config["AppSettings:PortalUrl"] ?? "" });
        TempData["Success"] = "Account verified! You can now submit loan requests.";
        return RedirectToAction("Index", "EndUserDashboard");
    }
}
