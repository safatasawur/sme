using SMElevate.Web.Models.Common;

namespace SMElevate.Web.Services.Interfaces;

public interface IEmailTemplateService
{
    Task<EmailTemplate?> GetByCodeAsync(string templateCode);
    Task<List<EmailTemplate>> GetAllAsync();
    Task<EmailTemplate> UpsertAsync(EmailTemplate template);
    string ReplacePlaceholders(string template, Dictionary<string, string> values);
}
