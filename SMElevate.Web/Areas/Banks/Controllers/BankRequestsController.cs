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
public class BankRequestsController : Controller
{
    private readonly ILoanRequestService _requests;
    private readonly ILookupService _lookups;
    private readonly INotificationService _notifications;
    private readonly IEmailService _email;
    private readonly IAuditService _audit;
    private readonly IFileUploadService _fileUpload;
    private readonly IWorkflowService _workflow;
    private readonly IBankAssessmentService _assessment;
    private readonly IAdditionalInfoRequestService _infoService;
    private readonly IBankDecisionService _decisionService;
    private readonly IConditionalOfferService _offerService;
    private readonly ApplicationDbContext _db;

    public BankRequestsController(ILoanRequestService requests, ILookupService lookups,
        INotificationService notifications, IEmailService email, IAuditService audit, IFileUploadService fileUpload,
        IWorkflowService workflow, IBankAssessmentService assessment, IAdditionalInfoRequestService infoService,
        IBankDecisionService decisionService, IConditionalOfferService offerService, ApplicationDbContext db)
    {
        _requests = requests; _lookups = lookups; _notifications = notifications; _email = email;
        _audit = audit; _fileUpload = fileUpload; _workflow = workflow; _assessment = assessment;
        _infoService = infoService; _decisionService = decisionService; _offerService = offerService; _db = db;
    }

    private int GetBankId()
    {
        var claim = User.FindFirst("BankId");
        return claim is not null && int.TryParse(claim.Value, out var id) ? id : 0;
    }
    private int GetUserId()
    {
        var claim = User.FindFirst("UserId");
        return claim is not null && int.TryParse(claim.Value, out var id) ? id : 0;
    }

    public async Task<IActionResult> Assigned()
    {
        ViewData["Title"] = "Active Applications"; ViewData["PreTitle"] = "Application Management"; ViewData["ActiveNav"] = "Requests";
        var bankId = GetBankId();
        // Show all active/in-process applications for this bank
        var finalStatuses = new[] { AppStatus.Declined, AppStatus.OfferRejected, AppStatus.OfferExpired, AppStatus.Completed, AppStatus.Closed, AppStatus.Withdrawn };
        var requests = await _db.LoanRequests
            .AsNoTracking()
            .Include(r => r.Status)
            .Include(r => r.User)
            .Where(r => r.AssignedBankId == bankId && !finalStatuses.Contains(r.Status!.ValueText))
            .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
            .ToListAsync();
        return View(requests);
    }

    public async Task<IActionResult> Approved()
    {
        ViewData["Title"] = "Approved Requests"; ViewData["PreTitle"] = "Request Management"; ViewData["ActiveNav"] = "Requests";
        var requests = await _requests.GetRequestsByBankAsync(GetBankId(), AppStatus.Approved);
        return View(requests);
    }

    public async Task<IActionResult> Rejected()
    {
        ViewData["Title"] = "Rejected Requests"; ViewData["PreTitle"] = "Request Management"; ViewData["ActiveNav"] = "Requests";
        var requests = await _requests.GetRequestsByBankAsync(GetBankId(), AppStatus.Rejected);
        return View(requests);
    }

