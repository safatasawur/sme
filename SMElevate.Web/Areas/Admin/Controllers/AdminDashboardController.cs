using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Areas.Admin.ViewModels;
using SMElevate.Web.Data;
using SMElevate.Web.Models.Common;

namespace SMElevate.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminDashboardController : Controller
{
    private readonly ApplicationDbContext _db;

    public AdminDashboardController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Dashboard";
        ViewData["PreTitle"] = "SMElevate Admin Portal";
        ViewData["ActiveNav"] = "Dashboard";

        var vm = new AdminDashboardViewModel();

        // Scalar counts — no full entity load
        vm.TotalSmesRegistered = await _db.Users.AsNoTracking()
            .CountAsync(u => u.UserType == UserType.SME);

        vm.TotalApplications = await _db.LoanRequests.AsNoTracking().CountAsync();

        vm.ParticipatingBanks = await _db.Banks.AsNoTracking()
            .CountAsync(b => b.IsActive);

        vm.ActiveSchemes = await _db.Schemes.AsNoTracking()
            .CountAsync(s => s.IsPublished);

        // Status text projection — EF generates LEFT JOIN on MasterLookupValues
        var statusTexts = await _db.LoanRequests
            .AsNoTracking()
            .Select(r => r.Status == null ? null : r.Status.ValueText)
            .ToListAsync();

        vm.InProcessApplications = statusTexts.Count(s =>
            s == AppStatus.InProcess || s == AppStatus.MoreInformationRequired ||
            s == AppStatus.Assigned || s == AppStatus.Submitted ||
            s == AppStatus.ReferredToBank || s == AppStatus.UnderCreditBureauCheck ||
            s == AppStatus.UnderRiskAssessment || s == AppStatus.UnderCDDComplianceReview ||
            s == AppStatus.AdditionalInformationRequired || s == AppStatus.UnderBankDecision);
        vm.CompletedApplications = statusTexts.Count(s => s == AppStatus.Completed || s == AppStatus.Closed);
        vm.ApprovedApplications  = statusTexts.Count(s => s == AppStatus.ConditionallyApproved ||
            s == AppStatus.OfferAccepted || s == AppStatus.Disbursed || s == AppStatus.Approved);
        vm.RejectedApplications  = statusTexts.Count(s => s == AppStatus.Declined || s == AppStatus.Rejected);

        // V2.2 extended metrics
        vm.OffersIssued   = statusTexts.Count(s => s == AppStatus.OfferIssued);
        vm.OffersAccepted = statusTexts.Count(s => s == AppStatus.OfferAccepted);
        vm.Disbursed      = statusTexts.Count(s => s == AppStatus.Disbursed);
        vm.Referred       = statusTexts.Count(s => s == AppStatus.ReferredToBank ||
            s == AppStatus.UnderCreditBureauCheck || s == AppStatus.UnderRiskAssessment ||
            s == AppStatus.UnderCDDComplianceReview);

        // Application Status donut chart
        var statusGroups = statusTexts
            .Where(s => !string.IsNullOrEmpty(s))
            .GroupBy(s => s!)
            .OrderByDescending(g => g.Count())
            .ToList();
        vm.StatusLabels = statusGroups.Select(g => g.Key).ToList();
        vm.StatusCounts = statusGroups.Select(g => g.Count()).ToList();

        // SME Registration Trend — last 6 complete months
        var trendStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddMonths(-5);
        var monthlyReg = await _db.Users
            .AsNoTracking()
            .Where(u => u.UserType == UserType.SME && u.CreatedAt >= trendStart)
            .Select(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
            .ToListAsync();

        for (int i = 0; i < 6; i++)
        {
            var m = trendStart.AddMonths(i);
            vm.MonthlyLabels.Add(m.ToString("MMM yyyy"));
            vm.MonthlyCounts.Add(monthlyReg.Count(r => r.Year == m.Year && r.Month == m.Month));
        }

        // Bank-wise bar chart — LEFT JOIN, group in DB
        var bankWise = await (
            from r in _db.LoanRequests.AsNoTracking()
            join b in _db.Banks on r.AssignedBankId equals b.Id into banks
            from bank in banks.DefaultIfEmpty()
            group r by (bank == null ? "Unassigned" : bank.BankName) into g
            select new { Label = g.Key, Count = g.Count() }
        ).OrderByDescending(x => x.Count).ToListAsync();

        vm.BankWiseLabels = bankWise.Select(x => x.Label).ToList();
        vm.BankWiseCounts = bankWise.Select(x => x.Count).ToList();

        // Scheme-wise chart — FacilityRequested used as grouping (no SchemeId FK on LoanRequest)
        var schemeWise = await _db.LoanRequests
            .AsNoTracking()
            .Where(r => r.FacilityRequested != null && r.FacilityRequested != "")
            .GroupBy(r => r.FacilityRequested)
            .Select(g => new { Label = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        vm.SchemeWiseLabels = schemeWise.Select(x => x.Label).ToList();
        vm.SchemeWiseCounts = schemeWise.Select(x => x.Count).ToList();

        // Recent applications — projected, no full entity load
        vm.RecentApplications = await _db.LoanRequests
            .AsNoTracking()
            .OrderByDescending(r => r.SubmittedAt)
            .Take(10)
            .Select(r => new RecentApplicationDto
            {
                RequestNo    = r.RequestNo,
                CaseId       = r.CaseId,
                BusinessName = r.NameOfBusiness,
                BankName     = r.AssignedBank == null ? null : r.AssignedBank.BankName,
                Amount       = r.Amount,
                Status       = r.Status == null ? null : r.Status.ValueText,
                SubmittedAt  = r.SubmittedAt
            })
            .ToListAsync();

        return View(vm);
    }
}
