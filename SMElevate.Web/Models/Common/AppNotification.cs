namespace SMElevate.Web.Models.Common;

public class AppNotification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public NotificationType NotificationType { get; set; } = NotificationType.Info;
    public string? RelatedEntityType { get; set; }
    public int? RelatedEntityId { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }

    public ApplicationUser User { get; set; } = default!;
}
