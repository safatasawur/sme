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
public class BankConditionalOfferController : Controller
{
    private readonly IConditionalOfferService _offers;
    private readonly IWorkflowService _workflow;
    private readonly INotificationService _notifications;
    private readonly IEmailService _email;
    private readonly IFileUploadService _fileUpload;
    private readonly IAuditService _audit;
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;

    public BankConditionalOfferController(IConditionalOfferService offers, IWorkflowService workflow,
        INotificationService notifications, IEmailService email, IFileUploadService fileUpload,
        IAuditService audit, ApplicationDbContext db, IConfiguration config)
    { _offers = offers; _workflow = workflow; _notifications = notifications; _email = email;
      _fileUpload = fileUpload; _audit = audit; _db = db; _config = config; }

    private int BankId => int.TryParse(User.FindFirst("BankId")?.Value, out var id) ? id : 0;
    private int UserId => int.TryParse(User.FindFirst("UserId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> Create(int loanRequestId)
    {
        var request = await _db.LoanRequests.Include(r => r.BankDecision)
            .FirstOrDefaultAsync(r => r.Id == loanRequestId && r.AssignedBankId == BankId);
        if (request == null) return NotFound();

        ViewData["Title"] = "Issue Conditional Offer"; ViewData["PreTitle"] = "Conditional Offer"; ViewData["ActiveNav"] = "Requests";
        ViewBag.Request = request;

        var vm = new CreateConditionalOfferViewModel
        {
            LoanRequestId = loanRequestId,
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            ApprovedAmount = request.BankDecision?.ApprovedAmount ?? request.Amount,
            TenorMonths = request.BankDecision?.ApprovedTenorMonths ?? request.Tenor,
            FacilityType = request.BankDecision?.ApprovedFacilityType
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateConditionalOfferViewModel vm)
    {
        var request = await _db.LoanRequests.Include(r => r.User).Include(r => r.AssignedBank)
            .FirstOrDefaultAsync(r => r.Id == vm.LoanRequestId && r.AssignedBankId == BankId);
        if (request == null) return NotFound();

        if (vm.ExpiryDate <= DateTime.UtcNow)
        { TempData["Error"] = "Expiry date must be in the future."; return RedirectToAction("Create", new { loanRequestId = vm.LoanRequestId }); }

        string? offerLetterPath = null;
        if (vm.OfferLetterFile is { Length: > 0 })
        {
            if (!_fileUpload.IsAllowedType(vm.OfferLetterFile.FileName))
            { TempData["Error"] = "Invalid file type."; return RedirectToAction("Create", new { loanRequestId = vm.LoanRequestId }); }
            var (_, _, filePath, _, _) = await _fileUpload.UploadAsync(vm.OfferLetterFile, "offers");
            offerLetterPath = filePath;
        }

        var offer = new ConditionalOffer
        {
            LoanRequestId = vm.LoanRequestId, ExpiryDate = vm.ExpiryDate, FacilityType = vm.FacilityType,
            ApprovedAmount = vm.ApprovedAmount, TenorMonths = vm.TenorMonths, PricingSummary = vm.PricingSummary,
            TermsAndConditions = vm.TermsAndConditions, ConditionsPrecedent = vm.ConditionsPrecedent,
            OfferLetterPath = offerLetterPath, CreatedByUserId = UserId, IssueDate = DateTime.UtcNow
        };
        var created = await _offers.CreateOfferAsync(offer);

        await _workflow.AdvanceStatusAsync(vm.LoanRequestId, AppStatus.OfferIssued, UserId, "Bank",
            $"Offer {created.OfferNumber} issued");

        await _notifications.CreateAsync(request.UserId, "Conditional Offer Available",
            $"A conditional offer has been issued for your application {request.CaseId ?? request.RequestNo}. Offer No: {created.OfferNumber}",
            NotificationType.Success, "ConditionalOffer", created.Id);

        await _email.SendFromTemplateAsync("ENDUSER_OFFER_ISSUED", request.User.EmailAddress, new()
        {
            ["FullName"] = request.User.FullName,
            ["CaseId"] = request.CaseId ?? request.RequestNo,
            ["OfferNumber"] = created.OfferNumber,
            ["ExpiryDate"] = created.ExpiryDate.ToString("yyyy-MM-dd"),
            ["PortalUrl"] = _config["AppSettings:PortalUrl"] ?? ""
        });

        await _audit.LogAsync("OfferIssued", "ConditionalOffer", created.Id,
            newValue: created.OfferNumber, userId: UserId);

        TempData["Success"] = $"Conditional offer {created.OfferNumber} issued.";
        return RedirectToAction("Detail", "BankRequests", new { id = vm.LoanRequestId });
    }
}
