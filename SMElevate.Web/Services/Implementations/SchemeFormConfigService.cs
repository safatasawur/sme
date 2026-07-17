using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Areas.Admin.ViewModels;
using SMElevate.Web.Data;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Services.Implementations;

public class SchemeFormConfigService : ISchemeFormConfigService
{
    private readonly ApplicationDbContext _db;
    public SchemeFormConfigService(ApplicationDbContext db) => _db = db;

    public async Task<List<SchemeFormFieldConfiguration>> GetConfigsAsync(int schemeId) =>
        await _db.SchemeFormFieldConfigurations
            .Where(c => c.SchemeId == schemeId)
            .OrderBy(c => c.SectionOrder).ThenBy(c => c.DisplayOrder)
            .ToListAsync();

    public async Task SaveConfigsAsync(int schemeId, List<SchemeFormFieldConfigSaveDto> dtos)
    {
        var existing = await _db.SchemeFormFieldConfigurations
            .Where(c => c.SchemeId == schemeId)
            .ToListAsync();

        foreach (var dto in dtos)
        {
            var masterField = MasterFormTemplate.Fields.FirstOrDefault(f => f.FieldName == dto.FieldName);
            if (masterField is null) continue;

            var config = existing.FirstOrDefault(c => c.FieldName == dto.FieldName);
            if (config is null)
            {
                config = new SchemeFormFieldConfiguration
                {
                    SchemeId     = schemeId,
                    SectionName  = masterField.SectionName,
                    SectionOrder = masterField.SectionOrder,
                    FieldLabel   = masterField.FieldLabel,
                    FieldName    = masterField.FieldName,
                    FieldType    = masterField.FieldType,
                    LookupKey    = masterField.LookupKey,
                    DisplayOrder = masterField.DisplayOrder,
                    CreatedAt    = DateTime.UtcNow
                };
                _db.SchemeFormFieldConfigurations.Add(config);
            }

            config.IsAvailable             = dto.IsAvailable;
            config.IsRequired              = dto.IsRequired;
            config.HasConditionalVisibility = dto.HasConditionalVisibility;
            config.ConditionalExpression   = string.IsNullOrWhiteSpace(dto.ConditionalExpression) ? null : dto.ConditionalExpression.Trim();
            config.UpdatedAt               = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<Dictionary<int, int>> GetConfigCountsAsync(IEnumerable<int> schemeIds)
    {
        var ids = schemeIds.ToList();
        var counts = await _db.SchemeFormFieldConfigurations
            .Where(c => ids.Contains(c.SchemeId))
            .GroupBy(c => c.SchemeId)
            .Select(g => new { SchemeId = g.Key, Count = g.Count() })
            .ToListAsync();
        return counts.ToDictionary(x => x.SchemeId, x => x.Count);
    }

    public async Task<List<SchemeFieldForEndUserDto>> GetAvailableFieldsAsync(int schemeId)
    {
        // Load saved configs; fall back to master defaults if none exist
        var configs = await _db.SchemeFormFieldConfigurations
            .Where(c => c.SchemeId == schemeId && c.IsAvailable)
            .OrderBy(c => c.SectionOrder).ThenBy(c => c.DisplayOrder)
            .ToListAsync();

        // If no configs saved yet, use master defaults (all available)
        IReadOnlyList<(string SectionName, int SectionOrder, string FieldName, string FieldLabel,
            string FieldType, int DisplayOrder, string? LookupKey, bool IsRequired,
            bool HasCond, string? CondExpr)> source;

        if (configs.Count == 0)
        {
            source = MasterFormTemplate.Fields
                .Select(f => (f.SectionName, f.SectionOrder, f.FieldName, f.FieldLabel,
                              f.FieldType, f.DisplayOrder, f.LookupKey, f.DefaultRequired,
                              false, (string?)null))
                .ToList();
        }
        else
        {
            source = configs
                .Select(c => (c.SectionName, c.SectionOrder, c.FieldName, c.FieldLabel,
                              c.FieldType, c.DisplayOrder, c.LookupKey, c.IsRequired,
                              c.HasConditionalVisibility, c.ConditionalExpression))
                .ToList();
        }

        // Resolve lookup values in bulk
        var lookupKeys = source.Where(f => f.LookupKey != null).Select(f => f.LookupKey!).Distinct().ToList();
        var lookupDict = new Dictionary<string, List<string>>();
        if (lookupKeys.Count > 0)
        {
            var vals = await _db.MasterLookupValues
                .Include(v => v.MasterLookup)
                .Where(v => lookupKeys.Contains(v.MasterLookup.LookupCode) && v.IsActive)
                .OrderBy(v => v.MasterLookup.LookupCode).ThenBy(v => v.DisplayOrder)
                .ToListAsync();

            lookupDict = vals
                .GroupBy(v => v.MasterLookup.LookupCode)
                .ToDictionary(g => g.Key, g => g.Select(v => v.ValueText).ToList());
        }

        return source.Select(f => new SchemeFieldForEndUserDto
        {
            FieldName               = f.FieldName,
            FieldLabel              = f.FieldLabel,
            FieldType               = f.FieldType,
            SectionName             = f.SectionName,
            SectionOrder            = f.SectionOrder,
            DisplayOrder            = f.DisplayOrder,
            IsRequired              = f.IsRequired,
            HasConditionalVisibility = f.HasCond,
            ConditionalExpression   = f.CondExpr,
            Options                 = f.LookupKey != null && lookupDict.TryGetValue(f.LookupKey, out var opts) ? opts : new()
        }).ToList();
    }
}
