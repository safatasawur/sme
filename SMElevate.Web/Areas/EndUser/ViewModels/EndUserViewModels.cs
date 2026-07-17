using SMElevate.Web.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace SMElevate.Web.Areas.EndUser.ViewModels;

// ─── Business Profile ViewModels ─────────────────────────────────────────────

public class BusinessShareholderViewModel
{
    public string Name { get; set; } = "";
    public string? ContactNo { get; set; }
    public string? Email { get; set; }
    public string? CNIC { get; set; }
    public decimal ShareholdingPercentage { get; set; }
}

public class BusinessProfileFormViewModel
{
    [Required(ErrorMessage = "Business name is required.")]
    [Display(Name = "Name of Business")]
    public string NameOfBusiness { get; set; } = "";

    [Required(ErrorMessage = "Owner CNIC is required.")]
    [RegularExpression(@"^\d{5}-\d{7}-\d$", ErrorMessage = "CNIC must be in format: xxxxx-xxxxxxx-x")]
    [Display(Name = "Owner CNIC")]
    public string OwnerCNIC { get; set; } = "";

    [Required(ErrorMessage = "Contact person is required.")]
    [Display(Name = "Contact Person")]
    public string ContactPerson { get; set; } = "";

    [Required(ErrorMessage = "Cell / Landline No. is required.")]
    [Display(Name = "Cell / Landline No.")]
    public string CellOrLandlineNo { get; set; } = "";

    [Display(Name = "NTN No. (If applicable)")]
    public string? NTNNo { get; set; }

    [Required(ErrorMessage = "Business address is required.")]
    [Display(Name = "Business Address")]
    public string BusinessAddress { get; set; } = "";

    [Required(ErrorMessage = "Annual sales is required.")]
    [Range(0, double.MaxValue, ErrorMessage = "Annual sales must be 0 or greater.")]
    [Display(Name = "Annual Sales (Rs.)")]
    public decimal AnnualSales { get; set; }

    [Required(ErrorMessage = "Year of establishment is required.")]
    [Range(1900, 9999, ErrorMessage = "Please enter a valid year.")]
    [Display(Name = "Year of Establishment")]
    public int YearOfEstablishment { get; set; }

    [Required(ErrorMessage = "Number of employees is required.")]
    [Range(0, int.MaxValue, ErrorMessage = "Employees must be 0 or greater.")]
    [Display(Name = "No. of Employees")]
    public int NoOfEmployees { get; set; }

    [Required(ErrorMessage = "Business premise is required.")]
    [Display(Name = "Business Premise")]
    public string BusinessPremise { get; set; } = "";

    [Required(ErrorMessage = "Business registration status is required.")]
    [Display(Name = "Business Registration")]
    public string IsBusinessRegistered { get; set; } = "No"; // "Yes" | "No"

    [Display(Name = "Registration Authority")]
    public string? RegistrationAuthority { get; set; }

    [Required(ErrorMessage = "Business status is required.")]
    [Display(Name = "Business Status")]
    public string BusinessStatus { get; set; } = "";

    [Required(ErrorMessage = "Business nature is required.")]
    [Display(Name = "Business Nature")]
    public string BusinessNature { get; set; } = "";

    [Required(ErrorMessage = "Business description is required.")]
    [Display(Name = "Business Description")]
    public string BusinessDescription { get; set; } = "";

    [Required(ErrorMessage = "Business email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Name = "Business Email Address")]
    public string BusinessEmailAddress { get; set; } = "";

    // Bank detail (optional)
    [Display(Name = "Bank")]
    public int? BusinessBankId { get; set; }

    [Display(Name = "Business IBAN")]
    [RegularExpression(@"^[A-Z]{2}\d{2}[A-Z0-9]{1,30}$", ErrorMessage = "IBAN format: PK36SCBL0000001123456702")]
    public string? BusinessIBAN { get; set; }

    public List<BusinessShareholderViewModel> Shareholders { get; set; } = new();

    // Lookup data (populated by controller — not bound from form)
    public List<MasterLookupValue> BusinessNatures { get; set; } = new();
    public List<MasterLookupValue> BusinessStatuses { get; set; } = new();
    public List<MasterLookupValue> BusinessPremises { get; set; } = new();
    public List<Bank> AvailableBanks { get; set; } = new();
}

public class BusinessProfileCreateViewModel : BusinessProfileFormViewModel { }

public class BusinessProfileEditViewModel : BusinessProfileFormViewModel
{
    public int Id { get; set; }
    public string CurrentCellOrLandlineNo { get; set; } = "";
    public string CurrentBusinessEmailAddress { get; set; } = "";
    public bool IsBusinessEmailVerified { get; set; }
    public bool IsBusinessMobileVerified { get; set; }
    public string BusinessVerificationStatus { get; set; } = "";
}

public class BusinessProfileDetailsViewModel
{
    public BusinessProfile Profile { get; set; } = null!;
}

public class BusinessVerifyViewModel
{
    public int BusinessProfileId { get; set; }
    public string MaskedMobile { get; set; } = "";
    public string MaskedEmail { get; set; } = "";
    public bool IsBusinessEmailVerified { get; set; }
    public bool IsBusinessMobileVerified { get; set; }
    public string BusinessVerificationStatus { get; set; } = "";
    public string MobileOtp { get; set; } = "";
    public string EmailOtp { get; set; } = "";
    public bool EmailResendCooldown { get; set; }
    public bool MobileResendCooldown { get; set; }
}

public class EndUserLoginViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = default!;
    [Required] public string Password { get; set; } = default!;
}

public class EndUserRegisterViewModel
{
    [Required] public string FullName { get; set; } = default!;
    [Required, EmailAddress] public string Email { get; set; } = default!;
    public string? MobileNo { get; set; }
    [Required, MinLength(6)] public string Password { get; set; } = default!;
    public bool AgreeTerms { get; set; }
}

public class ProfileCompleteViewModel
{
    [Required] public string FirstName { get; set; } = default!;
    [Required] public string LastName { get; set; } = default!;
    [Required] public string MobileNo { get; set; } = default!;
    [Required] public string CNIC { get; set; } = default!;
    [Required, EmailAddress] public string BusinessEmailAddress { get; set; } = default!;
    [Required] public string GenderOfProprietor { get; set; } = default!;
}

public class TokenVerifyViewModel
{
    [Required] public string Token { get; set; } = default!;
}

public class LoanRequestCreateViewModel
{
    // Business profile selector (optional — auto-fills matching fields)
    public int? BusinessProfileId { get; set; }

