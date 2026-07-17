using SMElevate.Web.Areas.Admin.ViewModels;
using SMElevate.Web.Models.Common;

namespace SMElevate.Web.Services.Interfaces;

public class SchemeFieldForEndUserDto
{
    public string FieldName { get; set; } = "";
    public string FieldLabel { get; set; } = "";
    public string FieldType { get; set; } = "";
    public string SectionName { get; set; } = "";
    public int SectionOrder { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsRequired { get; set; }
    public bool HasConditionalVisibility { get; set; }
    public string? ConditionalExpression { get; set; }
    public List<string> Options { get; set; } = new();
}

public interface ISchemeFormConfigService
{
    Task<List<SchemeFormFieldConfiguration>> GetConfigsAsync(int schemeId);
    Task SaveConfigsAsync(int schemeId, List<SchemeFormFieldConfigSaveDto> dtos);
    Task<List<SchemeFieldForEndUserDto>> GetAvailableFieldsAsync(int schemeId);
    Task<Dictionary<int, int>> GetConfigCountsAsync(IEnumerable<int> schemeIds);
}
