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
public class BankDisbursementController : Controller
{
    private readonly IDisbursementService _disbursement;
    private readonly IWorkflowService _workflow;
    private readonly INotificationService _notifications;
    private readonly IEmailService _email;
    private readonly IAuditService _audit;
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;

    public BankDisbursementController(IDisbursementService disbursement, IWorkflowService workflow,
        INotificationService notifications, IEmailService email, IAuditService audit,
        ApplicationDbContext db, IConfiguration config)
    { _disbursement = disbursement; _workflow = workflow; _notifications = notifications;
      _email = email; _audit = audit; _db = db; _config = config; }

    private int BankId => int.TryParse(User.FindFirst("BankId")?.Value, out var id) ? id : 0;
    private int UserId => int.TryParse(User.FindFirst("UserId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> Index(int loanRequestId)
    {
        var request = await _db.LoanRequests.Include(r => r.Status).Include(r => r.BankDecision)
            .FirstOrDefaultAsync(r => r.Id == loanRequestId && r.AssignedBankId == BankId);
        if (request == null) return NotFound();

        ViewData["Title"] = "Disbursement"; ViewData["PreTitle"] = "Disbursement Tracking"; ViewData["ActiveNav"] = "Requests";
        ViewBag.Request = request;
        ViewBag.Existing = await _disbursement.GetByLoanRequestAsync(loanRequestId);

        var vm = new RecordDisbursementViewModel
        {
            LoanRequestId = loanRequestId,
            ApprovedAmount = request.BankDecision?.ApprovedAmount ?? request.Amount,
            DisbursedAmount = request.BankDecision?.ApprovedAmount ?? request.Amount,
            DisbursementAccount = request.IBANOrRaastValue
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Record(RecordDisbursementViewModel vm)
    {
        var request = await _db.LoanRequests.Include(r => r.User).Include(r => r.AssignedBank)
            .FirstOrDefaultAsync(r => r.Id == vm.LoanRequestId && r.AssignedBankId == BankId);
        if (request == null) return NotFound();

        if (vm.DisbursedAmount > vm.ApprovedAmount)
        { TempData["Error"] = "Disbursed amount cannot exceed approved amount."; return RedirectToAction("Index", new { loanRequestId = vm.LoanRequestId }); }

        var disbursement = new Disbursement
        {
            LoanRequestId = vm.LoanRequestId, DisbursementStatus = vm.DisbursementStatus,
            ApprovedAmount = vm.ApprovedAmount, DisbursedAmount = vm.DisbursedAmount,
            ValueDate = vm.ValueDate, DisbursementAccount = vm.DisbursementAccount,
            BankReferenceNumber = vm.BankReferenceNumber, Remarks = vm.Remarks, UpdatedByUserId = UserId
        };
        await _disbursement.CreateOrUpdateAsync(disbursement);

        await _workflow.AdvanceStatusAsync(vm.LoanRequestId, AppStatus.Disbursed, UserId, "Bank",
            $"Disbursed: {vm.DisbursedAmount:N0}");

        await _notifications.CreateAsync(request.UserId, "Disbursement Recorded",
            $"Disbursement has been recorded for your application {request.CaseId ?? request.RequestNo}.",
            NotificationType.Success, "Disbursement", disbursement.Id);

        await _email.SendFromTemplateAsync("ENDUSER_DISBURSED", request.User.EmailAddress, new()
        {
            ["FullName"] = request.User.FullName,
            ["CaseId"] = request.CaseId ?? request.RequestNo,
            ["DisbursedAmount"] = $"PKR {vm.DisbursedAmount:N0}",
            ["ValueDate"] = vm.ValueDate?.ToString("yyyy-MM-dd") ?? "N/A",
            ["PortalUrl"] = _config["AppSettings:PortalUrl"] ?? ""
        });

        await _audit.LogAsync("DisbursementRecorded", "Disbursement", disbursement.Id,
            newValue: $"{vm.DisbursedAmount}", userId: UserId);

        TempData["Success"] = "Disbursement recorded successfully.";
        return RedirectToAction("Detail", "BankRequests", new { id = vm.LoanRequestId });
    }
}
