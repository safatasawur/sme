using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminReportsController : Controller
{
    private readonly IReportService _reports;
    private readonly IBankService _banks;

    public AdminReportsController(IReportService reports, IBankService banks)
    { _reports = reports; _banks = banks; }

    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Reports"; ViewData["PreTitle"] = "Admin Portal"; ViewData["ActiveNav"] = "Reports";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> ApplicationStatus(DateTime? from, DateTime? to, int? bankId)
    {
        ViewData["Title"] = "Application Status Report"; ViewData["PreTitle"] = "Reports"; ViewData["ActiveNav"] = "Reports";
        ViewBag.Banks = await _banks.GetAllBanksAsync();
        ViewBag.From = from; ViewBag.To = to; ViewBag.BankId = bankId;
        var result = await _reports.GetApplicationStatusReportAsync(from, to, bankId);
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> TurnaroundTime(DateTime? from, DateTime? to, int? bankId)
    {
        ViewData["Title"] = "Turnaround Time Report"; ViewData["PreTitle"] = "Reports"; ViewData["ActiveNav"] = "Reports";
        ViewBag.Banks = await _banks.GetAllBanksAsync();
        ViewBag.From = from; ViewBag.To = to; ViewBag.BankId = bankId;
        return View(await _reports.GetTurnaroundTimeReportAsync(from, to, bankId));
    }

    [HttpGet]
    public async Task<IActionResult> AssessmentReferral(DateTime? from, DateTime? to, int? bankId)
    {
        ViewData["Title"] = "Assessment & Referral Report"; ViewData["PreTitle"] = "Reports"; ViewData["ActiveNav"] = "Reports";
        ViewBag.Banks = await _banks.GetAllBanksAsync();
        ViewBag.From = from; ViewBag.To = to; ViewBag.BankId = bankId;
        return View(await _reports.GetAssessmentReferralReportAsync(from, to, bankId));
    }

    [HttpGet]
    public async Task<IActionResult> DeclineAnalysis(DateTime? from, DateTime? to, int? bankId)
    {
        ViewData["Title"] = "Decline Analysis"; ViewData["PreTitle"] = "Reports"; ViewData["ActiveNav"] = "Reports";
        ViewBag.Banks = await _banks.GetAllBanksAsync();
        ViewBag.From = from; ViewBag.To = to; ViewBag.BankId = bankId;
        return View(await _reports.GetDeclineAnalysisAsync(from, to, bankId));
    }

    [HttpGet]
    public async Task<IActionResult> DisbursementSummary(DateTime? from, DateTime? to, int? bankId)
    {
        ViewData["Title"] = "Disbursement Summary"; ViewData["PreTitle"] = "Reports"; ViewData["ActiveNav"] = "Reports";
        ViewBag.Banks = await _banks.GetAllBanksAsync();
        ViewBag.From = from; ViewBag.To = to; ViewBag.BankId = bankId;
        return View(await _reports.GetDisbursementSummaryAsync(from, to, bankId));
    }

    [HttpGet]
    public async Task<IActionResult> GeographicSpread(DateTime? from, DateTime? to)
    {
        ViewData["Title"] = "Geographic Distribution"; ViewData["PreTitle"] = "Reports"; ViewData["ActiveNav"] = "Reports";
        ViewBag.From = from; ViewBag.To = to;
        return View(await _reports.GetGeographicSpreadAsync(from, to));
    }

    [HttpGet]
    public async Task<IActionResult> BankPerformance(DateTime? from, DateTime? to)
    {
        ViewData["Title"] = "Bank Performance Report"; ViewData["PreTitle"] = "Reports"; ViewData["ActiveNav"] = "Reports";
        ViewBag.From = from; ViewBag.To = to;
        return View(await _reports.GetBankPerformanceReportAsync(from, to));
    }
}
