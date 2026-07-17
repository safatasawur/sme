namespace SMElevate.Web.Models.Common;

public class Scheme
{
    public int Id { get; set; }
    public string SchemeName { get; set; } = default!;
    public string? SchemeCode { get; set; }
    public string? SchemeDescription { get; set; }
    public string? EligibilityCriteria { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Draft";
    public bool IsPublished { get; set; } = false;
    public int? FormId { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    public ApplicationUser? CreatedBy { get; set; }
    public SchemeForm? Form { get; set; }
}

public class SchemeForm
{
    public int Id { get; set; }
    public int SchemeId { get; set; }
    public string FormName { get; set; } = default!;
    public string? FormJson { get; set; }
    public bool IsPublished { get; set; } = false;
    public int VersionNumber { get; set; } = 1;
    public string FormStatus { get; set; } = "Draft"; // Draft | Published | Archived
    public DateTime? EffectiveDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    public Scheme Scheme { get; set; } = default!;
    public ICollection<SchemeFormField> Fields { get; set; } = new List<SchemeFormField>();
}

public class SchemeFormField
{
    public int Id { get; set; }
    public int SchemeFormId { get; set; }
    public string SectionName { get; set; } = "Default";
    public int SectionOrder { get; set; } = 0;
    public string FieldLabel { get; set; } = default!;
    public string FieldName { get; set; } = default!;
    public string FieldType { get; set; } = default!;
    public int? LookupId { get; set; }
    public bool IsRequired { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    public string? ColClass { get; set; }
    public string? ValidationRule { get; set; }
    public string? Placeholder { get; set; }
    public string? DefaultValue { get; set; }
    public string? FieldOptions { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public SchemeForm SchemeForm { get; set; } = default!;
    public MasterLookup? Lookup { get; set; }
}

public class SchemeFormFieldConfiguration
{
    public int Id { get; set; }
    public int SchemeId { get; set; }
    public string SectionName { get; set; } = default!;
    public int SectionOrder { get; set; }
    public string FieldLabel { get; set; } = default!;
    public string FieldName { get; set; } = default!;
    public string FieldType { get; set; } = default!;
    public string? LookupKey { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsRequired { get; set; } = true;
    public bool HasConditionalVisibility { get; set; } = false;
    public string? ConditionalExpression { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Scheme Scheme { get; set; } = default!;
}
