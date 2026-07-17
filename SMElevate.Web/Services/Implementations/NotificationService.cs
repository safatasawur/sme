using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Data;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _db;
    public NotificationService(ApplicationDbContext db) => _db = db;

    public async Task CreateAsync(int userId, string title, string message,
        NotificationType type = NotificationType.Info, string? entityType = null, int? entityId = null)
    {
        _db.AppNotifications.Add(new AppNotification
        {
            UserId = userId,
            Title = title,
            Message = message,
            NotificationType = type,
            RelatedEntityType = entityType,
            RelatedEntityId = entityId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    public async Task<List<AppNotification>> GetForUserAsync(int userId, int take = 20) =>
        await _db.AppNotifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync();

    public async Task<int> GetUnreadCountAsync(int userId) =>
        await _db.AppNotifications.CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task MarkReadAsync(int notificationId, int userId)
    {
        var n = await _db.AppNotifications.FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
        if (n is not null && !n.IsRead) { n.IsRead = true; n.ReadAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
    }

    public async Task MarkAllReadAsync(int userId)
    {
        var unread = await _db.AppNotifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        unread.ForEach(n => { n.IsRead = true; n.ReadAt = DateTime.UtcNow; });
        await _db.SaveChangesAsync();
    }
}
