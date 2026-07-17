using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMElevate.Web.Areas.Admin.ViewModels;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;
using System.Text.Json;

namespace SMElevate.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminFormBuilderController : Controller
{
    private readonly ISchemeService _schemes;
    private readonly ISchemeFormConfigService _formConfig;
    private readonly IAuditService _audit;

    public AdminFormBuilderController(ISchemeService schemes, ISchemeFormConfigService formConfig, IAuditService audit)
    { _schemes = schemes; _formConfig = formConfig; _audit = audit; }

    private void SetNav()
    {
        ViewData["ActiveNav"] = "Schemes";
        ViewData["PreTitle"]  = "SME Scheme Management";
        ViewData["Title"]     = "Form Builder";
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? schemeId)
    {
        SetNav();
        ViewBag.AllSchemes      = await _schemes.GetAllSchemesAsync();
        ViewBag.SelectedSchemeId = schemeId;

        if (!schemeId.HasValue)
            return View(new List<FormFieldConfigRowViewModel>());

        var existingConfigs = await _formConfig.GetConfigsAsync(schemeId.Value);

        var vm = MasterFormTemplate.Fields.Select(mf =>
        {
            var cfg = existingConfigs.FirstOrDefault(c => c.FieldName == mf.FieldName);
            return new FormFieldConfigRowViewModel
            {
                SectionName             = mf.SectionName,
                SectionOrder            = mf.SectionOrder,
                FieldName               = mf.FieldName,
                FieldLabel              = mf.FieldLabel,
                FieldType               = mf.FieldType,
                LookupKey               = mf.LookupKey,
                DisplayOrder            = mf.DisplayOrder,
                IsAvailable             = cfg?.IsAvailable             ?? true,
                IsRequired              = cfg?.IsRequired              ?? mf.DefaultRequired,
                HasConditionalVisibility = cfg?.HasConditionalVisibility ?? false,
                ConditionalExpression   = cfg?.ConditionalExpression
            };
        }).ToList();

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([FromForm] int schemeId, [FromForm] string configsJson)
    {
        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var dtos = JsonSerializer.Deserialize<List<SchemeFormFieldConfigSaveDto>>(configsJson, opts) ?? new();

        await _formConfig.SaveConfigsAsync(schemeId, dtos);
        await _audit.LogAsync("SaveFormConfig", "SchemeFormFieldConfig", schemeId,
            newValue: $"Scheme {schemeId} form builder saved");

        TempData["Success"] = "Form configuration saved.";
        return RedirectToAction("Index", new { schemeId });
    }
}
