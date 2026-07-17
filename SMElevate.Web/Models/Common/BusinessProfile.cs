namespace SMElevate.Web.Models.Common;

public static class BusinessVerificationStatus
{
    public const string Pending           = "Pending";
    public const string PartiallyVerified = "PartiallyVerified";
    public const string Verified          = "Verified";
    public const string VerificationExpired = "VerificationExpired";
}

public class BusinessProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }

    // ── Core business details ─────────────────────────────────────────────────
    public string NameOfBusiness { get; set; } = "";
    public string OwnerCNIC { get; set; } = "";
    public string ContactPerson { get; set; } = "";
    public string CellOrLandlineNo { get; set; } = "";
    public string? NTNNo { get; set; }
    public string BusinessAddress { get; set; } = "";
    public decimal AnnualSales { get; set; }
    public int YearOfEstablishment { get; set; }
    public int NoOfEmployees { get; set; }
    public string BusinessPremise { get; set; } = "";
    public bool IsBusinessRegistered { get; set; }
    public string? RegistrationAuthority { get; set; }
    public string BusinessStatus { get; set; } = "";
    public string BusinessNature { get; set; } = "";
    public string BusinessDescription { get; set; } = "";
    public string BusinessEmailAddress { get; set; } = "";

    // ── Bank detail ───────────────────────────────────────────────────────────
    public int? BusinessBankId { get; set; }
    public string? BusinessIBAN { get; set; }

    // ── Verification state ────────────────────────────────────────────────────
    public bool IsBusinessEmailVerified { get; set; }
    public bool IsBusinessMobileVerified { get; set; }
    public string BusinessVerificationStatus { get; set; } = Models.Common.BusinessVerificationStatus.Pending;
    public DateTime? BusinessVerifiedAt { get; set; }

    // ── Email OTP ─────────────────────────────────────────────────────────────
    public string? BusinessEmailOtpCode { get; set; }
    public DateTime? BusinessEmailOtpExpiry { get; set; }
    public int EmailOtpAttempts { get; set; }
    public int EmailResendCount { get; set; }
    public DateTime? LastEmailOtpSentAt { get; set; }

    // ── Mobile OTP ────────────────────────────────────────────────────────────
    public string? BusinessMobileOtpCode { get; set; }
    public DateTime? BusinessMobileOtpExpiry { get; set; }
    public int MobileOtpAttempts { get; set; }
    public int MobileResendCount { get; set; }
    public DateTime? LastMobileOtpSentAt { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────────
    public ApplicationUser User { get; set; } = null!;
    public Bank? BusinessBank { get; set; }
    public ICollection<BusinessShareholder> Shareholders { get; set; } = new List<BusinessShareholder>();
    public ICollection<LoanRequest> LoanRequests { get; set; } = new List<LoanRequest>();
}

public class BusinessShareholder
{
    public int Id { get; set; }
    public int BusinessProfileId { get; set; }
    public string Name { get; set; } = "";
    public string? ContactNo { get; set; }
    public string? Email { get; set; }
    public string? CNIC { get; set; }
    public decimal ShareholdingPercentage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public BusinessProfile BusinessProfile { get; set; } = null!;
}
