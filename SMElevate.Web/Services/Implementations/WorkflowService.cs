using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Data;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Services.Implementations;

public class WorkflowService : IWorkflowService
{
    private readonly ApplicationDbContext _db;

    public WorkflowService(ApplicationDbContext db) => _db = db;

    public async Task<bool> IsValidTransitionAsync(string fromStatusCode, string toStatusCode, string actorType)
    {
        var transition = await _db.WorkflowTransitions
            .FirstOrDefaultAsync(t => t.FromStatusCode == fromStatusCode
                                   && t.ToStatusCode == toStatusCode
                                   && t.IsActive);
        if (transition == null) return false;
        return transition.AllowedActorTypes.Split(',').Select(a => a.Trim())
            .Contains(actorType, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<List<WorkflowTransition>> GetAllowedTransitionsAsync(string currentStatusCode, string actorType)
    {
        var all = await _db.WorkflowTransitions
            .Where(t => t.FromStatusCode == currentStatusCode && t.IsActive)
            .OrderBy(t => t.DisplayOrder)
            .ToListAsync();

        return all.Where(t => t.AllowedActorTypes.Split(',').Select(a => a.Trim())
            .Contains(actorType, StringComparer.OrdinalIgnoreCase)).ToList();
    }

    public async Task<LoanRequest> AdvanceStatusAsync(int loanRequestId, string toStatusCode, int changedByUserId,
        string actorType, string? remarks = null, string? reasonCode = null, string? ipAddress = null)
    {
        var request = await _db.LoanRequests
            .Include(r => r.Status)
            .FirstOrDefaultAsync(r => r.Id == loanRequestId)
            ?? throw new InvalidOperationException($"Loan request {loanRequestId} not found.");

        var currentStatusCode = request.Status?.ValueCode?.Replace("_", " ") ?? string.Empty;
        var currentStatusText = request.Status?.ValueText ?? string.Empty;

        if (!string.IsNullOrEmpty(currentStatusText) && !await IsValidTransitionAsync(currentStatusText, toStatusCode, actorType))
            throw new InvalidOperationException($"Transition from '{currentStatusText}' to '{toStatusCode}' is not allowed for actor '{actorType}'.");

        var newStatus = await _db.MasterLookupValues
            .FirstOrDefaultAsync(v => v.ValueText == toStatusCode && v.MasterLookup.LookupCode == LookupCodes.Status)
            ?? throw new InvalidOperationException($"Status '{toStatusCode}' not found in lookup.");

        var history = new LoanRequestStatusHistory
        {
            LoanRequestId = loanRequestId,
            OldStatusId = request.StatusId,
            NewStatusId = newStatus.Id,
            ChangedByUserId = changedByUserId,
            ActorType = actorType,
            Remarks = remarks,
            ReasonCode = reasonCode,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };
        _db.LoanRequestStatusHistories.Add(history);

        request.StatusId = newStatus.Id;
        request.UpdatedAt = DateTime.UtcNow;

        // Generate CaseId on first submission
        if (toStatusCode == AppStatus.Submitted && string.IsNullOrEmpty(request.CaseId))
        {
            request.CaseId = await GenerateCaseIdAsync();
            request.SubmittedAt = DateTime.UtcNow;
            request.IsDraft = false;
        }

        await _db.SaveChangesAsync();
        return request;
    }

    public async Task<List<WorkflowStatus>> GetAllStatusesAsync()
        => await _db.WorkflowStatuses.Where(w => w.IsActive).OrderBy(w => w.DisplayOrder).ToListAsync();

    public async Task<WorkflowStatus?> GetStatusByCodeAsync(string statusCode)
        => await _db.WorkflowStatuses.FirstOrDefaultAsync(w => w.StatusCode == statusCode);

    public async Task<MasterLookupValue?> GetLookupStatusByCodeAsync(string statusCode)
        => await _db.MasterLookupValues
            .Include(v => v.MasterLookup)
            .FirstOrDefaultAsync(v => v.ValueText == statusCode && v.MasterLookup.LookupCode == LookupCodes.Status);

    public async Task<bool> IsActorAllowedAsync(string statusCode, string actorType)
    {
        var ws = await _db.WorkflowStatuses.FirstOrDefaultAsync(w => w.StatusCode == statusCode);
        if (ws == null || string.IsNullOrEmpty(ws.AllowedActorTypes)) return false;
        return ws.AllowedActorTypes.Split(',').Select(a => a.Trim())
            .Contains(actorType, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<string> GenerateCaseIdAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"SME-{year}-";
        var lastCase = await _db.LoanRequests
            .Where(r => r.CaseId != null && r.CaseId.StartsWith(prefix))
            .OrderByDescending(r => r.CaseId)
            .Select(r => r.CaseId)
            .FirstOrDefaultAsync();

        int nextSeq = 1;
        if (lastCase != null)
        {
            var seqPart = lastCase.Substring(prefix.Length);
            if (int.TryParse(seqPart, out int lastSeq)) nextSeq = lastSeq + 1;
        }
        return $"{prefix}{nextSeq:D6}";
    }
}
