namespace SMElevate.Web.Models.Common;

public class SystemSetting
{
    public int Id { get; set; }
    public string SettingKey { get; set; } = default!;
    public string? SettingValue { get; set; }
    public SettingCategory SettingCategory { get; set; } = SettingCategory.General;
    public bool IsEncrypted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