    public async Task<IActionResult> Detail(int id, bool viewOnly = false)
    {
        var bankId = GetBankId();
        var request = await _db.LoanRequests
            .Include(r => r.Status)
            .Include(r => r.User)
            .Include(r => r.AssignedBank)
            .Include(r => r.Scheme)
            .Include(r => r.Shareholders)
            .Include(r => r.FieldValues)
            .Include(r => r.Attachments)
            .Include(r => r.StatusHistory.OrderBy(h => h.CreatedAt))
                .ThenInclude(h => h.NewStatus)
            .Include(r => r.StatusHistory)
                .ThenInclude(h => h.ChangedBy)
            .FirstOrDefaultAsync(r => r.Id == id && r.AssignedBankId == bankId);

        if (request == null) return NotFound();

        ViewData["Title"] = $"Application {request.CaseId ?? request.RequestNo}";
        ViewData["PreTitle"] = "Application Management"; ViewData["ActiveNav"] = "Requests";

        var currentStatus = request.Status?.ValueText ?? "";
        var vm = new BankApplicationDetailViewModel
        {
            Request = request,
            StatusHistory = request.StatusHistory.OrderBy(h => h.CreatedAt).ToList(),
            Assessments = await _assessment.GetByLoanRequestAsync(id),
            InfoRequests = await _infoService.GetByLoanRequestAsync(id),
            Decision = await _decisionService.GetByLoanRequestAsync(id),
            Offers = await _offerService.GetByLoanRequestAsync(id),
            Checklist = await _db.PostApprovalChecklists.Include(c => c.Items).FirstOrDefaultAsync(c => c.LoanRequestId == id),
            Disbursement = await _db.Disbursements.FirstOrDefaultAsync(d => d.LoanRequestId == id),
            AllowedTransitions = await _workflow.GetAllowedTransitionsAsync(currentStatus, "Bank"),
            DeclineReasonCodes = await _decisionService.GetActiveReasonCodesAsync()
        };

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitDecision(BankDecisionViewModel vm)
    {
        var request = await _requests.GetByIdAsync(vm.RequestId);
        if (request is null || request.AssignedBankId != GetBankId()) return NotFound();

        var newStatus = await _lookups.GetValueByIdAsync(vm.NewStatusId);
        var requiresRemarks = newStatus?.ValueText is AppStatus.Approved or AppStatus.Rejected or AppStatus.MoreInformationRequired;
        if (requiresRemarks && string.IsNullOrWhiteSpace(vm.Remarks))
        {
            TempData["Error"] = "Remarks/reason are required for this decision status.";
            return RedirectToAction("Detail", new { id = vm.RequestId });
        }

        string? attachmentPath = null;
        if (vm.Attachment is { Length: > 0 })
        {
            if (!_fileUpload.IsAllowedType(vm.Attachment.FileName)) { TempData["Error"] = "Invalid file type."; return RedirectToAction("Detail", new { id = vm.RequestId }); }
            var (fileName, originalFileName, filePath, contentType, fileSize) = await _fileUpload.UploadAsync(vm.Attachment, "uploads");
            attachmentPath = filePath;
            await _requests.SaveAttachmentAsync(new LoanRequestAttachment { LoanRequestId = vm.RequestId, UploadedByUserId = GetUserId(), FileName = fileName, OriginalFileName = originalFileName, FilePath = filePath, ContentType = contentType, FileSize = fileSize, AttachmentType = "BankDecision" });
        }

        await _requests.UpdateStatusAsync(vm.RequestId, vm.NewStatusId, GetUserId(), vm.Remarks, attachmentPath);
        await _audit.LogAsync("StatusUpdate", "LoanRequest", vm.RequestId, oldValue: request.Status?.ValueText, newValue: newStatus?.ValueText);

        // Notifications for EndUser
        var title = $"Loan Request {request.RequestNo} - {newStatus?.ValueText}";
        var message = $"Your request {request.RequestNo} status has been updated to {newStatus?.ValueText} by {request.AssignedBank?.BankName ?? "the bank"}.";
        if (vm.Remarks is not null) message += $" Remarks: {vm.Remarks}";
        await _notifications.CreateAsync(request.UserId, title, message, NotificationType.Info, "LoanRequest", vm.RequestId);

        // Send status-wise email to EndUser
        var templateCode = newStatus?.ValueText switch
        {
            AppStatus.InProcess => "ENDUSER_STATUS_IN_PROCESS",
            AppStatus.Approved => "ENDUSER_STATUS_APPROVED",
            AppStatus.Rejected => "ENDUSER_STATUS_REJECTED",
            AppStatus.MoreInformationRequired => "ENDUSER_STATUS_MORE_INFORMATION_REQUIRED",
            AppStatus.Completed => "ENDUSER_STATUS_COMPLETED",
            _ => null
        };
        if (templateCode is not null)
        {
            await _email.SendFromTemplateAsync(templateCode, request.User.EmailAddress, new()
            {
                ["FullName"] = request.User.FullName, ["RequestNo"] = request.RequestNo,
                ["BusinessName"] = request.NameOfBusiness, ["BankName"] = request.AssignedBank?.BankName ?? "the bank",
                ["Status"] = newStatus?.ValueText ?? "", ["Remarks"] = vm.Remarks ?? "", ["UpdatedDate"] = DateTime.Today.ToString("yyyy-MM-dd")
            });
        }

        TempData["Success"] = $"Decision recorded. Status updated to {newStatus?.ValueText}.";
        return newStatus?.ValueText switch
        {
            AppStatus.Approved => RedirectToAction("Approved"),
            AppStatus.Rejected => RedirectToAction("Rejected"),
            _ => RedirectToAction("Assigned")
        };
    }
}
