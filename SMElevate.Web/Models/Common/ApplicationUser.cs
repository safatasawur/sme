using Microsoft.AspNetCore.Identity;

namespace SMElevate.Web.Models.Common;

public class ApplicationUser
{
    public int Id { get; set; }
    public string FullName { get; set; } = default!;
    public string EmailAddress { get; set; } = default!;
    public string? MobileNo { get; set; }
    public string PasswordHash { get; set; } = default!;
    public UserType UserType { get; set; } = UserType.SME;
    public int? RoleId { get; set; }
    public int? BankId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; } = false;
    public bool IsMobileVerified { get; set; } = false;
    public string? AuthenticationMode { get; set; } // Manual, OTP, Google, Apple, Microsoft
    public string? OtpCode { get; set; }
    public DateTime? OtpExpiry { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public Role? Role { get; set; }
    public Bank? Bank { get; set; }
    public EndUserProfile? Profile { get; set; }
    public ICollection<BankMember> BankMemberships { get; set; } = new List<BankMember>();
    public ICollection<AppNotification> Notifications { get; set; } = new List<AppNotification>();
    public ICollection<LoanRequest> LoanRequests { get; set; } = new List<LoanRequest>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
