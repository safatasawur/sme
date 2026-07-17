using System.Security.Claims;
using SMElevate.Web.Models.Common;

namespace SMElevate.Web.Services.Interfaces;

public interface IAuthService
{
    Task<ApplicationUser?> ValidateAdminLoginAsync(string email, string password);
    Task<ApplicationUser?> ValidateBankEmailAsync(string email);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    ClaimsPrincipal CreatePrincipal(ApplicationUser user);
    Task UpdateLastLoginAsync(int userId);
}
