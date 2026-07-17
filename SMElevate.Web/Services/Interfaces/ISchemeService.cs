using SMElevate.Web.Models.Common;

namespace SMElevate.Web.Services.Interfaces;

public interface ISchemeService
{
    Task<List<Scheme>> GetAllSchemesAsync();
    Task<Scheme?> GetByIdAsync(int id);
    Task<Scheme> CreateSchemeAsync(Scheme scheme);
    Task<Scheme> UpdateSchemeAsync(Scheme scheme);
    Task<bool> PublishAsync(int schemeId);
    Task<bool> UnpublishAsync(int schemeId);
    Task<List<Scheme>> GetPublishedSchemesAsync();
    Task<SchemeForm?> GetFormBySchemeIdAsync(int schemeId);
    Task<SchemeForm?> GetPublishedFormAsync(int schemeId);
    Task<SchemeForm> SaveFormAsync(SchemeForm form, List<SchemeFormField>? fields = null, string? conditionsJson = null, string? sectionsJson = null);
    Task<bool> PublishFormAsync(int formId);
}
