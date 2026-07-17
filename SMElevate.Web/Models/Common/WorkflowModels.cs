namespace SMElevate.Web.Models.Common;

public class WorkflowStatus
{
    public int Id { get; set; }
    public string StatusCode { get; set; } = default!;
    public string StatusName { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsInitial { get; set; }
    public bool IsFinal { get; set; }
    public string? AllowedActorTypes { get; set; } // comma-separated: EndUser,Bank,Admin,System
    public string? ColorClass { get; set; } // Bootstrap badge color class
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class WorkflowTransition
{
    public int Id { get; set; }
    public string FromStatusCode { get; set; } = default!;
    public string ToStatusCode { get; set; } = default!;
    public string AllowedActorTypes { get; set; } = default!; // comma-separated
    public string? ActionLabel { get; set; } // UI button label
    public bool RequiresRemarks { get; set; }
    public bool RequiresReasonCode { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}

public class DeclineReasonCode
{
    public int Id { get; set; }
    public string Code { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? Category { get; set; } // Credit, Compliance, Documentation, Other
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}
