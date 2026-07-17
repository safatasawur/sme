namespace SMElevate.Web.Models.Common;

public class BankAssessment
{
    public int Id { get; set; }
    public int LoanRequestId { get; set; }
    public string AssessmentType { get; set; } = default!; // CreditBureauCheck | RiskAssessment | CDDCompliance
    public string Status { get; set; } = "Pending"; // Pending | InProgress | Completed | Failed | NotRequired

    // Credit Bureau Check
    public DateTime? CheckDate { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? ResultSummary { get; set; }

    // Risk Assessment
    public string? ScorecardReference { get; set; }
    public string? Result { get; set; }
    public string? RiskCategory { get; set; } // Low | Medium | High
    public DateTime? AssessmentDate { get; set; }

    // CDD / KYC / AML / Compliance
    public string? CDDStatus { get; set; }
    public string? KYCStatus { get; set; }
    public string? AMLStatus { get; set; }
    public string? SanctionsScreeningStatus { get; set; }
    public string? PEPScreeningStatus { get; set; }
    public string? ComplianceResult { get; set; }
    public DateTime? CompletionDate { get; set; }

    // Common
    public string? Remarks { get; set; }
    public string? AttachmentPath { get; set; }
    public int? UpdatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public LoanRequest LoanRequest { get; set; } = default!;
    public ApplicationUser? UpdatedBy { get; set; }
}

public static class AssessmentType
{
    public const string CreditBureauCheck = "CreditBureauCheck";
    public const string RiskAssessment = "RiskAssessment";
    public const string CDDCompliance = "CDDCompliance";
}

public static class AssessmentStatus
{
    public const string Pending = "Pending";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
    public const string NotRequired = "NotRequired";
}
