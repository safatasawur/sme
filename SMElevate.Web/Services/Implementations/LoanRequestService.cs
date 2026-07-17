using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Data;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Services.Implementations;

public class LoanRequestService : ILoanRequestService
{
    private readonly ApplicationDbContext _db;
    private readonly IRequestNumberService _reqNo;
    private readonly ILookupService _lookup;

    public LoanRequestService(ApplicationDbContext db, IRequestNumberService reqNo, ILookupService lookup)
    {
        _db = db;
        _reqNo = reqNo;
        _lookup = lookup;
    }

    private IQueryable<LoanRequest> BaseQuery() =>
        _db.LoanRequests
            .Include(r => r.Status)
            .Include(r => r.AssignedBank)
            .Include(r => r.User)
            .Include(r => r.Shareholders);

    public async Task<List<LoanRequest>> GetAllRequestsAsync() =>
        await BaseQuery().OrderByDescending(r => r.SubmittedAt).ToListAsync();

    public async Task<List<LoanRequest>> GetRequestsByUserAsync(int userId) =>
        await BaseQuery().Where(r => r.UserId == userId).OrderByDescending(r => r.SubmittedAt).ToListAsync();

    public async Task<List<LoanRequest>> GetRequestsByBankAsync(int bankId, string? statusFilter = null)
    {
        var query = BaseQuery().Where(r => r.AssignedBankId == bankId);
        if (!string.IsNullOrEmpty(statusFilter))
            query = query.Where(r => r.Status != null && r.Status.ValueText == statusFilter);
        return await query.OrderByDescending(r => r.SubmittedAt).ToListAsync();
    }

    public async Task<LoanRequest?> GetByIdAsync(int id) =>
        await BaseQuery()
            .Include(r => r.StatusHistory).ThenInclude(h => h.NewStatus)
            .Include(r => r.StatusHistory).ThenInclude(h => h.ChangedBy)
            .Include(r => r.Attachments).ThenInclude(a => a.UploadedBy)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<LoanRequest?> GetByRequestNoAsync(string requestNo) =>
        await BaseQuery()
            .Include(r => r.StatusHistory).ThenInclude(h => h.NewStatus)
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.RequestNo == requestNo);

    public async Task<LoanRequest> CreateAsync(LoanRequest request, List<LoanRequestShareholder> shareholders)
    {
        request.RequestNo = await _reqNo.GenerateAsync();
        request.CreatedAt = DateTime.UtcNow;

        // Status is set by the WorkflowService after creation.
        // For backward compatibility when called without workflow (legacy path), default to Draft.
        if (request.StatusId == null)
        {
            var draftStatus = await _lookup.GetStatusByNameAsync(AppStatus.Draft);
            if (draftStatus is not null) request.StatusId = draftStatus.Id;
        }

        _db.LoanRequests.Add(request);
        await _db.SaveChangesAsync();

        foreach (var sh in shareholders)
        {
            sh.LoanRequestId = request.Id;
            _db.LoanRequestShareholders.Add(sh);
        }

        await _db.SaveChangesAsync();
        return request;
    }

    public async Task<LoanRequest> UpdateStatusAsync(int requestId, int newStatusId, int changedByUserId, string? remarks, string? attachmentPath)
    {
        var request = await _db.LoanRequests.FindAsync(requestId)
            ?? throw new KeyNotFoundException($"Loan request {requestId} not found.");

        var history = new LoanRequestStatusHistory
        {
            LoanRequestId = requestId,
            OldStatusId = request.StatusId,
            NewStatusId = newStatusId,
            ChangedByUserId = changedByUserId,
            Remarks = remarks,
            AttachmentPath = attachmentPath,
            CreatedAt = DateTime.UtcNow
        };

        request.StatusId = newStatusId;
        request.UpdatedAt = DateTime.UtcNow;

        _db.LoanRequestStatusHistories.Add(history);
        await _db.SaveChangesAsync();
        return request;
    }

    public async Task<(int Total, int Assigned, int InProcess, int Approved, int Rejected, int Completed)> GetDashboardStatsAsync(int? bankId = null, int? userId = null)
    {
        var query = _db.LoanRequests.Include(r => r.Status).AsQueryable();
        if (bankId.HasValue) query = query.Where(r => r.AssignedBankId == bankId.Value);
        if (userId.HasValue) query = query.Where(r => r.UserId == userId.Value);

        var all = await query.Select(r => r.Status == null ? "" : r.Status.ValueText).ToListAsync();
        return (
            Total: all.Count,
            Assigned: all.Count(s => s == AppStatus.Assigned || s == AppStatus.Submitted),
            InProcess: all.Count(s => s == AppStatus.InProcess || s == AppStatus.MoreInformationRequired),
            Approved: all.Count(s => s == AppStatus.Approved),
            Rejected: all.Count(s => s == AppStatus.Rejected),
            Completed: all.Count(s => s == AppStatus.Completed)
        );
    }

    public async Task<List<LoanRequestStatusHistory>> GetStatusHistoryAsync(int requestId) =>
        await _db.LoanRequestStatusHistories
            .Include(h => h.NewStatus)
            .Include(h => h.OldStatus)
            .Include(h => h.ChangedBy)
            .Where(h => h.LoanRequestId == requestId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();

    public async Task SaveAttachmentAsync(LoanRequestAttachment attachment)
    {
        _db.LoanRequestAttachments.Add(attachment);
        await _db.SaveChangesAsync();
    }

    public async Task SaveFieldValuesAsync(int requestId, List<(string FieldName, string? FieldValue)> values)
    {
        foreach (var (fieldName, fieldValue) in values)
        {
            _db.LoanRequestFieldValues.Add(new LoanRequestFieldValue
            {
                LoanRequestId = requestId,
                FieldName = fieldName,
                FieldValue = fieldValue,
                CreatedAt = DateTime.UtcNow
            });
        }
        await _db.SaveChangesAsync();
    }
}
