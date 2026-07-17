using SMElevate.Web.Models.Common;

namespace SMElevate.Web.Services.Interfaces;

public interface IUserService
{
    Task<List<ApplicationUser>> GetAllUsersAsync();
    Task<ApplicationUser?> GetUserByIdAsync(int id);
    Task<ApplicationUser?> GetUserByEmailAsync(string email);
    Task<ApplicationUser> CreateUserAsync(ApplicationUser user, string plainPassword);
    Task<ApplicationUser> UpdateUserAsync(ApplicationUser user);
    Task<bool> ActivateDeactivateAsync(int userId);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
}
