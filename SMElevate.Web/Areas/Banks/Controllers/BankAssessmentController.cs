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
public class BankAssessmentController : Controller
{
    private readonly IBankAssessmentService _assessment;
    private readonly IWorkflowService _workflow;
    private readonly IFileUploadService _fileUpload;
    private readonly IAuditService _audit;
    private readonly ApplicationDbContext _db;

    public BankAssessmentController(IBankAssessmentService assessment, IWorkflowService workflow,
        IFileUploadService fileUpload, IAuditService audit, ApplicationDbContext db)
    { _assessment = assessment; _workflow = workflow; _fileUpload = fileUpload; _audit = audit; _db = db; }

    private int BankId => int.TryParse(User.FindFirst("BankId")?.Value, out var id) ? id : 0;
    private int UserId => int.TryParse(User.FindFirst("UserId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> Index(int loanRequestId)
    {
        var request = await GetRequestOrNotFound(loanRequestId);
        if (request == null) return NotFound();

        ViewData["Title"] = $"Assessment — {request.CaseId ?? request.RequestNo}";
        ViewData["PreTitle"] = "Bank Assessment"; ViewData["ActiveNav"] = "Requests";

        var assessments = await _assessment.GetByLoanRequestAsync(loanRequestId);
        ViewBag.LoanRequestId = loanRequestId;
        ViewBag.Request = request;
        ViewBag.Assessments = assessments;
        ViewBag.AllowedTransitions = await _workflow.GetAllowedTransitionsAsync(request.Status?.ValueText ?? "", "Bank");
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(BankAssessmentFormViewModel vm)
    {
        var request = await GetRequestOrNotFound(vm.LoanRequestId);
        if (request == null) return NotFound();

        string? attachmentPath = null;
        if (vm.Attachment is { Length: > 0 })
        {
            if (!_fileUpload.IsAllowedType(vm.Attachment.FileName))
            { TempData["Error"] = "Invalid file type."; return RedirectToAction("Index", new { loanRequestId = vm.LoanRequestId }); }
            var (_, _, filePath, _, _) = await _fileUpload.UploadAsync(vm.Attachment, "assessments");
            attachmentPath = filePath;
        }

        var entity = new BankAssessment
        {
            LoanRequestId = vm.LoanRequestId, AssessmentType = vm.AssessmentType,
            Status = vm.Status, CheckDate = vm.CheckDate, ReferenceNumber = vm.ReferenceNumber,
            ResultSummary = vm.ResultSummary, ScorecardReference = vm.ScorecardReference,
            Result = vm.Result, RiskCategory = vm.RiskCategory, AssessmentDate = vm.AssessmentDate,
            CDDStatus = vm.CDDStatus, KYCStatus = vm.KYCStatus, AMLStatus = vm.AMLStatus,
            SanctionsScreeningStatus = vm.SanctionsScreeningStatus, PEPScreeningStatus = vm.PEPScreeningStatus,
            ComplianceResult = vm.ComplianceResult, CompletionDate = vm.CompletionDate,
            Remarks = vm.Remarks, AttachmentPath = attachmentPath, UpdatedByUserId = UserId
        };

        await _assessment.UpsertAsync(entity);
        await _audit.LogAsync("AssessmentUpdate", "BankAssessment", vm.LoanRequestId,
            newValue: $"{vm.AssessmentType}:{vm.Status}", userId: UserId);

        // Auto-advance workflow if assessment completed
        if (vm.Status == AssessmentStatus.Completed)
        {
            var targetStatus = vm.AssessmentType switch
            {
                AssessmentType.CreditBureauCheck => AppStatus.UnderRiskAssessment,
                AssessmentType.RiskAssessment => AppStatus.UnderCDDComplianceReview,
                AssessmentType.CDDCompliance => AppStatus.UnderBankDecision,
                _ => null
            };
            if (targetStatus != null)
            {
                try
                {
                    await _workflow.AdvanceStatusAsync(vm.LoanRequestId, targetStatus, UserId, "Bank",
                        $"{vm.AssessmentType} completed");
                }
                catch { /* Transition may not be valid in current state — record assessment only */ }
            }
        }

        TempData["Success"] = "Assessment saved.";
        return RedirectToAction("Index", new { loanRequestId = vm.LoanRequestId });
    }

    private async Task<LoanRequest?> GetRequestOrNotFound(int id)
        => await _db.LoanRequests.Include(r => r.Status).FirstOrDefaultAsync(r => r.Id == id && r.AssignedBankId == BankId);
}
