using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMElevate.Web.Areas.EndUser.ViewModels;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Areas.EndUser.Controllers;

[Area("EndUser")]
[Authorize(Policy = "EndUserOnly")]
public class BusinessProfilesController : Controller
{
    private readonly IBusinessProfileService _bp;
    private readonly IEmailService _email;
    private readonly ISmsService _sms;
    private readonly ILookupService _lookups;
    private readonly IBankService _banks;
    private readonly IConfiguration _config;

    public BusinessProfilesController(IBusinessProfileService bp, IEmailService email,
        ISmsService sms, ILookupService lookups, IBankService banks, IConfiguration config)
    {
        _bp = bp; _email = email; _sms = sms; _lookups = lookups; _banks = banks; _config = config;
    }

    private int UserId => int.Parse(User.FindFirst("UserId")!.Value);

    private static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 1) return email;
        return email[0] + new string('*', Math.Min(at - 1, 5)) + email[at..];
    }

    private static string MaskMobile(string mobile) =>
        mobile.Length > 4
            ? mobile[..4] + new string('*', Math.Max(0, mobile.Length - 5)) + mobile[^1]
            : "****";

    private async Task PopulateLookupsAsync(BusinessProfileFormViewModel vm)
    {
        vm.BusinessNatures  = await _lookups.GetValuesAsync("BUSINESS_NATURE");
        vm.BusinessStatuses = await _lookups.GetValuesAsync("BUSINESS_STATUS");
        vm.BusinessPremises = await _lookups.GetValuesAsync("BUSINESS_PREMISE");
        vm.AvailableBanks   = await _banks.GetAllBanksAsync(activeOnly: true);
    }

    // ── Index ─────────────────────────────────────────────────────────────────

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "My Business Profiles";
        ViewData["PreTitle"] = "EndUser Portal";
        ViewData["ActiveNav"] = "BusinessProfiles";
        var list = await _bp.GetByUserAsync(UserId);
        return View(list);
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Add Business";
        ViewData["PreTitle"] = "EndUser Portal";
        ViewData["ActiveNav"] = "BusinessProfiles";
        var vm = new BusinessProfileCreateViewModel
        {
            Shareholders = new List<BusinessShareholderViewModel> { new() }
        };
        await PopulateLookupsAsync(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BusinessProfileCreateViewModel vm)
    {
        ViewData["Title"] = "Add Business";
        ViewData["PreTitle"] = "EndUser Portal";
        ViewData["ActiveNav"] = "BusinessProfiles";

        ApplyConditionalValidation(vm);

        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(vm);
            return View(vm);
        }

        var profile = MapFormToEntity(vm, UserId);
        var shareholders = vm.Shareholders
            .Where(s => !string.IsNullOrWhiteSpace(s.Name))
            .Select(MapShareholder).ToList();

        var created = await _bp.CreateAsync(profile, shareholders);

        await _bp.SendEmailOtpAsync(created.Id, UserId, _config, _email);
        await _bp.SendMobileOtpAsync(created.Id, UserId, _config, _sms);

        TempData["Success"] = "Business added. Please verify your email and mobile to activate the profile.";
        return RedirectToAction("Verify", new { id = created.Id });
    }

    // ── Edit ──────────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Business";
        ViewData["PreTitle"] = "EndUser Portal";
        ViewData["ActiveNav"] = "BusinessProfiles";

        var profile = await _bp.GetByIdForUserAsync(id, UserId);
        if (profile is null) return NotFound();

        var vm = new BusinessProfileEditViewModel
        {
            Id = profile.Id,
            NameOfBusiness = profile.NameOfBusiness,
            OwnerCNIC = profile.OwnerCNIC,
            ContactPerson = profile.ContactPerson,
            CellOrLandlineNo = profile.CellOrLandlineNo,
            NTNNo = profile.NTNNo,
            BusinessAddress = profile.BusinessAddress,
            AnnualSales = profile.AnnualSales,
            YearOfEstablishment = profile.YearOfEstablishment,
            NoOfEmployees = profile.NoOfEmployees,
            BusinessPremise = profile.BusinessPremise,
            IsBusinessRegistered = profile.IsBusinessRegistered ? "Yes" : "No",
            RegistrationAuthority = profile.RegistrationAuthority,
            BusinessStatus = profile.BusinessStatus,
            BusinessNature = profile.BusinessNature,
            BusinessDescription = profile.BusinessDescription,
            BusinessEmailAddress = profile.BusinessEmailAddress,
            BusinessBankId = profile.BusinessBankId,
            BusinessIBAN = profile.BusinessIBAN,
            CurrentCellOrLandlineNo = profile.CellOrLandlineNo,
            CurrentBusinessEmailAddress = profile.BusinessEmailAddress,
            IsBusinessEmailVerified = profile.IsBusinessEmailVerified,
            IsBusinessMobileVerified = profile.IsBusinessMobileVerified,
            BusinessVerificationStatus = profile.BusinessVerificationStatus,
            Shareholders = profile.Shareholders
                .Select(s => new BusinessShareholderViewModel
                {
                    Name = s.Name, ContactNo = s.ContactNo, Email = s.Email,
                    CNIC = s.CNIC, ShareholdingPercentage = s.ShareholdingPercentage
                }).ToList()
        };
        if (!vm.Shareholders.Any()) vm.Shareholders.Add(new());
        await PopulateLookupsAsync(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BusinessProfileEditViewModel vm)
    {
        ViewData["Title"] = "Edit Business";
        ViewData["PreTitle"] = "EndUser Portal";
        ViewData["ActiveNav"] = "BusinessProfiles";

        ApplyConditionalValidation(vm);

        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(vm);
            return View(vm);
        }

        var profile = await _bp.GetByIdForUserAsync(id, UserId);
        if (profile is null) return NotFound();

        bool mobileChanged = !string.Equals(profile.CellOrLandlineNo, vm.CellOrLandlineNo, StringComparison.OrdinalIgnoreCase);
        bool emailChanged = !string.Equals(profile.BusinessEmailAddress, vm.BusinessEmailAddress, StringComparison.OrdinalIgnoreCase);

        profile.NameOfBusiness = vm.NameOfBusiness;
        profile.OwnerCNIC = vm.OwnerCNIC;
        profile.ContactPerson = vm.ContactPerson;
        profile.CellOrLandlineNo = vm.CellOrLandlineNo;
        profile.NTNNo = vm.NTNNo;
        profile.BusinessAddress = vm.BusinessAddress;
        profile.AnnualSales = vm.AnnualSales;
        profile.YearOfEstablishment = vm.YearOfEstablishment;
        profile.NoOfEmployees = vm.NoOfEmployees;
        profile.BusinessPremise = vm.BusinessPremise;
        profile.IsBusinessRegistered = vm.IsBusinessRegistered == "Yes";
        profile.RegistrationAuthority = vm.IsBusinessRegistered == "Yes" ? vm.RegistrationAuthority : null;
        profile.BusinessStatus = vm.BusinessStatus;
        profile.BusinessNature = vm.BusinessNature;
        profile.BusinessDescription = vm.BusinessDescription;
        profile.BusinessEmailAddress = vm.BusinessEmailAddress;
        profile.BusinessBankId = vm.BusinessBankId;
        profile.BusinessIBAN = vm.BusinessIBAN;

        var shareholders = vm.Shareholders
            .Where(s => !string.IsNullOrWhiteSpace(s.Name))
            .Select(MapShareholder).ToList();

        await _bp.UpdateAsync(profile, shareholders, mobileChanged, emailChanged, _config, _email, _sms);

        if (mobileChanged || emailChanged)
        {
            TempData["Success"] = "Business updated. Please re-verify your changed contact details.";
            return RedirectToAction("Verify", new { id });
        }
        TempData["Success"] = "Business updated successfully.";
        return RedirectToAction("Index");
    }

    // ── Details ───────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        ViewData["Title"] = "Business Details";
        ViewData["PreTitle"] = "EndUser Portal";
        ViewData["ActiveNav"] = "BusinessProfiles";

        var profile = await _bp.GetByIdForUserAsync(id, UserId);
        if (profile is null) return NotFound();
        return View(new BusinessProfileDetailsViewModel { Profile = profile });
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        ViewData["Title"] = "Delete Business";
        ViewData["PreTitle"] = "EndUser Portal";
        ViewData["ActiveNav"] = "BusinessProfiles";

        var profile = await _bp.GetByIdForUserAsync(id, UserId);
        if (profile is null) return NotFound();
        return View(new BusinessProfileDetailsViewModel { Profile = profile });
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var (success, message) = await _bp.ArchiveAsync(id, UserId);
        if (!success)
        {
            TempData["Error"] = message;
            return RedirectToAction("Details", new { id });
        }
        TempData["Success"] = message;
        return RedirectToAction("Index");
    }

    // ── Verify ────────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Verify(int id)
    {
        ViewData["Title"] = "Verify Business";
        ViewData["PreTitle"] = "EndUser Portal";
        ViewData["ActiveNav"] = "BusinessProfiles";

        var profile = await _bp.GetByIdForUserAsync(id, UserId);
        if (profile is null) return NotFound();
        if (profile.BusinessVerificationStatus == Models.Common.BusinessVerificationStatus.Verified)
        {
            TempData["Success"] = "This business is already verified.";
            return RedirectToAction("Index");
        }

        var cooldown = _config.GetValue("BusinessProfileOtp:ResendCooldownSeconds", 60);
        var vm = new BusinessVerifyViewModel
        {
            BusinessProfileId = profile.Id,
            MaskedMobile = MaskMobile(profile.CellOrLandlineNo),
            MaskedEmail = MaskEmail(profile.BusinessEmailAddress),
            IsBusinessEmailVerified = profile.IsBusinessEmailVerified,
            IsBusinessMobileVerified = profile.IsBusinessMobileVerified,
            BusinessVerificationStatus = profile.BusinessVerificationStatus,
            EmailResendCooldown = profile.LastEmailOtpSentAt.HasValue &&
                (DateTime.UtcNow - profile.LastEmailOtpSentAt.Value).TotalSeconds < cooldown,
            MobileResendCooldown = profile.LastMobileOtpSentAt.HasValue &&
                (DateTime.UtcNow - profile.LastMobileOtpSentAt.Value).TotalSeconds < cooldown,
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyEmailOtp(int businessProfileId, string emailOtp)
    {
        var (success, error) = await _bp.VerifyEmailOtpAsync(businessProfileId, UserId, emailOtp ?? "", _config);
        TempData[success ? "EmailSuccess" : "EmailError"] = success ? "Email verified successfully." : error;
        return RedirectToAction("Verify", new { id = businessProfileId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyMobileOtp(int businessProfileId, string mobileOtp)
    {
        var (success, error) = await _bp.VerifyMobileOtpAsync(businessProfileId, UserId, mobileOtp ?? "", _config);
        TempData[success ? "MobileSuccess" : "MobileError"] = success ? "Mobile verified successfully." : error;
        return RedirectToAction("Verify", new { id = businessProfileId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendEmailOtp(int businessProfileId)
    {
        var (success, error) = await _bp.ResendEmailOtpAsync(businessProfileId, UserId, _config, _email);
        TempData[success ? "EmailSuccess" : "EmailError"] = success ? "OTP resent to your business email." : error;
        return RedirectToAction("Verify", new { id = businessProfileId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendMobileOtp(int businessProfileId)
    {
        var (success, error) = await _bp.ResendMobileOtpAsync(businessProfileId, UserId, _config, _sms);
        TempData[success ? "MobileSuccess" : "MobileError"] = success ? "OTP resent to your business mobile." : error;
        return RedirectToAction("Verify", new { id = businessProfileId });
    }

    // ── AJAX: business data for loan request autofill ─────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetForLoanRequest(int id)
    {
        var profile = await _bp.GetByIdForUserAsync(id, UserId);
        if (profile is null || !profile.IsActive) return NotFound();

        return Json(new
        {
            nameOfBusiness        = profile.NameOfBusiness,
            ownerCNIC             = profile.OwnerCNIC,
            contactPerson         = profile.ContactPerson,
            cellOrLandlineNo      = profile.CellOrLandlineNo,
            ntnNo                 = profile.NTNNo ?? "",
            businessAddress       = profile.BusinessAddress,
            annualSales           = profile.AnnualSales,
            yearOfEstablishment   = profile.YearOfEstablishment,
            noOfEmployees         = profile.NoOfEmployees,
            businessPremise       = profile.BusinessPremise,
            isBusinessRegistered  = profile.IsBusinessRegistered ? "Yes" : "No",
            registrationAuthority = profile.RegistrationAuthority ?? "",
            businessStatus        = profile.BusinessStatus,
            businessNature        = profile.BusinessNature,
            businessDescription   = profile.BusinessDescription,
            businessIBAN          = profile.BusinessIBAN ?? "",
            businessBankId        = profile.BusinessBankId,
            businessBankName      = profile.BusinessBank?.BankName ?? "",
            shareholders          = profile.Shareholders.Select(s => new
            {
                name         = s.Name,
                contactNo    = s.ContactNo ?? "",
                email        = s.Email ?? "",
                cnic         = s.CNIC ?? "",
                sharePercent = s.ShareholdingPercentage
            }).ToList()
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    // Server-side conditional validation that mirrors client-side rules
    private void ApplyConditionalValidation(BusinessProfileFormViewModel vm)
    {
        if (vm.IsBusinessRegistered == "Yes" && string.IsNullOrWhiteSpace(vm.RegistrationAuthority))
            ModelState.AddModelError(nameof(vm.RegistrationAuthority), "Registration authority is required when business is registered.");

        if (vm.BusinessStatus == "Partnership" &&
            !vm.Shareholders.Any(s => !string.IsNullOrWhiteSpace(s.Name)))
            ModelState.AddModelError("Shareholders", "At least one shareholder / partner is required for a Partnership.");
    }

    private static BusinessProfile MapFormToEntity(BusinessProfileFormViewModel vm, int userId) => new()
    {
        UserId = userId,
        NameOfBusiness = vm.NameOfBusiness,
        OwnerCNIC = vm.OwnerCNIC,
        ContactPerson = vm.ContactPerson,
        CellOrLandlineNo = vm.CellOrLandlineNo,
        NTNNo = vm.NTNNo,
        BusinessAddress = vm.BusinessAddress,
        AnnualSales = vm.AnnualSales,
        YearOfEstablishment = vm.YearOfEstablishment,
        NoOfEmployees = vm.NoOfEmployees,
        BusinessPremise = vm.BusinessPremise,
        IsBusinessRegistered = vm.IsBusinessRegistered == "Yes",
        RegistrationAuthority = vm.IsBusinessRegistered == "Yes" ? vm.RegistrationAuthority : null,
        BusinessStatus = vm.BusinessStatus,
        BusinessNature = vm.BusinessNature,
        BusinessDescription = vm.BusinessDescription,
        BusinessEmailAddress = vm.BusinessEmailAddress,
        BusinessBankId = vm.BusinessBankId,
        BusinessIBAN = vm.BusinessIBAN,
    };

    private static BusinessShareholder MapShareholder(BusinessShareholderViewModel vm) => new()
    {
        Name = vm.Name,
        ContactNo = vm.ContactNo,
        Email = vm.Email,
        CNIC = vm.CNIC,
        ShareholdingPercentage = vm.ShareholdingPercentage
    };
}
