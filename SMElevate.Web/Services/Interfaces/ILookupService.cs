using SMElevate.Web.Models.Common;

namespace SMElevate.Web.Services.Interfaces;

public interface ILookupService
{
    Task<List<MasterLookup>> GetAllLookupsAsync();
    Task<MasterLookup?> GetByIdAsync(int id);
    Task<MasterLookup?> GetByCodeAsync(string code);
    Task<List<MasterLookupValue>> GetValuesAsync(string lookupCode, bool activeOnly = true);
    Task<MasterLookupValue?> GetValueByIdAsync(int id);
    Task<MasterLookup> CreateLookupAsync(MasterLookup lookup);
    Task<MasterLookup> UpdateLookupAsync(MasterLookup lookup);
    Task<bool> ToggleLookupActiveAsync(int id);
    Task<MasterLookupValue> SaveValueAsync(MasterLookupValue value);
    Task<bool> DeleteValueAsync(int valueId);
    Task<MasterLookupValue?> GetStatusByNameAsync(string statusName);
}
