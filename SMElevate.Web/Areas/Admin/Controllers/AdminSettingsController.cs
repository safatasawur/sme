using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMElevate.Web.Areas.Admin.ViewModels;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminSettingsController : Controller
{
    private readonly ISettingsService _settings;
    private readonly IAuditService _audit;

    public AdminSettingsController(ISettingsService settings, IAuditService audit) { _settings = settings; _audit = audit; }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveNav"] = "Settings"; ViewData["PreTitle"] = "Settings Management"; ViewData["Title"] = "Settings";
        var vm = new SettingsViewModel
        {
            Email = await _settings.GetCategoryAsync(SettingCategory.Email),
            Notification = await _settings.GetCategoryAsync(SettingCategory.Notification),
            OAuth = await _settings.GetCategoryAsync(SettingCategory.OAuth),
            General = await _settings.GetCategoryAsync(SettingCategory.General),
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(SettingsViewModel vm, [FromForm] string tab)
    {
        var category = tab switch
        {
            "email" => SettingCategory.Email,
            "notification" => SettingCategory.Notification,
            "oauth" => SettingCategory.OAuth,
            _ => SettingCategory.General
        };
        var values = tab switch
        {
            "email" => vm.Email,
            "notification" => vm.Notification,
            "oauth" => vm.OAuth,
            _ => vm.General
        };
        await _settings.SaveCategoryAsync(category, values);
        await _audit.LogAsync("SaveSettings", "SystemSetting", null, newValue: tab);
        TempData["Success"] = "Settings saved successfully.";
        return RedirectToAction("Index");
    }
}
