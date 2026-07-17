using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMElevate.Web.Areas.Admin.ViewModels;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminLookupsController : Controller
{
    private readonly ILookupService _lookups;
    private readonly IAuditService _audit;

    public AdminLookupsController(ILookupService lookups, IAuditService audit) { _lookups = lookups; _audit = audit; }
    private void SetNav(string t) { ViewData["ActiveNav"] = "Lookups"; ViewData["PreTitle"] = "Master Lookup Field Management"; ViewData["Title"] = t; }

    public async Task<IActionResult> Index() { SetNav("Master Lookup Fields"); return View(await _lookups.GetAllLookupsAsync()); }

    public IActionResult Create() { SetNav("New Lookup"); return View(new LookupFormViewModel()); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LookupFormViewModel vm, [FromForm] List<string> valueTexts)
    {
        if (!ModelState.IsValid) { SetNav("New Lookup"); return View(vm); }
        var lookup = new MasterLookup { LookupName = vm.LookupName, LookupCode = vm.LookupCode, Description = vm.Description, IsActive = vm.IsActive };
        await _lookups.CreateLookupAsync(lookup);
        for (int i = 0; i < valueTexts.Count; i++)
            if (!string.IsNullOrWhiteSpace(valueTexts[i]))
                await _lookups.SaveValueAsync(new MasterLookupValue { MasterLookupId = lookup.Id, ValueText = valueTexts[i], ValueCode = valueTexts[i].Replace(" ","_").ToUpper(), DisplayOrder = i, IsActive = true });
        await _audit.LogAsync("Create", "MasterLookup", lookup.Id, newValue: lookup.LookupCode);
        TempData["Success"] = "Lookup created."; return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(int id)
    {
        var lookup = await _lookups.GetByIdAsync(id);
        if (lookup is null) return NotFound();
        SetNav("Edit Lookup");
        return View(new LookupFormViewModel { Id = lookup.Id, LookupName = lookup.LookupName, LookupCode = lookup.LookupCode, Description = lookup.Description, IsActive = lookup.IsActive, Values = lookup.Values.OrderBy(v => v.DisplayOrder).ToList() });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LookupFormViewModel vm, [FromForm] List<string> valueTexts)
    {
        if (!ModelState.IsValid) { SetNav("Edit Lookup"); return View(vm); }
        var lookup = await _lookups.GetByIdAsync(id);
        if (lookup is null) return NotFound();
        lookup.LookupName = vm.LookupName; lookup.Description = vm.Description; lookup.IsActive = vm.IsActive;
        await _lookups.UpdateLookupAsync(lookup);
        var existingValues = lookup.Values.ToList();
        for (int i = 0; i < valueTexts.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(valueTexts[i]))
            {
                if (i < existingValues.Count) { existingValues[i].ValueText = valueTexts[i]; await _lookups.SaveValueAsync(existingValues[i]); }
                else await _lookups.SaveValueAsync(new MasterLookupValue { MasterLookupId = id, ValueText = valueTexts[i], ValueCode = valueTexts[i].Replace(" ","_").ToUpper(), DisplayOrder = i, IsActive = true });
            }
        }
        await _audit.LogAsync("Update", "MasterLookup", id, newValue: lookup.LookupCode);
        TempData["Success"] = "Lookup updated."; return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id) { await _lookups.ToggleLookupActiveAsync(id); TempData["Success"] = "Lookup status updated."; return RedirectToAction("Index"); }
}
