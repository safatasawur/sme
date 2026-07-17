using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Areas.EndUser.Controllers;

[Area("EndUser")]
[Authorize(Policy = "EndUserOnly")]
public class EndUserNotificationsController : Controller
{
    private readonly INotificationService _notifications;
    public EndUserNotificationsController(INotificationService n) => _notifications = n;
    private int UserId => int.Parse(User.FindFirst("UserId")!.Value);

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Notifications"; ViewData["PreTitle"] = "EndUser Portal";
        return View(await _notifications.GetForUserAsync(UserId, 50));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id) { await _notifications.MarkReadAsync(id, UserId); return RedirectToAction("Index"); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead() { await _notifications.MarkAllReadAsync(UserId); TempData["Success"] = "All notifications marked as read."; return RedirectToAction("Index"); }

    [HttpGet]
    public async Task<JsonResult> UnreadCount() => Json(new { count = await _notifications.GetUnreadCountAsync(UserId) });
}
