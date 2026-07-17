using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Data;
using SMElevate.Web.Models.Common;

namespace SMElevate.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminWorkflowController : Controller
{
    private readonly ApplicationDbContext _db;

    public AdminWorkflowController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Statuses()
    {
        ViewData["Title"] = "Workflow Statuses"; ViewData["PreTitle"] = "Workflow Configuration"; ViewData["ActiveNav"] = "Workflow";
        return View(await _db.WorkflowStatuses.OrderBy(w => w.DisplayOrder).ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Transitions()
    {
        ViewData["Title"] = "Workflow Transitions"; ViewData["PreTitle"] = "Workflow Configuration"; ViewData["ActiveNav"] = "Workflow";
        return View(await _db.WorkflowTransitions.OrderBy(t => t.DisplayOrder).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleTransition(int id)
    {
        var t = await _db.WorkflowTransitions.FindAsync(id);
        if (t == null) return NotFound();
        t.IsActive = !t.IsActive;
        await _db.SaveChangesAsync();
        return Json(new { isActive = t.IsActive });
    }

    [HttpGet]
    public async Task<IActionResult> DeclineReasonCodes()
    {
        ViewData["Title"] = "Decline Reason Codes"; ViewData["PreTitle"] = "Workflow Configuration"; ViewData["ActiveNav"] = "Workflow";
        return View(await _db.DeclineReasonCodes.OrderBy(d => d.Category).ThenBy(d => d.DisplayOrder).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveDeclineReasonCode(DeclineReasonCode model)
    {
        if (model.Id == 0)
        {
            _db.DeclineReasonCodes.Add(model);
        }
        else
        {
            var existing = await _db.DeclineReasonCodes.FindAsync(model.Id);
            if (existing == null) return NotFound();
            existing.Code = model.Code; existing.Description = model.Description;
            existing.Category = model.Category; existing.IsActive = model.IsActive;
        }
        await _db.SaveChangesAsync();
        TempData["Success"] = "Reason code saved.";
        return RedirectToAction("DeclineReasonCodes");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleDeclineReasonCode(int id)
    {
        var code = await _db.DeclineReasonCodes.FindAsync(id);
        if (code == null) return NotFound();
        code.IsActive = !code.IsActive;
        await _db.SaveChangesAsync();
        return Json(new { isActive = code.IsActive });
    }
}
