using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Areas.EndUser.ViewModels;
using SMElevate.Web.Data;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;
using System.Text.Json;

namespace SMElevate.Web.Areas.EndUser.Controllers;

[Area("EndUser")]
[Authorize(Policy = "EndUserOnly")]
public class EndUserLoanRequestsController : Controller
{
    private readonly ILoanRequestService _requests;
    private readonly IBankService _banks;
    private readonly ISchemeService _schemes;
    private readonly ISchemeFormConfigService _formConfig;
    private readonly ILookupService _lookups;
    private readonly INotificationService _notifications;
    private readonly IEmailService _email;
    private readonly IAuditService _audit;
    private readonly IConfiguration _config;
    private readonly IAdditionalInfoRequestService _infoRequests;
    private readonly IConditionalOfferService _offers;
    private readonly IWorkflowService _workflow;
    private readonly ApplicationDbContext _db;
    private readonly IBusinessProfileService _businessProfiles;

    public EndUserLoanRequestsController(ILoanRequestService requests, IBankService banks,
        ISchemeService schemes, ISchemeFormConfigService formConfig, ILookupService lookups,
        INotificationService notifications, IEmailService email, IAuditService audit, IConfiguration config,
        IAdditionalInfoRequestService infoRequests, IConditionalOfferService offers,
        IWorkflowService workflow, ApplicationDbContext db, IBusinessProfileService businessProfiles)
    {
        _requests = requests; _banks = banks; _schemes = schemes; _formConfig = formConfig;
        _lookups = lookups; _notifications = notifications; _email = email; _audit = audit; _config = config;
        _infoRequests = infoRequests; _offers = offers; _workflow = workflow; _db = db;
        _businessProfiles = businessProfiles;
    }

    private int UserId => int.Parse(User.FindFirst("UserId")!.Value);
    private string UserName => User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value ?? "User";
    private string UserEmail => User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value ?? "";

    public IActionResult Index()
    {
        ViewData["Title"] = "Request a Loan"; ViewData["PreTitle"] = "EndUser Portal"; ViewData["ActiveNav"] = "LoanRequests";
        return RedirectToAction("Create");
    }

    public async Task<IActionResult> MyApplications()
    {
        ViewData["Title"] = "My Applications"; ViewData["PreTitle"] = "EndUser Portal"; ViewData["ActiveNav"] = "MyApplications";
        return View(await _requests.GetRequestsByUserAsync(UserId));
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Request a Loan"; ViewData["PreTitle"] = "EndUser Portal"; ViewData["ActiveNav"] = "LoanRequests";
        var vm = new LoanRequestCreateViewModel
        {
            Schemes = await _schemes.GetPublishedSchemesAsync(),
            Shareholders = new List<ShareholderRowViewModel>()
        };
        await RepopulateCreateVm(vm);
        return View(vm);
    }

