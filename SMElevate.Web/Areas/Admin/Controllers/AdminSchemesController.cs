using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMElevate.Web.Areas.Admin.ViewModels;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminSchemesController : Controller
{
    private readonly ISchemeService _schemes;
    private readonly IAuditService _audit;
    private readonly ISchemeFormConfigService _formConfig;

    public AdminSchemesController(ISchemeService schemes, IAuditService audit, ISchemeFormConfigService formConfig)
    { _schemes = schemes; _audit = audit; _formConfig = formConfig; }

    private void SetNav(string title) { ViewData["ActiveNav"] = "Schemes"; ViewData["PreTitle"] = "SME Scheme Management"; ViewData["Title"] = title; }

    public async Task<IActionResult> Index()
    {
        SetNav("Scheme List");
        var schemes = await _schemes.GetAllSchemesAsync();
        ViewBag.ConfigCounts = await _formConfig.GetConfigCountsAsync(schemes.Select(s => s.Id));
        return View(schemes);
    }

    public IActionResult Create() { SetNav("New Scheme"); return View(new SchemeFormViewModel()); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SchemeFormViewModel vm)
    {
        if (!ModelState.IsValid) { SetNav("New Scheme"); return View(vm); }
        var scheme = new Scheme { SchemeName = vm.SchemeName, SchemeCode = vm.SchemeCode, SchemeDescription = vm.SchemeDescription, EligibilityCriteria = vm.EligibilityCriteria, StartDate = vm.StartDate, EndDate = vm.EndDate, Status = vm.Status, IsPublished = vm.IsPublished };
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim is not null) scheme.CreatedByUserId = int.Parse(userIdClaim.Value);
        await _schemes.CreateSchemeAsync(scheme);
        await _audit.LogAsync("Create", "Scheme", scheme.Id, newValue: scheme.SchemeName);
        TempData["Success"] = "Scheme created successfully.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(int id)
    {
        var scheme = await _schemes.GetByIdAsync(id);
        if (scheme is null) return NotFound();
        SetNav("Edit Scheme");
        return View(new SchemeFormViewModel { Id = scheme.Id, SchemeName = scheme.SchemeName, SchemeCode = scheme.SchemeCode, SchemeDescription = scheme.SchemeDescription, EligibilityCriteria = scheme.EligibilityCriteria, StartDate = scheme.StartDate, EndDate = scheme.EndDate, Status = scheme.Status, IsPublished = scheme.IsPublished, FormId = scheme.FormId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SchemeFormViewModel vm)
    {
        if (!ModelState.IsValid) { SetNav("Edit Scheme"); return View(vm); }
        var scheme = await _schemes.GetByIdAsync(id);
        if (scheme is null) return NotFound();
        scheme.SchemeName = vm.SchemeName; scheme.SchemeCode = vm.SchemeCode; scheme.SchemeDescription = vm.SchemeDescription; scheme.EligibilityCriteria = vm.EligibilityCriteria; scheme.StartDate = vm.StartDate; scheme.EndDate = vm.EndDate; scheme.Status = vm.Status; scheme.IsPublished = vm.IsPublished;
        await _schemes.UpdateSchemeAsync(scheme);
        await _audit.LogAsync("Update", "Scheme", id, newValue: scheme.SchemeName);
        TempData["Success"] = "Scheme updated.";
        return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int id) { await _schemes.PublishAsync(id); await _audit.LogAsync("Publish", "Scheme", id); TempData["Success"] = "Scheme published."; return RedirectToAction("Index"); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unpublish(int id) { await _schemes.UnpublishAsync(id); await _audit.LogAsync("Unpublish", "Scheme", id); TempData["Success"] = "Scheme unpublished."; return RedirectToAction("Index"); }
}
