namespace SMElevate.Web.Models.Common;

public class AdditionalInformationRequest
{
    public int Id { get; set; }
    public int LoanRequestId { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? RequiredDocuments { get; set; }
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = AdditionalInfoStatus.Pending; // Pending | Responded | Closed
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Applicant response
    public string? ApplicantResponse { get; set; }
    public DateTime? ResponseDate { get; set; }
    public int? RespondedByUserId { get; set; }

    public LoanRequest LoanRequest { get; set; } = default!;
    public ApplicationUser CreatedBy { get; set; } = default!;
    public ApplicationUser? RespondedBy { get; set; }

    public ICollection<ApplicationDocument> ResponseDocuments { get; set; } = new List<ApplicationDocument>();
}

public static class AdditionalInfoStatus
{
    public const string Pending = "Pending";
    public const string Responded = "Responded";
    public const string Closed = "Closed";
}
