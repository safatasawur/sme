using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Data;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Services.Implementations;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _ctx;

    public AuditService(ApplicationDbContext db, IHttpContextAccessor ctx) { _db = db; _ctx = ctx; }

    public async Task LogAsync(string action, string entityName, int? entityId = null,
        string? oldValue = null, string? newValue = null, int? userId = null)
    {
        var http = _ctx.HttpContext;
        int? resolvedUserId = userId;
        if (!resolvedUserId.HasValue && http?.User?.Identity?.IsAuthenticated == true)
        {
            var claim = http.User.FindFirst("UserId");
            if (claim is not null && int.TryParse(claim.Value, out var uid)) resolvedUserId = uid;
        }

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = resolvedUserId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValue = oldValue,
            NewValue = newValue,
            IPAddress = http?.Connection?.RemoteIpAddress?.ToString(),
            UserAgent = http?.Request?.Headers.UserAgent,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }
}

public class FileUploadService : IFileUploadService
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;
    private static readonly string[] Allowed = [".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx"];

    public FileUploadService(IConfiguration config, IWebHostEnvironment env) { _config = config; _env = env; }

    public async Task<(string FileName, string OriginalFileName, string FilePath, string ContentType, long FileSize)>
        UploadAsync(IFormFile file, string subfolder = "uploads")
    {
        var ext = Path.GetExtension(file.FileName).ToLower();
        var safeFileName = $"{Guid.NewGuid():N}{ext}";
        var uploadRoot = Path.Combine(_env.WebRootPath, subfolder);
        Directory.CreateDirectory(uploadRoot);
        var fullPath = Path.Combine(uploadRoot, safeFileName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return (safeFileName, file.FileName, $"/{subfolder}/{safeFileName}", file.ContentType, file.Length);
    }

    public bool IsAllowedType(string fileName) =>
        Allowed.Contains(Path.GetExtension(fileName).ToLower());

    public bool IsAllowedSize(long sizeBytes, int maxMB = 10) =>
        sizeBytes <= (long)maxMB * 1024 * 1024;
}

public class RequestNumberService : IRequestNumberService
{
    private readonly ApplicationDbContext _db;
    private static readonly SemaphoreSlim _lock = new(1, 1);

    public RequestNumberService(ApplicationDbContext db) => _db = db;

    public async Task<string> GenerateAsync()
    {
        await _lock.WaitAsync();
        try
        {
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var prefix = $"SME-{today}-";
            var lastToday = await _db.LoanRequests
                .Where(r => r.RequestNo.StartsWith(prefix))
                .OrderByDescending(r => r.RequestNo)
                .Select(r => r.RequestNo)
                .FirstOrDefaultAsync();

            int nextSeq = 1;
            if (lastToday is not null)
            {
                var parts = lastToday.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out var seq)) nextSeq = seq + 1;
            }
            return $"{prefix}{nextSeq:D6}";
        }
        finally { _lock.Release(); }
    }
}

public class SettingsService : ISettingsService
{
    private readonly ApplicationDbContext _db;

    public SettingsService(ApplicationDbContext db) => _db = db;

    public async Task<string?> GetValueAsync(string key)
    {
        var setting = await _db.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == key);
        return setting?.SettingValue;
    }

    public async Task SetValueAsync(string key, string value, SettingCategory category = SettingCategory.General, bool isEncrypted = false)
    {
        var setting = await _db.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == key);
        if (setting is null)
        {
            _db.SystemSettings.Add(new SystemSetting { SettingKey = key, SettingValue = value, SettingCategory = category, IsEncrypted = isEncrypted, CreatedAt = DateTime.UtcNow });
        }
        else { setting.SettingValue = value; setting.UpdatedAt = DateTime.UtcNow; }
        await _db.SaveChangesAsync();
    }

    public async Task<Dictionary<string, string?>> GetCategoryAsync(SettingCategory category)
    {
        var settings = await _db.SystemSettings.Where(s => s.SettingCategory == category).ToListAsync();
        return settings.ToDictionary(s => s.SettingKey, s => s.SettingValue);
    }

    public async Task SaveCategoryAsync(SettingCategory category, Dictionary<string, string?> values)
    {
        foreach (var (key, value) in values)
            await SetValueAsync(key, value ?? "", category);
    }
}
