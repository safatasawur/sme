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
public class BankAdditionalInfoController : Controller
{
    private readonly IAdditionalInfoRequestService _infoService;
    private readonly IWorkflowService _workflow;
    private readonly INotificationService _notifications;
    private readonly IEmailService _email;
    private readonly IAuditService _audit;
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;

    public BankAdditionalInfoController(IAdditionalInfoRequestService infoService, IWorkflowService workflow,
        INotificationService notifications, IEmailService email, IAuditService audit,
        ApplicationDbContext db, IConfiguration config)
    { _infoService = infoService; _workflow = workflow; _notifications = notifications;
      _email = email; _audit = audit; _db = db; _config = config; }

    private int BankId => int.TryParse(User.FindFirst("BankId")?.Value, out var id) ? id : 0;
    private int UserId => int.TryParse(User.FindFirst("UserId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> Create(int loanRequestId)
    {
        var request = await _db.LoanRequests.FirstOrDefaultAsync(r => r.Id == loanRequestId && r.AssignedBankId == BankId);
        if (request == null) return NotFound();

        ViewData["Title"] = "Request Information"; ViewData["PreTitle"] = "Additional Information"; ViewData["ActiveNav"] = "Requests";
        ViewBag.Request = request;
        return View(new CreateInfoRequestViewModel { LoanRequestId = loanRequestId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateInfoRequestViewModel vm)
    {
        var request = await _db.LoanRequests.Include(r => r.User).Include(r => r.AssignedBank)
            .FirstOrDefaultAsync(r => r.Id == vm.LoanRequestId && r.AssignedBankId == BankId);
        if (request == null) return NotFound();

        if (!ModelState.IsValid)
        { ViewBag.Request = request; return View(vm); }

        var infoRequest = new AdditionalInformationRequest
        {
            LoanRequestId = vm.LoanRequestId, Title = vm.Title, Description = vm.Description,
            RequiredDocuments = vm.RequiredDocuments, DueDate = vm.DueDate, CreatedByUserId = UserId
        };
        await _infoService.CreateAsync(infoRequest);

        // Advance workflow if not already in AdditionalInformationRequired
        if (request.Status?.ValueText != AppStatus.AdditionalInformationRequired)
        {
            try
            {
                await _workflow.AdvanceStatusAsync(vm.LoanRequestId, AppStatus.AdditionalInformationRequired,
                    UserId, "Bank", $"Information requested: {vm.Title}");
            }
            catch { /* Status transition may not be allowed from current state */ }
        }

        // Notify applicant
        await _notifications.CreateAsync(request.UserId, "Additional Information Requested",
            $"Your application {request.CaseId ?? request.RequestNo} requires additional information: {vm.Title}",
            NotificationType.Warning, "AdditionalInformationRequest", infoRequest.Id);

        await _email.SendFromTemplateAsync("ENDUSER_ADDITIONAL_INFO_REQUESTED", request.User.EmailAddress, new()
        {
            ["FullName"] = request.User.FullName,
            ["CaseId"] = request.CaseId ?? request.RequestNo,
            ["BankName"] = request.AssignedBank?.BankName ?? "",
            ["RequestTitle"] = vm.Title,
            ["DueDate"] = vm.DueDate?.ToString("yyyy-MM-dd") ?? "N/A",
            ["PortalUrl"] = _config["AppSettings:PortalUrl"] ?? ""
        });

        await _audit.LogAsync("InfoRequestCreated", "AdditionalInformationRequest",
            infoRequest.Id, newValue: vm.Title, userId: UserId);

        TempData["Success"] = "Information request sent to applicant.";
        return RedirectToAction("Detail", "BankRequests", new { id = vm.LoanRequestId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int id)
    {
        var infoRequest = await _db.AdditionalInfoRequests.Include(r => r.LoanRequest)
            .FirstOrDefaultAsync(r => r.Id == id && r.LoanRequest.AssignedBankId == BankId);
        if (infoRequest == null) return NotFound();
        await _infoService.CloseAsync(id);
        TempData["Success"] = "Information request closed.";
        return RedirectToAction("Detail", "BankRequests", new { id = infoRequest.LoanRequestId });
    }
}
