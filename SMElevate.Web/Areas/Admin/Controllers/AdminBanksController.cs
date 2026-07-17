using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMElevate.Web.Areas.Admin.ViewModels;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminBanksController : Controller
{
    private readonly IBankService _banks;
    private readonly IUserService _users;
    private readonly IAuditService _audit;

    public AdminBanksController(IBankService banks, IUserService users, IAuditService audit)
    { _banks = banks; _users = users; _audit = audit; }

    private void SetNav(string title) { ViewData["ActiveNav"] = "Banks"; ViewData["PreTitle"] = "Bank Management"; ViewData["Title"] = title; }

    public async Task<IActionResult> Index() { SetNav("Banks"); return View(await _banks.GetAllBanksAsync()); }

    public IActionResult Create() { SetNav("New Bank"); return View(new BankFormViewModel { IsActive = true }); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BankFormViewModel vm)
    {
        if (!ModelState.IsValid) { SetNav("New Bank"); return View(vm); }
        var bank = new Bank { BankName = vm.BankName, IBANPrefix = vm.IBANPrefix, BankCode = vm.BankCode, BankEmailAddress = vm.BankEmailAddress, IsActive = vm.IsActive };
        await _banks.CreateBankAsync(bank);
        await _audit.LogAsync("Create", "Bank", bank.Id, newValue: bank.BankName);
        TempData["Success"] = "Bank created.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(int id)
    {
        var bank = await _banks.GetByIdAsync(id);
        if (bank is null) return NotFound();
        SetNav("Edit Bank");
        return View(new BankFormViewModel { Id = bank.Id, BankName = bank.BankName, IBANPrefix = bank.IBANPrefix, BankCode = bank.BankCode, BankEmailAddress = bank.BankEmailAddress, IsActive = bank.IsActive });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BankFormViewModel vm)
    {
        if (!ModelState.IsValid) { SetNav("Edit Bank"); return View(vm); }
        var bank = await _banks.GetByIdAsync(id);
        if (bank is null) return NotFound();
        bank.BankName = vm.BankName; bank.IBANPrefix = vm.IBANPrefix; bank.BankCode = vm.BankCode; bank.BankEmailAddress = vm.BankEmailAddress; bank.IsActive = vm.IsActive;
        await _banks.UpdateBankAsync(bank);
        await _audit.LogAsync("Update", "Bank", id, newValue: bank.BankName);
        TempData["Success"] = "Bank updated.";
        return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        await _banks.ToggleActiveAsync(id);
        await _audit.LogAsync("ToggleActive", "Bank", id);
        TempData["Success"] = "Bank status updated.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Members(int id)
    {
        var bank = await _banks.GetByIdAsync(id);
        if (bank is null) return NotFound();
        SetNav($"Bank Members - {bank.BankName}");
        var allUsers = await _users.GetAllUsersAsync();
        var currentMemberIds = bank.Members.Where(m => m.IsActive).Select(m => m.UserId).ToHashSet();
        ViewBag.Bank = bank;
        ViewBag.AllUsers = allUsers;
        ViewBag.CurrentMemberIds = currentMemberIds;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignMembers(int id, [FromForm] List<int> selectedUserIds)
    {
        if (selectedUserIds.Any())
        {
            await _banks.AssignMembersAsync(id, selectedUserIds);
            await _audit.LogAsync("AssignMembers", "Bank", id, newValue: string.Join(",", selectedUserIds));
        }
        TempData["Success"] = "Bank members updated.";
        return RedirectToAction("Members", new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMembers(int id, [FromForm] List<int> removeUserIds)
    {
        if (removeUserIds.Any())
        {
            await _banks.RemoveMembersAsync(id, removeUserIds);
            await _audit.LogAsync("RemoveMembers", "Bank", id, newValue: string.Join(",", removeUserIds));
        }
        TempData["Success"] = "Selected members removed.";
        return RedirectToAction("Members", new { id });
    }
}
