using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Data;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminLoanApplicationsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWorkflowService _workflow;
    private readonly IBankService _banks;
    private readonly INotificationService _notifications;
    private readonly IAuditService _audit;

    public AdminLoanApplicationsController(ApplicationDbContext db, IWorkflowService workflow,
        IBankService banks, INotificationService notifications, IAuditService audit)
    { _db = db; _workflow = workflow; _banks = banks; _notifications = notifications; _audit = audit; }

    private int UserId => int.TryParse(User.FindFirst("UserId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> Index(string? status, int? bankId, string? search,
        DateTime? from, DateTime? to, int page = 1, int pageSize = 20)
    {
        ViewData["Title"] = "Loan Applications"; ViewData["PreTitle"] = "Admin Portal"; ViewData["ActiveNav"] = "LoanApplications";

        var query = _db.LoanRequests
            .AsNoTracking()
            .Include(r => r.Status)
            .Include(r => r.User)
            .Include(r => r.AssignedBank)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.Status!.ValueText == status);
        if (bankId.HasValue)
            query = query.Where(r => r.AssignedBankId == bankId.Value);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.CaseId!.Contains(search) || r.RequestNo.Contains(search)
                || r.NameOfBusiness.Contains(search) || r.User.EmailAddress.Contains(search));
        if (from.HasValue)
            query = query.Where(r => r.CreatedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(r => r.CreatedAt <= to.Value.AddDays(1));

        var total = await query.CountAsync();
        var requests = await query.OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.StatusList = await _workflow.GetAllStatusesAsync();
        ViewBag.Banks = await _banks.GetAllBanksAsync(activeOnly: true);
        ViewBag.Total = total; ViewBag.Page = page; ViewBag.PageSize = pageSize;
        ViewBag.SearchParams = new { status, bankId, search, from, to };
        return View(requests);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var request = await _db.LoanRequests
            .Include(r => r.Status).Include(r => r.User).Include(r => r.AssignedBank)
            .Include(r => r.Scheme).Include(r => r.Shareholders).Include(r => r.FieldValues)
            .Include(r => r.StatusHistory.OrderBy(h => h.CreatedAt))
                .ThenInclude(h => h.NewStatus)
            .Include(r => r.StatusHistory).ThenInclude(h => h.ChangedBy)
            .Include(r => r.BankDecision).ThenInclude(d => d!.DeclineReasonCode)
            .Include(r => r.BankAssessments)
            .Include(r => r.AdditionalInfoRequests)
            .Include(r => r.ConditionalOffers)
            .Include(r => r.Disbursement)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null) return NotFound();
        ViewData["Title"] = $"Application {request.CaseId ?? request.RequestNo}";
        ViewData["PreTitle"] = "Admin Portal"; ViewData["ActiveNav"] = "LoanApplications";

        ViewBag.AllowedTransitions = await _workflow.GetAllowedTransitionsAsync(request.Status?.ValueText ?? "", "Admin");
        ViewBag.Banks = await _banks.GetAllBanksAsync(activeOnly: true);
        return View(request);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AdvanceStatus(int loanRequestId, string toStatus, string? remarks)
    {
        var request = await _db.LoanRequests.FirstOrDefaultAsync(r => r.Id == loanRequestId);
        if (request == null) return NotFound();

        try
        {
            await _workflow.AdvanceStatusAsync(loanRequestId, toStatus, UserId, "Admin", remarks,
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());
            await _audit.LogAsync("StatusAdvanced", "LoanRequest", loanRequestId, newValue: toStatus, userId: UserId);
            TempData["Success"] = $"Status advanced to '{toStatus}'.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction("Detail", new { id = loanRequestId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignBank(int loanRequestId, int newBankId, string? remarks)
    {
        var request = await _db.LoanRequests.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == loanRequestId);
        if (request == null) return NotFound();

        var oldBankId = request.AssignedBankId;
        request.AssignedBankId = newBankId;
        request.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _audit.LogAsync("BankAssigned", "LoanRequest", loanRequestId,
            oldValue: oldBankId?.ToString(), newValue: newBankId.ToString(), userId: UserId);

        // Advance to ReferredToBank if in Validated state
        try
        {
            await _workflow.AdvanceStatusAsync(loanRequestId, AppStatus.ReferredToBank, UserId, "Admin",
                remarks ?? $"Assigned to bank ID {newBankId}");
        }
        catch { /* May already be past this status */ }

        await _notifications.CreateAsync(request.UserId, "Application Referred to Bank",
            $"Your application {request.CaseId ?? request.RequestNo} has been referred to a bank for assessment.",
            NotificationType.Info, "LoanRequest", loanRequestId);

        TempData["Success"] = "Application assigned to bank.";
        return RedirectToAction("Detail", new { id = loanRequestId });
    }
}
