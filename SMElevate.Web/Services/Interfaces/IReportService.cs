namespace SMElevate.Web.Services.Interfaces;

public interface IReportService
{
    Task<ApplicationStatusReportResult> GetApplicationStatusReportAsync(DateTime? from, DateTime? to, int? bankId);
    Task<TurnaroundTimeReportResult> GetTurnaroundTimeReportAsync(DateTime? from, DateTime? to, int? bankId);
    Task<AssessmentReferralReportResult> GetAssessmentReferralReportAsync(DateTime? from, DateTime? to, int? bankId);
    Task<DeclineAnalysisResult> GetDeclineAnalysisAsync(DateTime? from, DateTime? to, int? bankId);
    Task<DisbursementSummaryResult> GetDisbursementSummaryAsync(DateTime? from, DateTime? to, int? bankId);
    Task<GeographicSpreadResult> GetGeographicSpreadAsync(DateTime? from, DateTime? to);
    Task<BankPerformanceResult> GetBankPerformanceReportAsync(DateTime? from, DateTime? to);
}

public record ApplicationStatusReportResult(List<StatusCountRow> Rows, int Total);
public record StatusCountRow(string Status, int Count, double Percentage);
public record TurnaroundTimeReportResult(List<TATRow> Rows, double OverallAvgDays);
public record TATRow(string BankName, int TotalCases, double AvgDaysToDecision, double AvgDaysToDisbursement);
public record AssessmentReferralReportResult(List<AssessmentRow> Rows);
public record AssessmentRow(string BankName, int Referred, int CreditChecked, int RiskAssessed, int CDDCompleted, int Decided);
public record DeclineAnalysisResult(List<DeclineRow> Rows, int TotalDeclined);
public record DeclineRow(string ReasonCode, string Description, string Category, int Count, double Percentage);
public record DisbursementSummaryResult(List<DisbursementRow> Rows, decimal TotalDisbursed, int TotalCases);
public record DisbursementRow(string BankName, int Cases, decimal TotalAmount, decimal DisbursedAmount);
public record GeographicSpreadResult(List<GeoRow> Rows);
public record GeoRow(string City, int Applications, int Approved, int Disbursed);
public record BankPerformanceResult(List<BankPerfRow> Rows);
public record BankPerfRow(string BankName, int Assigned, int Approved, int Declined, int Disbursed, double ApprovalRate, double AvgTATDays);
