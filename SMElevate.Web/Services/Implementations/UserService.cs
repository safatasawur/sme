using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Data;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Services.Implementations;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _db;
    private readonly IAuthService _auth;

    public UserService(ApplicationDbContext db, IAuthService auth)
    {
        _db = db;
        _auth = auth;
    }

    public async Task<List<ApplicationUser>> GetAllUsersAsync() =>
        await _db.Users.Include(u => u.Role).Include(u => u.Bank).Include(u => u.Profile)
            .OrderBy(u => u.FullName).ToListAsync();

    public async Task<ApplicationUser?> GetUserByIdAsync(int id) =>
        await _db.Users.Include(u => u.Role).Include(u => u.Bank).Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == id);

    public async Task<ApplicationUser?> GetUserByEmailAsync(string email) =>
        await _db.Users.Include(u => u.Role).Include(u => u.Bank).Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.EmailAddress.ToLower() == email.ToLower());

    public async Task<ApplicationUser> CreateUserAsync(ApplicationUser user, string plainPassword)
    {
        user.PasswordHash = _auth.HashPassword(plainPassword);
        user.CreatedAt = DateTime.UtcNow;
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<ApplicationUser> UpdateUserAsync(ApplicationUser user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<bool> ActivateDeactivateAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return false;
        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return user.IsActive;
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        var query = _db.Users.Where(u => u.EmailAddress.ToLower() == email.ToLower());
        if (excludeId.HasValue) query = query.Where(u => u.Id != excludeId.Value);
        return await query.AnyAsync();
    }
}
