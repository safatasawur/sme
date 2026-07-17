using SMElevate.Web.Models.Common;

namespace SMElevate.Web.Services.Interfaces;

public interface IBankService
{
    Task<List<Bank>> GetAllBanksAsync(bool activeOnly = false);
    Task<Bank?> GetByIdAsync(int id);
    Task<Bank> CreateBankAsync(Bank bank);
    Task<Bank> UpdateBankAsync(Bank bank);
    Task<bool> ToggleActiveAsync(int bankId);
    Task<List<ApplicationUser>> GetBankMembersAsync(int bankId);
    Task AssignMembersAsync(int bankId, IEnumerable<int> userIds);
    Task RemoveMembersAsync(int bankId, IEnumerable<int> userIds);
    Task<Bank?> GetBankByUserEmailAsync(string email);
}
