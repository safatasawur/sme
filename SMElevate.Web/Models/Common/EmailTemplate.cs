namespace SMElevate.Web.Models.Common;

public class EmailTemplate
{
    public int Id { get; set; }
    public string TemplateCode { get; set; } = default!;
    public string TemplateName { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public string BodyHtml { get; set; } = default!;
    public EmailRecipientType RecipientType { get; set; } = EmailRecipientType.EndUser;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class EmailLog
{
    public int Id { get; set; }
    public string ToEmail { get; set; } = default!;
    public string? CcEmail { get; set; }
    public string Subject { get; set; } = default!;
    public string BodyHtml { get; set; } = default!;
    public string? TemplateCode { get; set; }
    public EmailStatus Status { get; set; } = EmailStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
