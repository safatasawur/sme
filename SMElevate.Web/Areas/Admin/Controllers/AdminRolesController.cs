using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminRolesController : Controller
{
    private readonly IRoleService _roles;
    private readonly IAuditService _audit;

    public AdminRolesController(IRoleService roles, IAuditService audit) { _roles = roles; _audit = audit; }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveNav"] = "Roles"; ViewData["PreTitle"] = "Role Management"; ViewData["Title"] = "Roles";
        return View(await _roles.GetAllRolesAsync());
    }

    public IActionResult Create()
    {
        ViewData["ActiveNav"] = "Roles"; ViewData["PreTitle"] = "Role Management"; ViewData["Title"] = "New Role";
        return View(new Role());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Role role)
    {
        if (!ModelState.IsValid) { ViewData["ActiveNav"] = "Roles"; ViewData["PreTitle"] = "Role Management"; ViewData["Title"] = "New Role"; return View(role); }
        await _roles.CreateRoleAsync(role);
        await _audit.LogAsync("Create", "Role", role.Id, newValue: role.RoleName);
        TempData["Success"] = "Role created.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(int id)
    {
        var role = await _roles.GetByIdAsync(id);
        if (role is null) return NotFound();
        ViewData["ActiveNav"] = "Roles"; ViewData["PreTitle"] = "Role Management"; ViewData["Title"] = "Edit Role";
        return View(role);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Role role)
    {
        if (!ModelState.IsValid) { ViewData["ActiveNav"] = "Roles"; ViewData["PreTitle"] = "Role Management"; ViewData["Title"] = "Edit Role"; return View(role); }
        role.Id = id;
        await _roles.UpdateRoleAsync(role);
        await _audit.LogAsync("Update", "Role", id, newValue: role.RoleName);
        TempData["Success"] = "Role updated.";
        return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var newState = await _roles.ToggleActiveAsync(id);
        await _audit.LogAsync(newState ? "Activate" : "Deactivate", "Role", id);
        TempData["Success"] = newState ? "Role activated." : "Role deactivated.";
        return RedirectToAction("Index");
    }
}
