using SMElevate.Web.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace SMElevate.Web.Areas.Admin.ViewModels;

public class AdminDashboardViewModel
{
    // Stat counts
    public int TotalSmesRegistered { get; set; }
    public int TotalApplications { get; set; }
    public int InProcessApplications { get; set; }
    public int CompletedApplications { get; set; }
    public int ApprovedApplications { get; set; }
    public int RejectedApplications { get; set; }
    public int ParticipatingBanks { get; set; }
    public int ActiveSchemes { get; set; }

    // V2.2 extended metrics
    public int OffersIssued { get; set; }
    public int OffersAccepted { get; set; }
    public int Disbursed { get; set; }
    public int Referred { get; set; }

    // Computed rates
    public double ApprovalRate => TotalApplications > 0
        ? Math.Round((double)ApprovedApplications / TotalApplications * 100, 1) : 0;
    public double ConversionRate => TotalApplications > 0
        ? Math.Round((double)Disbursed / TotalApplications * 100, 1) : 0;

    // Application Status donut chart
    public List<string> StatusLabels { get; set; } = new();
    public List<int> StatusCounts { get; set; } = new();

    // SME Registration Trend area chart (last 6 months)
    public List<string> MonthlyLabels { get; set; } = new();
    public List<int> MonthlyCounts { get; set; } = new();

    // Bank-wise bar chart
    public List<string> BankWiseLabels { get; set; } = new();
    public List<int> BankWiseCounts { get; set; } = new();

    // Scheme-wise (by FacilityRequested) horizontal bar chart
    public List<string> SchemeWiseLabels { get; set; } = new();
    public List<int> SchemeWiseCounts { get; set; } = new();

    // Recent applications table
    public List<RecentApplicationDto> RecentApplications { get; set; } = new();
}

public class RecentApplicationDto
{
    public string RequestNo { get; set; } = default!;
    public string? CaseId { get; set; }
    public string BusinessName { get; set; } = default!;
    public string? BankName { get; set; }
    public decimal Amount { get; set; }
    public string? Status { get; set; }
    public DateTime? SubmittedAt { get; set; }
}

public class UserListViewModel
{
    public List<ApplicationUser> Users { get; set; } = new();
    public List<Role> Roles { get; set; } = new();
    public int? FilterRoleId { get; set; }
    public string? FilterUserType { get; set; }
    public string? FilterStatus { get; set; }
    public string? FilterSearch { get; set; }
}

public class UserFormViewModel
{
    public int Id { get; set; }
    [Required] public string FullName { get; set; } = default!;
    [Required, EmailAddress] public string EmailAddress { get; set; } = default!;
    public string? MobileNo { get; set; }
    [Required] public UserType UserType { get; set; }
    public int? RoleId { get; set; }
    public int? BankId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? AuthenticationMode { get; set; }
    public string? NewPassword { get; set; }
    public string? ConfirmPassword { get; set; }
    public List<Role> Roles { get; set; } = new();
    public List<Bank> Banks { get; set; } = new();
}

public class UserDetailViewModel
{
    public ApplicationUser User { get; set; } = default!;
    public EndUserProfile? Profile { get; set; }
}

public class FormBuilderFieldDto
{
    public string SectionName { get; set; } = "";
    public int SectionOrder { get; set; }
    public string FieldLabel { get; set; } = "";
    public string FieldName { get; set; } = "";
    public string FieldType { get; set; } = "text";
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public string? ColClass { get; set; }
    public string? Placeholder { get; set; }
    public string? DefaultValue { get; set; }
    public string? FieldOptions { get; set; }
}

public class SchemeFormFieldConfigSaveDto
{
    public string FieldName { get; set; } = "";
    public bool IsAvailable { get; set; }
    public bool IsRequired { get; set; }
    public bool HasConditionalVisibility { get; set; }
    public string? ConditionalExpression { get; set; }
}

public class FormFieldConfigRowViewModel
{
    public string SectionName { get; set; } = "";
    public int SectionOrder { get; set; }
    public string FieldName { get; set; } = "";
    public string FieldLabel { get; set; } = "";
    public string FieldType { get; set; } = "";
    public string? LookupKey { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsRequired { get; set; } = true;
    public bool HasConditionalVisibility { get; set; }
    public string? ConditionalExpression { get; set; }
}

public class BankFormViewModel
{
    public int Id { get; set; }
    [Required] public string BankName { get; set; } = default!;
    [Required] public string IBANPrefix { get; set; } = default!;
    public string? BankCode { get; set; }
    [Required, EmailAddress] public string BankEmailAddress { get; set; } = default!;
    public bool IsActive { get; set; } = true;
}

public class SchemeFormViewModel
{
    public int Id { get; set; }
    [Required] public string SchemeName { get; set; } = default!;
    public string? SchemeCode { get; set; }
    public string? SchemeDescription { get; set; }
    public string? EligibilityCriteria { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Draft";
    public bool IsPublished { get; set; }
    public int? FormId { get; set; }
}

public class LookupFormViewModel
{
    public int Id { get; set; }
    [Required] public string LookupName { get; set; } = default!;
    [Required] public string LookupCode { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public List<MasterLookupValue> Values { get; set; } = new();
}

public class SettingsViewModel
{
    public Dictionary<string, string?> Email { get; set; } = new();
    public Dictionary<string, string?> Notification { get; set; } = new();
    public Dictionary<string, string?> OAuth { get; set; } = new();
    public Dictionary<string, string?> General { get; set; } = new();
}
