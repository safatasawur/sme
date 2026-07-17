using SMElevate.Web.Models.Common;

namespace SMElevate.Web.Services.Interfaces;

public interface IWorkflowService
{
    Task<bool> IsValidTransitionAsync(string fromStatusCode, string toStatusCode, string actorType);
    Task<List<WorkflowTransition>> GetAllowedTransitionsAsync(string currentStatusCode, string actorType);
    Task<LoanRequest> AdvanceStatusAsync(int loanRequestId, string toStatusCode, int changedByUserId,
        string actorType, string? remarks = null, string? reasonCode = null, string? ipAddress = null);
    Task<List<WorkflowStatus>> GetAllStatusesAsync();
    Task<WorkflowStatus?> GetStatusByCodeAsync(string statusCode);
    Task<MasterLookupValue?> GetLookupStatusByCodeAsync(string statusCode);
    Task<bool> IsActorAllowedAsync(string statusCode, string actorType);
}
