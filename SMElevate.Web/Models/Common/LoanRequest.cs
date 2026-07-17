namespace SMElevate.Web.Models.Common;

public class LoanRequest
{
    public int Id { get; set; }
    public string RequestNo { get; set; } = default!;
    public int UserId { get; set; }
    public int? AssignedBankId { get; set; }
    public int? StatusId { get; set; }

    // Business details
    public string NameOfBusiness { get; set; } = default!;
    public string ContactPerson { get; set; } = default!;
    public string CellOrLandlineNo { get; set; } = default!;
    public string BusinessAddress { get; set; } = default!;
    public decimal AnnualSales { get; set; }
    public int YearOfEstablishment { get; set; }
    public int NoOfEmployees { get; set; }
    public string? NTNNo { get; set; }
    public string BusinessPremise { get; set; } = default!;
    public bool IsBusinessRegistered { get; set; }
    public string? RegistrationAuthority { get; set; }
    public string BusinessStatus { get; set; } = default!;
    public string BusinessNature { get; set; } = default!;
    public string BusinessDescription { get; set; } = default!;

    // Facility details
    public string FacilityRequested { get; set; } = default!;
    public string TypeOfFacility { get; set; } = default!;
    public decimal Amount { get; set; }
    public int Tenor { get; set; }

    // Bank / payment identifier (extended)
    public string IBANOrRaastType { get; set; } = default!;
    public string IBANOrRaastValue { get; set; } = default!;
    public string? PreferredIdentifierType { get; set; } // IBAN | RAAST
    public int? IdentifiedBankId { get; set; }
    public string? AccountValidationStatus { get; set; } // Pending | Valid | Invalid | ManualVerified
    public string? AccountValidationMessage { get; set; }

    // Applicant consent
    public bool ConsentGiven { get; set; }
    public DateTime? ConsentDate { get; set; }
    public string? ConsentIpAddress { get; set; }
    public string? ConsentVersion { get; set; }

    // Case ID (immutable, generated on first submission)
    public string? CaseId { get; set; }
    public bool IsDraft { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Dynamic form links
    public int? SchemeId { get; set; }
    public int? SchemeFormId { get; set; }

    // Business profile reference (nullable — historical requests have no profile)
    public int? BusinessProfileId { get; set; }
    // Snapshot of owner CNIC at submission time
    public string? OwnerCNIC { get; set; }

    // Navigation
    public ApplicationUser User { get; set; } = default!;
    public Bank? AssignedBank { get; set; }
    public Bank? IdentifiedBank { get; set; }
    public BusinessProfile? BusinessProfile { get; set; }
    public MasterLookupValue? Status { get; set; }
    public Scheme? Scheme { get; set; }
    public SchemeForm? SchemeForm { get; set; }
    public ICollection<LoanRequestShareholder> Shareholders { get; set; } = new List<LoanRequestShareholder>();
    public ICollection<LoanRequestStatusHistory> StatusHistory { get; set; } = new List<LoanRequestStatusHistory>();
    public ICollection<LoanRequestAttachment> Attachments { get; set; } = new List<LoanRequestAttachment>();
    public ICollection<LoanRequestFieldValue> FieldValues { get; set; } = new List<LoanRequestFieldValue>();
    public ICollection<BankAssessment> BankAssessments { get; set; } = new List<BankAssessment>();
    public ICollection<AdditionalInformationRequest> AdditionalInfoRequests { get; set; } = new List<AdditionalInformationRequest>();
    public ICollection<ApplicationDocument> Documents { get; set; } = new List<ApplicationDocument>();
    public ICollection<ConditionalOffer> ConditionalOffers { get; set; } = new List<ConditionalOffer>();
    public ICollection<PostApprovalChecklist> PostApprovalChecklists { get; set; } = new List<PostApprovalChecklist>();
    public BankDecision? BankDecision { get; set; }
    public Disbursement? Disbursement { get; set; }
    public ApplicationMonitoring? Monitoring { get; set; }
}

public class LoanRequestShareholder
{
    public int Id { get; set; }
    public int LoanRequestId { get; set; }
    public string Name { get; set; } = default!;
    public string? ContactNo { get; set; }
    public string? Email { get; set; }
    public string? CNIC { get; set; }
    public decimal ShareholdingPercentage { get; set; }

    public LoanRequest LoanRequest { get; set; } = default!;
}

public class LoanRequestStatusHistory
{
    public int Id { get; set; }
    public int LoanRequestId { get; set; }
    public int? OldStatusId { get; set; }
    public int NewStatusId { get; set; }
    public int ChangedByUserId { get; set; }
    public string? ActorType { get; set; } // EndUser | Bank | Admin | System
    public string? Remarks { get; set; }
    public string? ReasonCode { get; set; }
    public string? IpAddress { get; set; }
    public string? AttachmentPath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public LoanRequest LoanRequest { get; set; } = default!;
    public MasterLookupValue? OldStatus { get; set; }
    public MasterLookupValue NewStatus { get; set; } = default!;
    public ApplicationUser ChangedBy { get; set; } = default!;
}

public class LoanRequestFieldValue
{
    public int Id { get; set; }
    public int LoanRequestId { get; set; }
    public int? SchemeFormFieldId { get; set; }
    public string FieldName { get; set; } = default!;
    public string? FieldValue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public LoanRequest LoanRequest { get; set; } = default!;
}

public class LoanRequestAttachment
{
    public int Id { get; set; }
    public int LoanRequestId { get; set; }
    public int UploadedByUserId { get; set; }
    public string FileName { get; set; } = default!;
    public string OriginalFileName { get; set; } = default!;
    public string FilePath { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public long FileSize { get; set; }
    public string AttachmentType { get; set; } = "BankDecision";
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public LoanRequest LoanRequest { get; set; } = default!;
    public ApplicationUser UploadedBy { get; set; } = default!;
}
