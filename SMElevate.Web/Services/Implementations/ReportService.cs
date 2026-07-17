using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Data;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Services.Implementations;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _db;
    public ReportService(ApplicationDbContext db) => _db = db;

    public async Task<ApplicationStatusReportResult> GetApplicationStatusReportAsync(DateTime? from, DateTime? to, int? bankId)
    {
        var query = _db.LoanRequests
            .Include(r => r.Status)
            .AsQueryable();
        if (from.HasValue) query = query.Where(r => r.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(r => r.CreatedAt <= to.Value.AddDays(1));
        if (bankId.HasValue) query = query.Where(r => r.AssignedBankId == bankId.Value);

        var total = await query.CountAsync();
        var grouped = await query
            .GroupBy(r => r.Status != null ? r.Status.ValueText : "Unknown")
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ToListAsync();

        var rows = grouped.Select(g => new StatusCountRow(g.Status, g.Count,
            total > 0 ? Math.Round((double)g.Count / total * 100, 1) : 0)).ToList();
        return new ApplicationStatusReportResult(rows, total);
    }

    public async Task<TurnaroundTimeReportResult> GetTurnaroundTimeReportAsync(DateTime? from, DateTime? to, int? bankId)
    {
        var query = _db.LoanRequests
            .Include(r => r.AssignedBank)
            .Where(r => r.SubmittedAt != null)
            .AsQueryable();
        if (from.HasValue) query = query.Where(r => r.SubmittedAt >= from.Value);
        if (to.HasValue) query = query.Where(r => r.SubmittedAt <= to.Value.AddDays(1));
        if (bankId.HasValue) query = query.Where(r => r.AssignedBankId == bankId.Value);

        var requests = await query.ToListAsync();
        var disbursements = await _db.Disbursements.ToListAsync();
        var decisions = await _db.BankDecisions.ToListAsync();

        var bankGroups = requests.GroupBy(r => r.AssignedBank?.BankName ?? "Unassigned");
        var rows = new List<TATRow>();
        double overallTotal = 0;
        int overallCount = 0;

        foreach (var g in bankGroups)
        {
            double avgDecision = 0;
            double avgDisburse = 0;
            var gList = g.ToList();

            var withDecision = gList.Where(r => decisions.Any(d => d.LoanRequestId == r.Id)).ToList();
            if (withDecision.Any())
            {
                avgDecision = withDecision.Average(r =>
                {
                    var d = decisions.First(dd => dd.LoanRequestId == r.Id);
                    return (d.DecisionDate - (r.SubmittedAt ?? r.CreatedAt)).TotalDays;
                });
            }

            var withDisburse = gList.Where(r => disbursements.Any(d => d.LoanRequestId == r.Id && d.ValueDate != null)).ToList();
            if (withDisburse.Any())
            {
                avgDisburse = withDisburse.Average(r =>
                {
                    var d = disbursements.First(dd => dd.LoanRequestId == r.Id);
                    return (d.ValueDate!.Value - (r.SubmittedAt ?? r.CreatedAt)).TotalDays;
                });
            }

            rows.Add(new TATRow(g.Key, gList.Count, Math.Round(avgDecision, 1), Math.Round(avgDisburse, 1)));
            overallTotal += avgDecision * gList.Count;
            overallCount += gList.Count;
        }

        return new TurnaroundTimeReportResult(rows, overallCount > 0 ? Math.Round(overallTotal / overallCount, 1) : 0);
    }

    public async Task<AssessmentReferralReportResult> GetAssessmentReferralReportAsync(DateTime? from, DateTime? to, int? bankId)
    {
        var query = _db.LoanRequests.Include(r => r.AssignedBank).AsQueryable();
        if (from.HasValue) query = query.Where(r => r.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(r => r.CreatedAt <= to.Value.AddDays(1));
        if (bankId.HasValue) query = query.Where(r => r.AssignedBankId == bankId.Value);

        var requests = await query.ToListAsync();
        var assessments = await _db.BankAssessments.ToListAsync();
        var decisions = await _db.BankDecisions.ToListAsync();

        var bankGroups = requests.GroupBy(r => r.AssignedBank?.BankName ?? "Unassigned");
        var rows = bankGroups.Select(g =>
        {
            var ids = g.Select(r => r.Id).ToHashSet();
            return new AssessmentRow(
                g.Key,
                Referred: g.Count(r => r.AssignedBankId != null),
                CreditChecked: assessments.Count(a => ids.Contains(a.LoanRequestId) && a.AssessmentType == AssessmentType.CreditBureauCheck && a.Status == AssessmentStatus.Completed),
                RiskAssessed: assessments.Count(a => ids.Contains(a.LoanRequestId) && a.AssessmentType == AssessmentType.RiskAssessment && a.Status == AssessmentStatus.Completed),
                CDDCompleted: assessments.Count(a => ids.Contains(a.LoanRequestId) && a.AssessmentType == AssessmentType.CDDCompliance && a.Status == AssessmentStatus.Completed),
                Decided: decisions.Count(d => ids.Contains(d.LoanRequestId))
            );
        }).ToList();

        return new AssessmentReferralReportResult(rows);
    }

    public async Task<DeclineAnalysisResult> GetDeclineAnalysisAsync(DateTime? from, DateTime? to, int? bankId)
    {
        var query = _db.BankDecisions
            .Include(d => d.DeclineReasonCode)
            .Include(d => d.LoanRequest)
            .Where(d => d.DecisionType == DecisionType.Declined)
            .AsQueryable();
        if (from.HasValue) query = query.Where(d => d.DecisionDate >= from.Value);
        if (to.HasValue) query = query.Where(d => d.DecisionDate <= to.Value.AddDays(1));
        if (bankId.HasValue) query = query.Where(d => d.LoanRequest.AssignedBankId == bankId.Value);

        var decisions = await query.ToListAsync();
        var total = decisions.Count;

        var grouped = decisions.GroupBy(d => d.DeclineReasonCode?.Code ?? "Other")
            .Select(g => new DeclineRow(
                g.Key,
                g.First().DeclineReasonCode?.Description ?? "Other",
                g.First().DeclineReasonCode?.Category ?? "Other",
                g.Count(),
                total > 0 ? Math.Round((double)g.Count() / total * 100, 1) : 0
            )).OrderByDescending(r => r.Count).ToList();

        return new DeclineAnalysisResult(grouped, total);
    }

    public async Task<DisbursementSummaryResult> GetDisbursementSummaryAsync(DateTime? from, DateTime? to, int? bankId)
    {
        var query = _db.Disbursements
            .Include(d => d.LoanRequest).ThenInclude(r => r.AssignedBank)
            .Include(d => d.UpdatedBy)
            .AsQueryable();
        if (from.HasValue) query = query.Where(d => d.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(d => d.CreatedAt <= to.Value.AddDays(1));
        if (bankId.HasValue) query = query.Where(d => d.LoanRequest.AssignedBankId == bankId.Value);

        var disbursements = await query.ToListAsync();
        var total = disbursements.Sum(d => d.DisbursedAmount ?? 0);

        var rows = disbursements.GroupBy(d => d.LoanRequest.AssignedBank?.BankName ?? "Unassigned")
            .Select(g => new DisbursementRow(g.Key, g.Count(), g.Sum(d => d.ApprovedAmount), g.Sum(d => d.DisbursedAmount ?? 0)))
            .OrderByDescending(r => r.DisbursedAmount).ToList();

        return new DisbursementSummaryResult(rows, total, disbursements.Count);
    }

    public async Task<GeographicSpreadResult> GetGeographicSpreadAsync(DateTime? from, DateTime? to)
    {
        // Uses BusinessAddress as proxy for geography until a dedicated city field is added
        var query = _db.LoanRequests.Include(r => r.Status).AsQueryable();
        if (from.HasValue) query = query.Where(r => r.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(r => r.CreatedAt <= to.Value.AddDays(1));

        var requests = await query.ToListAsync();
        // Group by first word of address as a rough city approximation
        var rows = requests.GroupBy(r =>
            {
                var parts = (r.BusinessAddress ?? "Unknown").Split(',', StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 1 ? parts[^1].Trim() : parts[0].Trim();
            })
            .Select(g => new GeoRow(g.Key, g.Count(),
                g.Count(r => r.Status?.ValueText == AppStatus.ConditionallyApproved || r.Status?.ValueText == AppStatus.Disbursed),
                g.Count(r => r.Status?.ValueText == AppStatus.Disbursed)))
            .OrderByDescending(r => r.Applications)
            .Take(50)
            .ToList();

        return new GeographicSpreadResult(rows);
    }

    public async Task<BankPerformanceResult> GetBankPerformanceReportAsync(DateTime? from, DateTime? to)
    {
        var query = _db.LoanRequests
            .Include(r => r.AssignedBank)
            .Include(r => r.Status)
            .Where(r => r.AssignedBankId != null)
            .AsQueryable();
        if (from.HasValue) query = query.Where(r => r.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(r => r.CreatedAt <= to.Value.AddDays(1));

        var requests = await query.ToListAsync();
        var decisions = await _db.BankDecisions.ToListAsync();
        var disbursements = await _db.Disbursements.ToListAsync();

        var rows = requests.GroupBy(r => r.AssignedBank?.BankName ?? "Unknown")
            .Select(g =>
            {
                var ids = g.Select(r => r.Id).ToHashSet();
                var approved = decisions.Count(d => ids.Contains(d.LoanRequestId) && d.DecisionType != DecisionType.Declined);
                var declined = decisions.Count(d => ids.Contains(d.LoanRequestId) && d.DecisionType == DecisionType.Declined);
                var disbursed = disbursements.Count(d => ids.Contains(d.LoanRequestId));
                var total = g.Count();
                var approvalRate = (approved + declined) > 0 ? Math.Round((double)approved / (approved + declined) * 100, 1) : 0;

                var withDecision = g.Where(r => decisions.Any(d => d.LoanRequestId == r.Id)).ToList();
                double avgTAT = 0;
                if (withDecision.Any())
                {
                    avgTAT = Math.Round(withDecision.Average(r =>
                    {
                        var d = decisions.First(dd => dd.LoanRequestId == r.Id);
                        return (d.DecisionDate - (r.SubmittedAt ?? r.CreatedAt)).TotalDays;
                    }), 1);
                }

                return new BankPerfRow(g.Key, total, approved, declined, disbursed, approvalRate, avgTAT);
            })
            .OrderByDescending(r => r.Assigned)
            .ToList();

        return new BankPerformanceResult(rows);
    }
}
