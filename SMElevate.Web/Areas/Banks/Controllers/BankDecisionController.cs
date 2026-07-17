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
public class BankDecisionController : Controller
{
    private readonly IBankDecisionService _decisions;
    private readonly IWorkflowService _workflow;
    private readonly INotificationService _notifications;
    private readonly IEmailService _email;
    private readonly IAuditService _audit;
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;

    public BankDecisionController(IBankDecisionService decisions, IWorkflowService workflow,
        INotificationService notifications, IEmailService email, IAuditService audit,
        ApplicationDbContext db, IConfiguration config)
    { _decisions = decisions; _workflow = workflow; _notifications = notifications;
      _email = email; _audit = audit; _db = db; _config = config; }

    private int BankId => int.TryParse(User.FindFirst("BankId")?.Value, out var id) ? id : 0;
    private int UserId => int.TryParse(User.FindFirst("UserId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> Index(int loanRequestId)
    {
        var request = await _db.LoanRequests.Include(r => r.Status).Include(r => r.User)
            .Include(r => r.AssignedBank)
            .FirstOrDefaultAsync(r => r.Id == loanRequestId && r.AssignedBankId == BankId);
        if (request == null) return NotFound();

        ViewData["Title"] = $"Decision — {request.CaseId ?? request.RequestNo}";
        ViewData["PreTitle"] = "Bank Decision"; ViewData["ActiveNav"] = "Requests";

        var vm = new BankFormalDecisionViewModel
        {
            LoanRequestId = loanRequestId,
            ReasonCodes = await _decisions.GetActiveReasonCodesAsync()
        };
        var existing = await _decisions.GetByLoanRequestAsync(loanRequestId);
        if (existing != null)
        {
            vm.DecisionType = existing.DecisionType; vm.DecisionRemarks = existing.DecisionRemarks;
            vm.DeclineReasonCodeId = existing.DeclineReasonCodeId;
            vm.ApprovedFacilityType = existing.ApprovedFacilityType;
            vm.ApprovedAmount = existing.ApprovedAmount; vm.ApprovedTenorMonths = existing.ApprovedTenorMonths;
            vm.AdditionalConditions = existing.AdditionalConditions;
        }
        ViewBag.Request = request;
        ViewBag.ExistingDecision = existing;
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Record(BankFormalDecisionViewModel vm)
    {
        var request = await _db.LoanRequests.Include(r => r.User).Include(r => r.AssignedBank)
            .FirstOrDefaultAsync(r => r.Id == vm.LoanRequestId && r.AssignedBankId == BankId);
        if (request == null) return NotFound();

        if (vm.DecisionType == DecisionType.Declined && vm.DeclineReasonCodeId == null)
        {
            TempData["Error"] = "A decline reason code is required.";
            return RedirectToAction("Index", new { loanRequestId = vm.LoanRequestId });
        }

        var decision = new BankDecision
        {
            LoanRequestId = vm.LoanRequestId, DecisionType = vm.DecisionType,
            DecisionDate = DateTime.UtcNow, DecisionRemarks = vm.DecisionRemarks,
            DeclineReasonCodeId = vm.DeclineReasonCodeId, ApprovedFacilityType = vm.ApprovedFacilityType,
            ApprovedAmount = vm.ApprovedAmount ?? 0, ApprovedTenorMonths = vm.ApprovedTenorMonths,
            AdditionalConditions = vm.AdditionalConditions, MadeByUserId = UserId
        };
        await _decisions.RecordDecisionAsync(decision);

        var newStatus = vm.DecisionType switch
        {
            DecisionType.ConditionallyApproved => AppStatus.ConditionallyApproved,
            DecisionType.Declined => AppStatus.Declined,
            _ => AppStatus.UnderBankDecision
        };
        await _workflow.AdvanceStatusAsync(vm.LoanRequestId, newStatus, UserId, "Bank", vm.DecisionRemarks);

        await _notifications.CreateAsync(request.UserId, $"Application Decision",
            $"A decision has been made on your application {request.CaseId ?? request.RequestNo}.",
            NotificationType.Info, "LoanRequest", vm.LoanRequestId);

        if (vm.DecisionType == DecisionType.Declined)
        {
            var reasonCode = vm.DeclineReasonCodeId.HasValue
                ? (await _decisions.GetActiveReasonCodesAsync()).FirstOrDefault(r => r.Id == vm.DeclineReasonCodeId.Value)
                : null;
            await _email.SendFromTemplateAsync("ENDUSER_APPLICATION_DECLINED", request.User.EmailAddress, new()
            {
                ["FullName"] = request.User.FullName,
                ["CaseId"] = request.CaseId ?? request.RequestNo,
                ["BankName"] = request.AssignedBank?.BankName ?? "",
                ["DeclineReason"] = reasonCode?.Description ?? vm.DecisionRemarks ?? "",
                ["PortalUrl"] = _config["AppSettings:PortalUrl"] ?? ""
            });
        }

        await _audit.LogAsync("DecisionRecorded", "BankDecision", vm.LoanRequestId,
            newValue: vm.DecisionType, userId: UserId);

        TempData["Success"] = $"Decision '{vm.DecisionType}' recorded.";
        return RedirectToAction("Assigned", "BankRequests");
    }
}
