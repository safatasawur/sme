namespace SMElevate.Web.Models.Common;

public class MasterLookup
{
    public int Id { get; set; }
    public string LookupName { get; set; } = default!;
    public string LookupCode { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<MasterLookupValue> Values { get; set; } = new List<MasterLookupValue>();
    public ICollection<SchemeFormField> FormFields { get; set; } = new List<SchemeFormField>();
}

public class MasterLookupValue
{
    public int Id { get; set; }
    public int MasterLookupId { get; set; }
    public string ValueText { get; set; } = default!;
    public string? ValueCode { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public MasterLookup MasterLookup { get; set; } = default!;
}