    // Dynamic form binding (populated when scheme is selected)
    public int? SchemeId { get; set; }
    public int? SchemeFormId { get; set; }
    public string? FieldValuesJson { get; set; }

    // Classic fixed fields (still used for model binding when dynamic form matches field names)
    public string? NameOfBusiness { get; set; }
    public string? ContactPerson { get; set; }
    public string? CellOrLandlineNo { get; set; }
    public string? BusinessAddress { get; set; }
    public decimal? AnnualSales { get; set; }
    public int? YearOfEstablishment { get; set; }
    public int? NoOfEmployees { get; set; }
    public string? NTNNo { get; set; }
    public string? BusinessPremise { get; set; }
    public bool IsBusinessRegistered { get; set; }
    public string? RegistrationAuthority { get; set; }
    public string? BusinessStatus { get; set; }
    public string? BusinessNature { get; set; }
    public string? BusinessDescription { get; set; }
    public string? FacilityRequested { get; set; }
    public string? TypeOfFacility { get; set; }
    public decimal? Amount { get; set; }
    public int? Tenor { get; set; }
    public int? AssignedBankId { get; set; }
    public string? IBANOrRaastType { get; set; }
    public string? IBANOrRaastValue { get; set; }
    public string? PreferredIdentifierType { get; set; }
    public bool ConsentGiven { get; set; }
    public bool SaveAsDraft { get; set; }
    public List<ShareholderRowViewModel> Shareholders { get; set; } = new();

    // Lookup data for GET
    public List<BusinessProfile> Businesses { get; set; } = new();
    public List<Scheme> Schemes { get; set; } = new();
    public List<Bank> Banks { get; set; } = new();
    public List<MasterLookupValue> BusinessNatures { get; set; } = new();
    public List<MasterLookupValue> FacilityRequestedOptions { get; set; } = new();
    public List<MasterLookupValue> FacilityTypes { get; set; } = new();
    public List<MasterLookupValue> BusinessStatuses { get; set; } = new();
    public List<MasterLookupValue> BusinessPremises { get; set; } = new();
    public List<MasterLookupValue> Tenors { get; set; } = new();
}

public class ShareholderRowViewModel
{
    public string Name { get; set; } = default!;
    public string? ContactNo { get; set; }
    public string? Email { get; set; }
    public string? CNIC { get; set; }
    public decimal ShareholdingPercentage { get; set; }
}

public class LoanApplicationDetailViewModel
{
    public LoanRequest Request { get; set; } = default!;
    public List<LoanRequestStatusHistory> StatusHistory { get; set; } = new();
    public List<AdditionalInformationRequest> InfoRequests { get; set; } = new();
    public ConditionalOffer? ActiveOffer { get; set; }
    public List<ConditionalOffer> AllOffers { get; set; } = new();
    public BankDecision? Decision { get; set; }
    public Disbursement? Disbursement { get; set; }
    public PostApprovalChecklist? Checklist { get; set; }
    public List<ApplicationDocument> Documents { get; set; } = new();
    public List<WorkflowTransition> AllowedTransitions { get; set; } = new();
}

public class InfoRequestResponseViewModel
{
    [Required] public int RequestId { get; set; }
    [Required, MaxLength(2000)] public string Response { get; set; } = default!;
    public List<IFormFile>? Documents { get; set; }
}

public class OfferResponseViewModel
{
    [Required] public int OfferId { get; set; }
    [Required] public string ResponseType { get; set; } = default!; // Accepted | Rejected
    public string? Remarks { get; set; }
}

public class EndUserDashboardViewModel
{
    public int TotalApplications { get; set; }
    public int DraftApplications { get; set; }
    public int SubmittedApplications { get; set; }
    public int PendingActions { get; set; }
    public int ActiveOffers { get; set; }
    public int DisbursedApplications { get; set; }
    public List<AppNotification> RecentNotifications { get; set; } = new();
    public List<LoanRequest> RecentApplications { get; set; } = new();
    public int UnreadNotifications { get; set; }
}
