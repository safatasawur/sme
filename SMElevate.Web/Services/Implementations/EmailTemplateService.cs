using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Data;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Services.Implementations;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly ApplicationDbContext _db;
    public EmailTemplateService(ApplicationDbContext db) => _db = db;

    public async Task<EmailTemplate?> GetByCodeAsync(string templateCode) =>
        await _db.EmailTemplates.FirstOrDefaultAsync(t => t.TemplateCode == templateCode && t.IsActive);

    public async Task<List<EmailTemplate>> GetAllAsync() =>
        await _db.EmailTemplates.OrderBy(t => t.TemplateName).ToListAsync();

    public async Task<EmailTemplate> UpsertAsync(EmailTemplate template)
    {
        var existing = await _db.EmailTemplates.FirstOrDefaultAsync(t => t.TemplateCode == template.TemplateCode);
        if (existing is null) { template.CreatedAt = DateTime.UtcNow; _db.EmailTemplates.Add(template); }
        else { existing.Subject = template.Subject; existing.BodyHtml = template.BodyHtml; existing.IsActive = template.IsActive; existing.UpdatedAt = DateTime.UtcNow; }
        await _db.SaveChangesAsync();
        return template;
    }

    public string ReplacePlaceholders(string template, Dictionary<string, string> values)
    {
        foreach (var (key, value) in values)
            template = template.Replace("{{" + key + "}}", value ?? "");
        return template;
    }
}
