namespace SMElevate.Web.Models.Common;

public class PostApprovalChecklist
{
    public int Id { get; set; }
    public int LoanRequestId { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public LoanRequest LoanRequest { get; set; } = default!;
    public ApplicationUser CreatedBy { get; set; } = default!;
    public ICollection<PostApprovalChecklistItem> Items { get; set; } = new List<PostApprovalChecklistItem>();
}

public class PostApprovalChecklistItem
{
    public int Id { get; set; }
    public int ChecklistId { get; set; }
    public string ItemName { get; set; } = default!;
    public string? ItemDescription { get; set; }
    public bool IsRequired { get; set; } = true;
    public bool DocumentRequired { get; set; }
    public DateTime? DueDate { get; set; }

    // Applicant submission
    public DateTime? SubmittedAt { get; set; }
    public string? SubmittedDocumentPath { get; set; }
    public int? SubmittedByUserId { get; set; }

    // Bank verification
    public string VerificationStatus { get; set; } = ChecklistItemStatus.Pending;
    public int? VerifiedByUserId { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? VerificationRemarks { get; set; }

    public PostApprovalChecklist Checklist { get; set; } = default!;
    public ApplicationUser? SubmittedBy { get; set; }
    public ApplicationUser? VerifiedBy { get; set; }
}

public static class ChecklistItemStatus
{
    public const string Pending = "Pending";
    public const string Submitted = "Submitted";
    public const string Verified = "Verified";
    public const string Rejected = "Rejected";
    public const string Waived = "Waived";
}
