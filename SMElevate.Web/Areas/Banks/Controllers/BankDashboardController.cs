using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Areas.Banks.ViewModels;
using SMElevate.Web.Data;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Areas.Banks.Controllers;

[Area("Banks")]
[Authorize(Policy = "BankOnly")]
public class BankDashboardController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILoanRequestService _requests;

    public BankDashboardController(ApplicationDbContext db, ILoanRequestService requests)
    { _db = db; _requests = requests; }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Dashboard"; ViewData["PreTitle"] = "SMElevate Banks Portal"; ViewData["ActiveNav"] = "Dashboard";
        var bankIdClaim = User.FindFirst("BankId");
        if (bankIdClaim is null || !int.TryParse(bankIdClaim.Value, out var bankId))
            return RedirectToAction("Login", "BankAuth");

        var statuses = await _db.LoanRequests
            .AsNoTracking()
            .Include(r => r.Status)
            .Where(r => r.AssignedBankId == bankId)
            .Select(r => new { r.Id, Status = r.Status == null ? null : r.Status.ValueText })
            .ToListAsync();

        var infoCount = await _db.AdditionalInfoRequests
            .Where(r => r.LoanRequest.AssignedBankId == bankId && r.Status == AdditionalInfoStatus.Pending)
            .CountAsync();
        var offerCount = await _db.ConditionalOffers
            .Where(o => o.LoanRequest.AssignedBankId == bankId && o.Status == OfferStatus.Issued)
            .CountAsync();

        var recent = await _db.LoanRequests
            .AsNoTracking()
            .Include(r => r.Status)
            .Include(r => r.User)
            .Where(r => r.AssignedBankId == bankId)
            .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
            .Take(5)
            .ToListAsync();

        var reviewStatuses = new[]
        {
            AppStatus.UnderCreditBureauCheck, AppStatus.UnderRiskAssessment,
            AppStatus.UnderCDDComplianceReview, AppStatus.UnderBankDecision
        };

        var vm = new BankDashboardViewModel
        {
            AssignedApplications = statuses.Count,
            NewReferrals = statuses.Count(s => s.Status == AppStatus.ReferredToBank),
            UnderReview = statuses.Count(s => s.Status != null && reviewStatuses.Contains(s.Status)),
            AdditionalInfoPending = infoCount,
            ConditionalOffers = offerCount,
            Approved = statuses.Count(s => s.Status == AppStatus.ConditionallyApproved || s.Status == AppStatus.OfferAccepted),
            Declined = statuses.Count(s => s.Status == AppStatus.Declined),
            ReadyForDisbursement = statuses.Count(s => s.Status == AppStatus.ReadyForDisbursement),
            Disbursed = statuses.Count(s => s.Status == AppStatus.Disbursed || s.Status == AppStatus.Completed),
            RecentApplications = recent
        };

        return View(vm);
    }
}
