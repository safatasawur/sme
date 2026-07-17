using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Data;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _db;
    private readonly PasswordHasher<ApplicationUser> _hasher = new();

    public AuthService(ApplicationDbContext db) => _db = db;

    public async Task<ApplicationUser?> ValidateAdminLoginAsync(string email, string password)
    {
        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.EmailAddress.ToLower() == email.ToLower()
                && u.UserType == UserType.Admin
                && u.IsActive);

        if (user is null) return null;
        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Failed ? null : user;
    }

    public async Task<ApplicationUser?> ValidateBankEmailAsync(string email)
    {
        return await _db.Users
            .Include(u => u.Bank)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.EmailAddress.ToLower() == email.ToLower()
                && u.UserType == UserType.Bank
                && u.IsActive
                && u.BankId != null);
    }

    public string HashPassword(string password)
    {
        var dummy = new ApplicationUser();
        return _hasher.HashPassword(dummy, password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        var dummy = new ApplicationUser { PasswordHash = hash };
        var result = _hasher.VerifyHashedPassword(dummy, hash, password);
        return result != PasswordVerificationResult.Failed;
    }

    public ClaimsPrincipal CreatePrincipal(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.EmailAddress),
            new("UserType", user.UserType.ToString()),
            new("UserId", user.Id.ToString()),
        };
        if (user.BankId.HasValue)
            claims.Add(new Claim("BankId", user.BankId.Value.ToString()));
        if (user.Role is not null)
            claims.Add(new Claim(ClaimTypes.Role, user.Role.RoleName));

        var identity = new ClaimsIdentity(claims,
            Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }

    public async Task UpdateLastLoginAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is not null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}
