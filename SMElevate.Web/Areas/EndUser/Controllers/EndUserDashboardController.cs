using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Data;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Areas.EndUser.Controllers;

[Area("EndUser")]
[Authorize(Policy = "EndUserOnly")]
public class EndUserDashboardController : Controller
{
    private readonly ILoanRequestService _requests;
    private readonly INotificationService _notifications;
    private readonly ApplicationDbContext _db;

    public EndUserDashboardController(ILoanRequestService requests, INotificationService notifications, ApplicationDbContext db)
    { _requests = requests; _notifications = notifications; _db = db; }

    private int UserId => int.Parse(User.FindFirst("UserId")!.Value);

    public async Task<IActionResult> Index()
    {
        // Guard: profile not completed → redirect to profile (matches HTML routeToCurrentStep)
        var profile = await _db.EndUserProfiles.FirstOrDefaultAsync(p => p.UserId == UserId);
        if (profile is null)
            return RedirectToAction("Complete", "EndUserProfile");
        if (!profile.IsVerified)
            return RedirectToAction("Verify", "EndUserProfile");

        ViewData["Title"] = "Dashboard"; ViewData["PreTitle"] = "EndUser Portal"; ViewData["ActiveNav"] = "Dashboard";

        var requests = await _db.LoanRequests
            .AsNoTracking()
            .Include(r => r.Status)
            .Where(r => r.UserId == UserId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var statuses = requests.Select(r => r.Status?.ValueText).ToList();
        var activeOfferLoanIds = await _db.ConditionalOffers
            .Where(o => o.LoanRequest.UserId == UserId && o.Status == OfferStatus.Issued && o.ExpiryDate >= DateTime.UtcNow)
            .Select(o => o.LoanRequestId).ToListAsync();

        var pendingInfoCount = await _db.AdditionalInfoRequests
            .Where(r => r.LoanRequest.UserId == UserId && r.Status == AdditionalInfoStatus.Pending)
            .CountAsync();

        var vm = new SMElevate.Web.Areas.EndUser.ViewModels.EndUserDashboardViewModel
        {
            TotalApplications = requests.Count,
            DraftApplications = requests.Count(r => r.IsDraft || r.Status?.ValueText == AppStatus.Draft),
            SubmittedApplications = requests.Count(r => !r.IsDraft && r.SubmittedAt != null),
            PendingActions = pendingInfoCount + activeOfferLoanIds.Count,
            ActiveOffers = activeOfferLoanIds.Count,
            DisbursedApplications = statuses.Count(s => s == AppStatus.Disbursed || s == AppStatus.Completed),
            RecentApplications = requests.Take(5).ToList(),
            RecentNotifications = await _notifications.GetForUserAsync(UserId, 5),
            UnreadNotifications = await _notifications.GetUnreadCountAsync(UserId)
        };

        return View(vm);
    }
}
