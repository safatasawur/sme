using SMElevate.Web.Models.Common;

namespace SMElevate.Web.Services.Interfaces;

public interface INotificationService
{
    Task CreateAsync(int userId, string title, string message, NotificationType type = NotificationType.Info, string? entityType = null, int? entityId = null);
    Task<List<AppNotification>> GetForUserAsync(int userId, int take = 20);
    Task<int> GetUnreadCountAsync(int userId);
    Task MarkReadAsync(int notificationId, int userId);
    Task MarkAllReadAsync(int userId);
}
