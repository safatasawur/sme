namespace SMElevate.Web.Models.Common;

public class ApplicationDocument
{
    public int Id { get; set; }
    public int LoanRequestId { get; set; }
    public string DocumentType { get; set; } = default!; // InfoResponse | PostApproval | SupportingDoc | BankDecisionAttachment
    public int? AdditionalInfoRequestId { get; set; }
    public int? ChecklistItemId { get; set; }

    public string FileName { get; set; } = default!;
    public string OriginalFileName { get; set; } = default!;
    public string FilePath { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public long FileSize { get; set; }
    public string? Description { get; set; }

    public int UploadedByUserId { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public LoanRequest LoanRequest { get; set; } = default!;
    public ApplicationUser UploadedBy { get; set; } = default!;
    public AdditionalInformationRequest? AdditionalInfoRequest { get; set; }
    public PostApprovalChecklistItem? ChecklistItem { get; set; }
}

public static class DocumentType
{
    public const string InfoResponse = "InfoResponse";
    public const string PostApproval = "PostApproval";
    public const string SupportingDoc = "SupportingDoc";
    public const string BankDecisionAttachment = "BankDecisionAttachment";
    public const string OfferLetter = "OfferLetter";
}
