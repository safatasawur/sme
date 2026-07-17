using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Data;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Services.Implementations;

public class RoleService : IRoleService
{
    private readonly ApplicationDbContext _db;
    public RoleService(ApplicationDbContext db) => _db = db;

    private static readonly string[] DefaultRoles = ["EndUser", "Bank", "SBP", "Admin"];

    public async Task<List<Role>> GetAllRolesAsync() =>
        await _db.Roles.OrderBy(r => r.RoleName).ToListAsync();

    public async Task<Role?> GetByIdAsync(int id) =>
        await _db.Roles.FindAsync(id);

    public async Task<Role> CreateRoleAsync(Role role)
    {
        role.CreatedAt = DateTime.UtcNow;
        _db.Roles.Add(role);
        await _db.SaveChangesAsync();
        return role;
    }

    public async Task<Role> UpdateRoleAsync(Role role)
    {
        _db.Roles.Update(role);
        await _db.SaveChangesAsync();
        return role;
    }

    public async Task<bool> ToggleActiveAsync(int roleId)
    {
        var role = await _db.Roles.FindAsync(roleId);
        if (role is null) return false;
        if (DefaultRoles.Contains(role.RoleName)) return role.IsActive;
        role.IsActive = !role.IsActive;
        await _db.SaveChangesAsync();
        return role.IsActive;
    }
}
