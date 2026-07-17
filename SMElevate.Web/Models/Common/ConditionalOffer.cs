namespace SMElevate.Web.Models.Common;

public class ConditionalOffer
{
    public int Id { get; set; }
    public string OfferNumber { get; set; } = default!; // e.g. CO-2026-000001
    public int LoanRequestId { get; set; }
    public int OfferVersion { get; set; } = 1;
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public DateTime ExpiryDate { get; set; }

    // Offer terms
    public string? FacilityType { get; set; }
    public decimal ApprovedAmount { get; set; }
    public int TenorMonths { get; set; }
    public string? PricingSummary { get; set; }
    public string? TermsAndConditions { get; set; }
    public string? ConditionsPrecedent { get; set; }
    public string? OfferLetterPath { get; set; }

    public string Status { get; set; } = OfferStatus.Issued; // Issued | Accepted | Rejected | Expired | Superseded
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public LoanRequest LoanRequest { get; set; } = default!;
    public ApplicationUser CreatedBy { get; set; } = default!;
    public ConditionalOfferResponse? Response { get; set; }
}

public class ConditionalOfferResponse
{
    public int Id { get; set; }
    public int ConditionalOfferId { get; set; }
    public string ResponseType { get; set; } = default!; // Accepted | Rejected
    public DateTime ResponseDate { get; set; } = DateTime.UtcNow;
    public int RespondedByUserId { get; set; }
    public string? IpAddress { get; set; }
    public string? Remarks { get; set; }
    public int OfferVersion { get; set; }

    public ConditionalOffer ConditionalOffer { get; set; } = default!;
    public ApplicationUser RespondedBy { get; set; } = default!;
}

public static class OfferStatus
{
    public const string Issued = "Issued";
    public const string Accepted = "Accepted";
    public const string Rejected = "Rejected";
    public const string Expired = "Expired";
    public const string Superseded = "Superseded";
}
