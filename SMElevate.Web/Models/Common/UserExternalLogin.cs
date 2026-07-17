namespace SMElevate.Web.Models.Common;

public class UserExternalLogin
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Provider { get; set; } = default!;        // "Google" | "Apple" | "Microsoft"
    public string ProviderUserId { get; set; } = default!; // sub claim from provider
    public string? ProviderEmail { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    public ApplicationUser User { get; set; } = default!;
}
