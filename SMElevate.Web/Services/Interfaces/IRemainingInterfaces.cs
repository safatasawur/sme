using Microsoft.AspNetCore.Http;
using SMElevate.Web.Models.Common;

namespace SMElevate.Web.Services.Interfaces;

public interface IAuditService
{
    Task LogAsync(string action, string entityName, int? entityId = null, string? oldValue = null, string? newValue = null, int? userId = null);
}

public interface IFileUploadService
{
    Task<(string FileName, string OriginalFileName, string FilePath, string ContentType, long FileSize)> UploadAsync(IFormFile file, string subfolder = "uploads");
    bool IsAllowedType(string fileName);
    bool IsAllowedSize(long sizeBytes, int maxMB = 10);
}

public interface IRequestNumberService
{
    Task<string> GenerateAsync();
}

public interface ISettingsService
{
    Task<string?> GetValueAsync(string key);
    Task SetValueAsync(string key, string value, SettingCategory category = SettingCategory.General, bool isEncrypted = false);
    Task<Dictionary<string, string?>> GetCategoryAsync(SettingCategory category);
    Task SaveCategoryAsync(SettingCategory category, Dictionary<string, string?> values);
}
