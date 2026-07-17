using SMElevate.Web.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace SMElevate.Web.Areas.Banks.ViewModels;

public class BankDecisionViewModel
{
    public int RequestId { get; set; }
    public string RequestNo { get; set; } = default!;
    [Required] public int NewStatusId { get; set; }
    public string? Remarks { get; set; }
    public IFormFile? Attachment { get; set; }
    public List<MasterLookupValue> Statuses { get; set; } = new();
    public LoanRequest? Request { get; set; }
}

public class BankLoginViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = default!;
}

public class OtpVerifyViewModel
{
    [Required] public string Otp { get; set; } = default!;
    public string? PendingEmail { get; set; }
}

public class BankApplicationDetailViewModel
{
    public LoanRequest Request { get; set; } = default!;
    public List<LoanRequestStatusHistory> StatusHistory { get; set; } = new();
    public List<BankAssessment> Assessments { get; set; } = new();
    public List<AdditionalInformationRequest> InfoRequests { get; set; } = new();
    public BankDecision? Decision { get; set; }
    public List<ConditionalOffer> Offers { get; set; } = new();
    public PostApprovalChecklist? Checklist { get; set; }
    public Disbursement? Disbursement { get; set; }
    public List<WorkflowTransition> AllowedTransitions { get; set; } = new();
    public List<DeclineReasonCode> DeclineReasonCodes { get; set; } = new();
}

public class BankAssessmentFormViewModel
{
    public int LoanRequestId { get; set; }
    public string AssessmentType { get; set; } = default!;
    public string Status { get; set; } = "InProgress";
    public DateTime? CheckDate { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? ResultSummary { get; set; }
    public string? ScorecardReference { get; set; }
    public string? Result { get; set; }
    public string? RiskCategory { get; set; }
    public DateTime? AssessmentDate { get; set; }
    public string? CDDStatus { get; set; }
    public string? KYCStatus { get; set; }
    public string? AMLStatus { get; set; }
    public string? SanctionsScreeningStatus { get; set; }
    public string? PEPScreeningStatus { get; set; }
    public string? ComplianceResult { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string? Remarks { get; set; }
    public IFormFile? Attachment { get; set; }
}

public class BankFormalDecisionViewModel
{
    [Required] public int LoanRequestId { get; set; }
    [Required] public string DecisionType { get; set; } = default!;
    public string? DecisionRemarks { get; set; }
    public int? DeclineReasonCodeId { get; set; }
    public string? ApprovedFacilityType { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public int? ApprovedTenorMonths { get; set; }
    public string? AdditionalConditions { get; set; }
    public List<DeclineReasonCode> ReasonCodes { get; set; } = new();
}

public class CreateInfoRequestViewModel
{
    [Required] public int LoanRequestId { get; set; }
    [Required, MaxLength(200)] public string Title { get; set; } = default!;
    [Required] public string Description { get; set; } = default!;
    public string? RequiredDocuments { get; set; }
    public DateTime? DueDate { get; set; }
}

public class CreateConditionalOfferViewModel
{
    [Required] public int LoanRequestId { get; set; }
    [Required] public DateTime ExpiryDate { get; set; }
    public string? FacilityType { get; set; }
    [Required] public decimal ApprovedAmount { get; set; }
    [Required] public int TenorMonths { get; set; }
    public string? PricingSummary { get; set; }
    public string? TermsAndConditions { get; set; }
    public string? ConditionsPrecedent { get; set; }
    public IFormFile? OfferLetterFile { get; set; }
}

public class CreateChecklistViewModel
{
    [Required] public int LoanRequestId { get; set; }
    [Required, MaxLength(200)] public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public List<ChecklistItemFormRow> Items { get; set; } = new();
}

public class ChecklistItemFormRow
{
    [Required] public string ItemName { get; set; } = default!;
    public string? ItemDescription { get; set; }
    public bool IsRequired { get; set; } = true;
    public bool DocumentRequired { get; set; }
    public DateTime? DueDate { get; set; }
}

public class RecordDisbursementViewModel
{
    [Required] public int LoanRequestId { get; set; }
    public string DisbursementStatus { get; set; } = "FullyDisbursed";
    [Required] public decimal ApprovedAmount { get; set; }
    public decimal? DisbursedAmount { get; set; }
    public DateTime? ValueDate { get; set; }
    public string? DisbursementAccount { get; set; }
    public string? BankReferenceNumber { get; set; }
    public string? Remarks { get; set; }
}

public class BankDashboardViewModel
{
    public int AssignedApplications { get; set; }
    public int NewReferrals { get; set; }
    public int UnderReview { get; set; }
    public int AdditionalInfoPending { get; set; }
    public int ConditionalOffers { get; set; }
    public int Approved { get; set; }
    public int Declined { get; set; }
    public int ReadyForDisbursement { get; set; }
    public int Disbursed { get; set; }
    public double AvgTurnaroundDays { get; set; }
    public List<LoanRequest> RecentApplications { get; set; } = new();
}
