using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Data;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Services.Implementations;

public class LookupService : ILookupService
{
    private readonly ApplicationDbContext _db;
    public LookupService(ApplicationDbContext db) => _db = db;

    public async Task<List<MasterLookup>> GetAllLookupsAsync() =>
        await _db.MasterLookups.Include(l => l.Values.Where(v => v.IsActive).OrderBy(v => v.DisplayOrder))
            .OrderBy(l => l.LookupName).ToListAsync();

    public async Task<MasterLookup?> GetByIdAsync(int id) =>
        await _db.MasterLookups.Include(l => l.Values.OrderBy(v => v.DisplayOrder)).FirstOrDefaultAsync(l => l.Id == id);

    public async Task<MasterLookup?> GetByCodeAsync(string code) =>
        await _db.MasterLookups.Include(l => l.Values.Where(v => v.IsActive).OrderBy(v => v.DisplayOrder))
            .FirstOrDefaultAsync(l => l.LookupCode == code);

    public async Task<List<MasterLookupValue>> GetValuesAsync(string lookupCode, bool activeOnly = true)
    {
        var query = _db.MasterLookupValues.Include(v => v.MasterLookup)
            .Where(v => v.MasterLookup.LookupCode == lookupCode);
        if (activeOnly) query = query.Where(v => v.IsActive);
        return await query.OrderBy(v => v.DisplayOrder).ToListAsync();
    }

    public async Task<MasterLookupValue?> GetValueByIdAsync(int id) =>
        await _db.MasterLookupValues.Include(v => v.MasterLookup).FirstOrDefaultAsync(v => v.Id == id);

    public async Task<MasterLookup> CreateLookupAsync(MasterLookup lookup)
    {
        lookup.CreatedAt = DateTime.UtcNow;
        _db.MasterLookups.Add(lookup);
        await _db.SaveChangesAsync();
        return lookup;
    }

    public async Task<MasterLookup> UpdateLookupAsync(MasterLookup lookup)
    {
        _db.MasterLookups.Update(lookup);
        await _db.SaveChangesAsync();
        return lookup;
    }

    public async Task<bool> ToggleLookupActiveAsync(int id)
    {
        var lookup = await _db.MasterLookups.FindAsync(id);
        if (lookup is null) return false;
        lookup.IsActive = !lookup.IsActive;
        await _db.SaveChangesAsync();
        return lookup.IsActive;
    }

    public async Task<MasterLookupValue> SaveValueAsync(MasterLookupValue value)
    {
        if (value.Id == 0) { value.CreatedAt = DateTime.UtcNow; _db.MasterLookupValues.Add(value); }
        else _db.MasterLookupValues.Update(value);
        await _db.SaveChangesAsync();
        return value;
    }

    public async Task<bool> DeleteValueAsync(int valueId)
    {
        var val = await _db.MasterLookupValues.FindAsync(valueId);
        if (val is null) return false;
        val.IsActive = false;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<MasterLookupValue?> GetStatusByNameAsync(string statusName) =>
        await _db.MasterLookupValues.Include(v => v.MasterLookup)
            .FirstOrDefaultAsync(v => v.MasterLookup.LookupCode == LookupCodes.Status
                && v.ValueText == statusName && v.IsActive);
}
