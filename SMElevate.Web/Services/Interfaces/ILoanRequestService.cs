using SMElevate.Web.Models.Common;

namespace SMElevate.Web.Services.Interfaces;

public interface ILoanRequestService
{
    Task<List<LoanRequest>> GetAllRequestsAsync();
    Task<List<LoanRequest>> GetRequestsByUserAsync(int userId);
    Task<List<LoanRequest>> GetRequestsByBankAsync(int bankId, string? statusFilter = null);
    Task<LoanRequest?> GetByIdAsync(int id);
    Task<LoanRequest?> GetByRequestNoAsync(string requestNo);
    Task<LoanRequest> CreateAsync(LoanRequest request, List<LoanRequestShareholder> shareholders);
    Task<LoanRequest> UpdateStatusAsync(int requestId, int newStatusId, int changedByUserId, string? remarks, string? attachmentPath);
    Task<(int Total, int Assigned, int InProcess, int Approved, int Rejected, int Completed)> GetDashboardStatsAsync(int? bankId = null, int? userId = null);
    Task<List<LoanRequestStatusHistory>> GetStatusHistoryAsync(int requestId);
    Task SaveAttachmentAsync(LoanRequestAttachment attachment);
    Task SaveFieldValuesAsync(int requestId, List<(string FieldName, string? FieldValue)> values);
}
