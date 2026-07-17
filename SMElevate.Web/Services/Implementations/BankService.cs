using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Data;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Services.Implementations;

public class BankService : IBankService
{
    private readonly ApplicationDbContext _db;
    public BankService(ApplicationDbContext db) => _db = db;

    public async Task<List<Bank>> GetAllBanksAsync(bool activeOnly = false)
    {
        var query = _db.Banks.AsQueryable();
        if (activeOnly) query = query.Where(b => b.IsActive);
        return await query.OrderBy(b => b.BankName).ToListAsync();
    }

    public async Task<Bank?> GetByIdAsync(int id) =>
        await _db.Banks.Include(b => b.Members).ThenInclude(m => m.User).FirstOrDefaultAsync(b => b.Id == id);

    public async Task<Bank> CreateBankAsync(Bank bank)
    {
        bank.CreatedAt = DateTime.UtcNow;
        _db.Banks.Add(bank);
        await _db.SaveChangesAsync();
        return bank;
    }

    public async Task<Bank> UpdateBankAsync(Bank bank)
    {
        bank.UpdatedAt = DateTime.UtcNow;
        _db.Banks.Update(bank);
        await _db.SaveChangesAsync();
        return bank;
    }

    public async Task<bool> ToggleActiveAsync(int bankId)
    {
        var bank = await _db.Banks.FindAsync(bankId);
        if (bank is null) return false;
        bank.IsActive = !bank.IsActive;
        bank.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return bank.IsActive;
    }

    public async Task<List<ApplicationUser>> GetBankMembersAsync(int bankId)
    {
        return await _db.BankMembers
            .Where(m => m.BankId == bankId && m.IsActive)
            .Select(m => m.User)
            .ToListAsync();
    }

    public async Task AssignMembersAsync(int bankId, IEnumerable<int> userIds)
    {
        foreach (var userId in userIds)
        {
            var exists = await _db.BankMembers.AnyAsync(m => m.BankId == bankId && m.UserId == userId);
            if (!exists)
            {
                _db.BankMembers.Add(new BankMember { BankId = bankId, UserId = userId, IsActive = true, AssignedAt = DateTime.UtcNow });
                var user = await _db.Users.FindAsync(userId);
                if (user is not null) { user.BankId = bankId; user.UpdatedAt = DateTime.UtcNow; }
            }
            else
            {
                var member = await _db.BankMembers.FirstAsync(m => m.BankId == bankId && m.UserId == userId);
                member.IsActive = true;
            }
        }
        await _db.SaveChangesAsync();
    }

    public async Task RemoveMembersAsync(int bankId, IEnumerable<int> userIds)
    {
        var members = await _db.BankMembers.Where(m => m.BankId == bankId && userIds.Contains(m.UserId)).ToListAsync();
        foreach (var member in members)
        {
            member.IsActive = false;
            var user = await _db.Users.FindAsync(member.UserId);
            if (user is not null && user.BankId == bankId) { user.BankId = null; user.UpdatedAt = DateTime.UtcNow; }
        }
        await _db.SaveChangesAsync();
    }

    public async Task<Bank?> GetBankByUserEmailAsync(string email)
    {
        var user = await _db.Users.Include(u => u.Bank)
            .FirstOrDefaultAsync(u => u.EmailAddress.ToLower() == email.ToLower() && u.BankId != null);
        return user?.Bank;
    }
}
