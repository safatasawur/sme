using SMElevate.Web.Models.Common;

namespace SMElevate.Web.Services.Interfaces;

public interface IRoleService
{
    Task<List<Role>> GetAllRolesAsync();
    Task<Role?> GetByIdAsync(int id);
    Task<Role> CreateRoleAsync(Role role);
    Task<Role> UpdateRoleAsync(Role role);
    Task<bool> ToggleActiveAsync(int roleId);
}
