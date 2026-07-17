namespace SMElevate.Web.Models.Common;

public class BankDecision
{
    public int Id { get; set; }
    public int LoanRequestId { get; set; }
    public string DecisionType { get; set; } = default!; // Approved | ConditionallyApproved | Declined
    public DateTime DecisionDate { get; set; } = DateTime.UtcNow;
    public string? DecisionRemarks { get; set; }
    public int? DeclineReasonCodeId { get; set; }

    // Approval details
    public string? ApprovedFacilityType { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public int? ApprovedTenorMonths { get; set; }
    public string? AdditionalConditions { get; set; }

    public int MadeByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public LoanRequest LoanRequest { get; set; } = default!;
    public DeclineReasonCode? DeclineReasonCode { get; set; }
    public ApplicationUser MadeBy { get; set; } = default!;
}

public static class DecisionType
{
    public const string Approved = "Approved";
    public const string ConditionallyApproved = "ConditionallyApproved";
    public const string Declined = "Declined";
}
