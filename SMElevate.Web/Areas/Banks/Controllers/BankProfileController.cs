using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Areas.Banks.Controllers;

[Area("Banks")]
[Authorize(Policy = "BankOnly")]
public class BankProfileController : Controller
{
    private readonly IUserService _users;
    private readonly IBankService _banks;

    public BankProfileController(IUserService users, IBankService banks) { _users = users; _banks = banks; }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Bank Profile"; ViewData["PreTitle"] = "Bank Profile"; ViewData["ActiveNav"] = "Profile";
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim is null) return RedirectToAction("Login", "BankAuth");
        var user = await _users.GetUserByIdAsync(int.Parse(userIdClaim.Value));
        if (user is null) return RedirectToAction("Login", "BankAuth");
        var bank = user.BankId.HasValue ? await _banks.GetByIdAsync(user.BankId.Value) : null;
        ViewBag.Bank = bank;
        ViewBag.User = user;
        return View();
    }
}
