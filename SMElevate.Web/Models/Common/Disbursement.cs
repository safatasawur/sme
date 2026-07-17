namespace SMElevate.Web.Models.Common;

public class Disbursement
{
    public int Id { get; set; }
    public int LoanRequestId { get; set; }
    public string DisbursementStatus { get; set; } = DisbursementStatuses.Pending;
    public decimal ApprovedAmount { get; set; }
    public decimal? DisbursedAmount { get; set; }
    public DateTime? ValueDate { get; set; }
    public string? DisbursementAccount { get; set; } // IBAN or RAAST
    public string? BankReferenceNumber { get; set; }
    public string? Remarks { get; set; }
    public int UpdatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public LoanRequest LoanRequest { get; set; } = default!;
    public ApplicationUser UpdatedBy { get; set; } = default!;
}

public class ApplicationMonitoring
{
    public int Id { get; set; }
    public int LoanRequestId { get; set; }
    public string MonitoringStatus { get; set; } = MonitoringStatuses.Active;
    public string? Notes { get; set; }
    public int UpdatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? NextReviewDate { get; set; }

    public LoanRequest LoanRequest { get; set; } = default!;
    public ApplicationUser UpdatedBy { get; set; } = default!;
}

public static class DisbursementStatuses
{
    public const string Pending = "Pending";
    public const string PartiallyDisbursed = "PartiallyDisbursed";
    public const string FullyDisbursed = "FullyDisbursed";
    public const string Cancelled = "Cancelled";
}

public static class MonitoringStatuses
{
    public const string Active = "Active";
    public const string UnderMonitoring = "UnderMonitoring";
    public const string Completed = "Completed";
    public const string Closed = "Closed";
}