    // ── AJAX: return available fields for a scheme ────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetFormForScheme(int schemeId)
    {
        var fields = await _formConfig.GetAvailableFieldsAsync(schemeId);
        if (fields.Count == 0)
            return Json(new { error = "No form configuration is available for this scheme." });

        var banks = await _banks.GetAllBanksAsync(activeOnly: true);

        return Json(new
        {
            schemeId,
            fields = fields.Select(f => new
            {
                fieldName               = f.FieldName,
                fieldLabel              = f.FieldLabel,
                fieldType               = f.FieldType,
                sectionName             = f.SectionName,
                sectionOrder            = f.SectionOrder,
                displayOrder            = f.DisplayOrder,
                isRequired              = f.IsRequired,
                hasConditionalVisibility = f.HasConditionalVisibility,
                conditionalExpression   = f.ConditionalExpression ?? "",
                options                 = f.Options
            }),
            banks = banks.Select(b => new { id = b.Id, name = b.BankName })
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LoanRequestCreateViewModel vm)
    {
        SetCreateNav();

        if (!vm.SchemeId.HasValue || string.IsNullOrWhiteSpace(vm.FieldValuesJson) || vm.FieldValuesJson == "[]")
        {
            await RepopulateCreateVm(vm);
            TempData["Error"] = "Please select a scheme and complete the form before submitting.";
            return View(vm);
        }

        ModelState.Clear();

        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var rawValues = JsonSerializer.Deserialize<List<DynamicFieldValueDto>>(vm.FieldValuesJson, opts) ?? new();
        var dict = rawValues
            .GroupBy(fv => fv.FieldName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Last().FieldValue ?? "", StringComparer.OrdinalIgnoreCase);

        var request = MapDynamicToLoanRequest(dict, vm);

        // ── Business Profile: verify ownership; load bank and shareholders from DB ──
        List<LoanRequestShareholder> shareholders;

        if (vm.BusinessProfileId.HasValue)
        {
            var biz = await _businessProfiles.GetByIdForUserAsync(vm.BusinessProfileId.Value, UserId);
            if (biz is null || !biz.IsActive)
            {
                await RepopulateCreateVm(vm);
                TempData["Error"] = "The selected business profile could not be verified. Please try again.";
                return View(vm);
            }

            if (!biz.BusinessBankId.HasValue)
            {
                await RepopulateCreateVm(vm);
                TempData["Error"] = "The selected business profile does not have an associated bank. Please update the business profile first.";
                return View(vm);
            }

            request.BusinessProfileId = biz.Id;
            request.OwnerCNIC        = biz.OwnerCNIC;
            request.AssignedBankId   = biz.BusinessBankId; // Always from DB, never from browser

            // Shareholders sourced from verified BP — browser values ignored
            shareholders = biz.Shareholders
                .Select(s => new LoanRequestShareholder
                {
                    Name                   = s.Name,
                    ContactNo              = s.ContactNo,
                    Email                  = s.Email,
                    CNIC                   = s.CNIC,
                    ShareholdingPercentage = s.ShareholdingPercentage
                }).ToList();
        }
        else
        {
            shareholders = MapShareholders(vm.Shareholders, dict);
        }

        // ── Server-side duplicate guard (60-second window) ─────────────────────
        var isDuplicate = await _db.LoanRequests.AnyAsync(r =>
            r.UserId           == UserId &&
            r.SchemeId         == vm.SchemeId &&
            r.BusinessProfileId == vm.BusinessProfileId &&
            r.CreatedAt        >= DateTime.UtcNow.AddSeconds(-60));
        if (isDuplicate)
        {
            TempData["Error"] = "Your application was already submitted. Please check My Applications.";
            return RedirectToAction("MyApplications");
        }

        request.ConsentGiven       = vm.ConsentGiven;
        request.ConsentDate        = vm.ConsentGiven ? DateTime.UtcNow : null;
        request.ConsentIpAddress   = vm.ConsentGiven ? HttpContext.Connection.RemoteIpAddress?.ToString() : null;
        request.ConsentVersion     = "v1.0";
        request.PreferredIdentifierType = vm.PreferredIdentifierType;
        request.IsDraft            = vm.SaveAsDraft;

        var created = await _requests.CreateAsync(request, shareholders);
        await _requests.SaveFieldValuesAsync(created.Id,
            rawValues.Select(fv => (fv.FieldName, (string?)fv.FieldValue)).ToList());
        await _audit.LogAsync("Create", "LoanRequest", created.Id, newValue: created.RequestNo);

        if (!vm.SaveAsDraft)
        {
            await _workflow.AdvanceStatusAsync(created.Id, AppStatus.Submitted, UserId, "EndUser",
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());
            var updated = await _requests.GetByIdAsync(created.Id);
            if (updated?.CaseId != null)
                await _email.SendFromTemplateAsync("ENDUSER_CASE_ID_GENERATED", UserEmail, new()
                {
                    ["FullName"] = UserName, ["CaseId"] = updated.CaseId,
                    ["PortalUrl"] = _config["AppSettings:PortalUrl"] ?? ""
                });
            await SendNotifications(created, vm, dict.GetValueOrDefault("NameOfBusiness", ""));
        }

        return RedirectToAction("Confirmation", new { requestNo = created.RequestNo });
    }

    public async Task<IActionResult> Confirmation(string requestNo)
    {
        ViewData["Title"] = "Application Submitted"; ViewData["PreTitle"] = "EndUser Portal"; ViewData["ActiveNav"] = "LoanRequests";
        var request = await _requests.GetByRequestNoAsync(requestNo);
        if (request is null || request.UserId != UserId) return NotFound();
        return View(request);
    }

    public async Task<IActionResult> Detail(string requestNo)
    {
        var request = await _db.LoanRequests
            .Include(r => r.Status)
            .Include(r => r.AssignedBank)
            .Include(r => r.Scheme)
            .Include(r => r.Shareholders)
            .Include(r => r.StatusHistory.OrderBy(h => h.CreatedAt))
                .ThenInclude(h => h.NewStatus)
            .Include(r => r.StatusHistory)
                .ThenInclude(h => h.ChangedBy)
            .Include(r => r.BankDecision)
                .ThenInclude(d => d!.DeclineReasonCode)
            .Include(r => r.Disbursement)
            .Include(r => r.PostApprovalChecklists)
                .ThenInclude(c => c.Items)
            .FirstOrDefaultAsync(r => r.RequestNo == requestNo);

        if (request is null || request.UserId != UserId) return NotFound();

        ViewData["Title"] = $"Application {request.CaseId ?? request.RequestNo}";
        ViewData["PreTitle"] = "EndUser Portal"; ViewData["ActiveNav"] = "MyApplications";

        var currentStatusText = request.Status?.ValueText ?? "";
        var vm = new LoanApplicationDetailViewModel
        {
            Request = request,
            StatusHistory = request.StatusHistory.OrderBy(h => h.CreatedAt).ToList(),
            InfoRequests = await _infoRequests.GetByLoanRequestAsync(request.Id),
            AllOffers = await _offers.GetByLoanRequestAsync(request.Id),
            Decision = request.BankDecision,
            Disbursement = request.Disbursement,
            Checklist = request.PostApprovalChecklists.FirstOrDefault(),
            Documents = await _db.ApplicationDocuments.Where(d => d.LoanRequestId == request.Id && d.UploadedByUserId == UserId).ToListAsync(),
            AllowedTransitions = await _workflow.GetAllowedTransitionsAsync(currentStatusText, "EndUser")
        };
        vm.ActiveOffer = vm.AllOffers.FirstOrDefault(o => o.Status == OfferStatus.Issued && o.ExpiryDate >= DateTime.UtcNow);

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RespondToInfoRequest(InfoRequestResponseViewModel vm)
    {
        var infoRequest = await _db.AdditionalInfoRequests
            .Include(r => r.LoanRequest)
            .FirstOrDefaultAsync(r => r.Id == vm.RequestId && r.LoanRequest.UserId == UserId);
        if (infoRequest == null) return NotFound();

        await _infoRequests.SubmitResponseAsync(vm.RequestId, vm.Response, UserId, vm.Documents?.ToList());

        // Notify bank
        var request = infoRequest.LoanRequest;
        if (request.AssignedBankId.HasValue)
        {
            var bank = await _banks.GetByIdAsync(request.AssignedBankId.Value);
            if (bank != null)
                await _email.SendFromTemplateAsync("BANK_ADDITIONAL_INFO_RESPONDED", bank.BankEmailAddress, new()
                {
                    ["CaseId"] = request.CaseId ?? request.RequestNo,
                    ["RequestTitle"] = infoRequest.Title,
                    ["ResponseDate"] = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    ["PortalUrl"] = _config["AppSettings:PortalUrl"] ?? ""
                });
        }

        TempData["Success"] = "Your response has been submitted successfully.";
        return RedirectToAction("Detail", new { requestNo = infoRequest.LoanRequest.RequestNo });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RespondToOffer(OfferResponseViewModel vm)
    {
        var offer = await _db.ConditionalOffers
            .Include(o => o.LoanRequest)
            .FirstOrDefaultAsync(o => o.Id == vm.OfferId && o.LoanRequest.UserId == UserId);
        if (offer == null) return NotFound();

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _offers.RecordResponseAsync(vm.OfferId, vm.ResponseType, UserId, vm.Remarks, ipAddress);

        // Advance workflow status
        var newStatus = vm.ResponseType == "Accepted" ? AppStatus.OfferAccepted : AppStatus.OfferRejected;
        await _workflow.AdvanceStatusAsync(offer.LoanRequest.Id, newStatus, UserId, "EndUser",
            vm.Remarks, ipAddress: ipAddress);

        await _notifications.CreateAsync(UserId, $"Offer {vm.ResponseType}",
            $"You have {vm.ResponseType.ToLower()} the conditional offer {offer.OfferNumber}.",
            NotificationType.Info, "ConditionalOffer", offer.Id);

        await _audit.LogAsync($"Offer{vm.ResponseType}", "ConditionalOffer", offer.Id, userId: UserId);
        TempData["Success"] = $"Offer has been {vm.ResponseType.ToLower()} successfully.";
        return RedirectToAction("Detail", new { requestNo = offer.LoanRequest.RequestNo });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SetCreateNav()
    {
        ViewData["Title"] = "Request a Loan"; ViewData["PreTitle"] = "EndUser Portal"; ViewData["ActiveNav"] = "LoanRequests";
    }

    private async Task RepopulateCreateVm(LoanRequestCreateViewModel vm)
    {
        vm.Businesses               = await _businessProfiles.GetByUserAsync(UserId, activeOnly: true);
        vm.Banks                    = await _banks.GetAllBanksAsync(activeOnly: true);
        vm.BusinessNatures          = await _lookups.GetValuesAsync("BUSINESS_NATURE");
        vm.BusinessStatuses         = await _lookups.GetValuesAsync("BUSINESS_STATUS");
        vm.BusinessPremises         = await _lookups.GetValuesAsync("BUSINESS_PREMISE");
        vm.FacilityRequestedOptions = await _lookups.GetValuesAsync("FACILITY_REQUESTED");
        vm.FacilityTypes            = await _lookups.GetValuesAsync("TYPE_OF_FACILITY");
        if (vm.Schemes == null || !vm.Schemes.Any())
            vm.Schemes = await _schemes.GetPublishedSchemesAsync();
    }

    private LoanRequest MapDynamicToLoanRequest(Dictionary<string, string> d, LoanRequestCreateViewModel vm)
    {
        static string? S(Dictionary<string, string> d, string k) => d.TryGetValue(k, out var v) && !string.IsNullOrEmpty(v) ? v : null;
        static decimal D(Dictionary<string, string> d, string k) => decimal.TryParse(d.GetValueOrDefault(k, "0"), out var n) ? n : 0;
        static int I(Dictionary<string, string> d, string k) => int.TryParse(d.GetValueOrDefault(k, "0"), out var n) ? n : 0;

        return new LoanRequest
        {
            UserId              = UserId,
            SchemeId            = vm.SchemeId,
            SchemeFormId        = vm.SchemeFormId,
            NameOfBusiness      = S(d, "NameOfBusiness") ?? "",
            ContactPerson       = S(d, "ContactPerson") ?? "",
            CellOrLandlineNo    = S(d, "CellOrLandlineNo") ?? "",
            BusinessAddress     = S(d, "BusinessAddress") ?? "",
            NTNNo               = S(d, "NTNNo"),
            AnnualSales         = D(d, "AnnualSales"),
            YearOfEstablishment = I(d, "YearOfEstablishment"),
            NoOfEmployees       = I(d, "NoOfEmployees"),
            BusinessPremise     = S(d, "BusinessPremise") ?? "",
            IsBusinessRegistered = d.GetValueOrDefault("IsBusinessRegistered", "No") == "Yes",
            RegistrationAuthority = S(d, "RegistrationAuthority"),
            BusinessStatus      = S(d, "BusinessStatus") ?? "",
            BusinessNature      = S(d, "BusinessNature") ?? "",
            BusinessDescription = S(d, "BusinessDescription") ?? "",
            FacilityRequested   = S(d, "FacilityRequested") ?? "",
            TypeOfFacility      = S(d, "TypeOfFacility") ?? "",
            Amount              = D(d, "Amount"),
            Tenor               = I(d, "Tenor"),
            AssignedBankId      = int.TryParse(S(d, "AssignedBankId"), out var bid) ? (int?)bid : null,
            IBANOrRaastType     = S(d, "IBANOrRaastType") ?? vm.IBANOrRaastType ?? "",
            IBANOrRaastValue    = S(d, "IBANOrRaastValue") ?? vm.IBANOrRaastValue ?? "",
            PreferredIdentifierType = vm.PreferredIdentifierType
        };
    }

    private static List<LoanRequestShareholder> MapShareholders(
        List<ShareholderRowViewModel> rows, Dictionary<string, string> dict)
    {
        // Shareholders submitted via indexed model binding Shareholders[0].Name ...
        return (rows ?? new()).Where(s => !string.IsNullOrWhiteSpace(s.Name))
            .Select(s => new LoanRequestShareholder
            {
                Name = s.Name, ContactNo = s.ContactNo, Email = s.Email,
                CNIC = s.CNIC, ShareholdingPercentage = s.ShareholdingPercentage
            }).ToList();
    }

    private async Task SendNotifications(LoanRequest created, LoanRequestCreateViewModel vm, string businessName)
    {
        await _audit.LogAsync("Create", "LoanRequest", created.Id, newValue: created.RequestNo);
        await _notifications.CreateAsync(UserId, "Loan Request Submitted",
            $"Your request {created.RequestNo} has been submitted and assigned to the selected bank.",
            NotificationType.Success, "LoanRequest", created.Id);
        var bank = created.AssignedBankId.HasValue ? await _banks.GetByIdAsync(created.AssignedBankId.Value) : null;
        await _email.SendFromTemplateAsync("ENDUSER_LOAN_SUBMITTED", UserEmail, new()
        {
            ["FullName"] = UserName, ["RequestNo"] = created.RequestNo, ["BusinessName"] = businessName,
            ["BankName"] = bank?.BankName ?? "", ["SubmittedDate"] = DateTime.Today.ToString("yyyy-MM-dd"),
            ["PortalUrl"] = _config["AppSettings:PortalUrl"] ?? ""
        });
        if (bank is not null)
            await _email.SendFromTemplateAsync("BANK_NEW_REQUEST_ASSIGNED", bank.BankEmailAddress, new()
            {
                ["RequestNo"] = created.RequestNo, ["BusinessName"] = businessName,
                ["SubmittedDate"] = DateTime.Today.ToString("yyyy-MM-dd"), ["PortalUrl"] = _config["AppSettings:PortalUrl"] ?? ""
            });
    }

    private class DynamicFieldValueDto
    {
        public string FieldName { get; set; } = "";
        public string? FieldValue { get; set; }
    }
}
