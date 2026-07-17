using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Data;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Services.Implementations;

public class SchemeService : ISchemeService
{
    private readonly ApplicationDbContext _db;
    public SchemeService(ApplicationDbContext db) => _db = db;

    public async Task<List<Scheme>> GetAllSchemesAsync() =>
        await _db.Schemes.Include(s => s.Form).Include(s => s.CreatedBy)
            .OrderByDescending(s => s.CreatedAt).ToListAsync();

    public async Task<Scheme?> GetByIdAsync(int id) =>
        await _db.Schemes.Include(s => s.Form).ThenInclude(f => f!.Fields)
            .Include(s => s.CreatedBy).FirstOrDefaultAsync(s => s.Id == id);

    public async Task<Scheme> CreateSchemeAsync(Scheme scheme)
    {
        scheme.CreatedAt = DateTime.UtcNow;
        _db.Schemes.Add(scheme);
        await _db.SaveChangesAsync();
        return scheme;
    }

    public async Task<Scheme> UpdateSchemeAsync(Scheme scheme)
    {
        scheme.UpdatedAt = DateTime.UtcNow;
        _db.Schemes.Update(scheme);
        await _db.SaveChangesAsync();
        return scheme;
    }

    public async Task<bool> PublishAsync(int schemeId)
    {
        var scheme = await _db.Schemes.FindAsync(schemeId);
        if (scheme is null) return false;
        scheme.IsPublished = true;
        scheme.Status = "Published";
        scheme.PublishedAt = DateTime.UtcNow;
        scheme.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnpublishAsync(int schemeId)
    {
        var scheme = await _db.Schemes.FindAsync(schemeId);
        if (scheme is null) return false;
        scheme.IsPublished = false;
        scheme.Status = "Inactive";
        scheme.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<Scheme>> GetPublishedSchemesAsync() =>
        await _db.Schemes.Where(s => s.IsPublished)
            .OrderBy(s => s.SchemeName).ToListAsync();

    public async Task<SchemeForm?> GetFormBySchemeIdAsync(int schemeId) =>
        await _db.SchemeForms.Include(f => f.Fields.OrderBy(ff => ff.SectionOrder).ThenBy(ff => ff.DisplayOrder))
            .FirstOrDefaultAsync(f => f.SchemeId == schemeId);

    public async Task<SchemeForm?> GetPublishedFormAsync(int schemeId) =>
        await _db.SchemeForms.Include(f => f.Fields.OrderBy(ff => ff.SectionOrder).ThenBy(ff => ff.DisplayOrder))
            .FirstOrDefaultAsync(f => f.SchemeId == schemeId && f.IsPublished);

    public async Task<SchemeForm> SaveFormAsync(SchemeForm form, List<SchemeFormField>? fields = null, string? conditionsJson = null, string? sectionsJson = null)
    {
        var existing = await _db.SchemeForms.Include(f => f.Fields).FirstOrDefaultAsync(f => f.SchemeId == form.SchemeId);
        if (existing is null)
        {
            form.CreatedAt = DateTime.UtcNow;
            if (conditionsJson is not null || sectionsJson is not null)
                form.FormJson = BuildFormJson(conditionsJson, sectionsJson, form.FormJson);
            _db.SchemeForms.Add(form);
            await _db.SaveChangesAsync();

            var scheme = await _db.Schemes.FindAsync(form.SchemeId);
            if (scheme is not null) { scheme.FormId = form.Id; await _db.SaveChangesAsync(); }
        }
        else
        {
            existing.FormName = form.FormName;
            existing.IsPublished = form.IsPublished;
            existing.UpdatedAt = DateTime.UtcNow;
            if (conditionsJson is not null || sectionsJson is not null)
                existing.FormJson = BuildFormJson(conditionsJson, sectionsJson, existing.FormJson);
            form.Id = existing.Id;

            if (fields is not null)
            {
                _db.SchemeFormFields.RemoveRange(existing.Fields);
                await _db.SaveChangesAsync();
            }
        }

        if (fields is not null)
        {
            var formId = form.Id;
            foreach (var field in fields) { field.SchemeFormId = formId; field.CreatedAt = DateTime.UtcNow; _db.SchemeFormFields.Add(field); }
        }

        await _db.SaveChangesAsync();
        return form;
    }

    private static string BuildFormJson(string? conditionsJson, string? sectionsJson, string? existingJson)
    {
        // Preserve existing values for anything not being updated
        string? existingConds = null, existingSecs = null;
        if (!string.IsNullOrEmpty(existingJson))
        {
            try
            {
                var doc = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(existingJson);
                if (doc.TryGetProperty("conditions", out var c)) existingConds = c.GetRawText();
                if (doc.TryGetProperty("sections", out var s))  existingSecs  = s.GetRawText();
            }
            catch { }
        }
        var conds = conditionsJson ?? existingConds ?? "[]";
        var secs  = sectionsJson  ?? existingSecs  ?? "[]";
        return "{\"conditions\":" + conds + ",\"sections\":" + secs + "}";
    }

    public async Task<bool> PublishFormAsync(int formId)
    {
        var form = await _db.SchemeForms.FindAsync(formId);
        if (form is null) return false;
        form.IsPublished = true;
        form.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }
}
