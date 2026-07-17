using Microsoft.AspNetCore.Http;
using SMElevate.Web.Models.Common;

namespace SMElevate.Web.Services.Interfaces;

public interface IBankAssessmentService
{
    Task<List<BankAssessment>> GetByLoanRequestAsync(int loanRequestId);
    Task<BankAssessment?> GetByTypeAsync(int loanRequestId, string assessmentType);
    Task<BankAssessment> UpsertAsync(BankAssessment assessment);
}

public interface IAdditionalInfoRequestService
{
    Task<List<AdditionalInformationRequest>> GetByLoanRequestAsync(int loanRequestId);
    Task<AdditionalInformationRequest?> GetByIdAsync(int id);
    Task<AdditionalInformationRequest> CreateAsync(AdditionalInformationRequest request);
    Task<AdditionalInformationRequest> SubmitResponseAsync(int id, string response, int respondedByUserId, List<IFormFile>? documents);
    Task<AdditionalInformationRequest> CloseAsync(int id);
}

public interface IBankDecisionService
{
    Task<BankDecision?> GetByLoanRequestAsync(int loanRequestId);
    Task<BankDecision> RecordDecisionAsync(BankDecision decision);
    Task<List<DeclineReasonCode>> GetActiveReasonCodesAsync();
}

public interface IConditionalOfferService
{
    Task<List<ConditionalOffer>> GetByLoanRequestAsync(int loanRequestId);
    Task<ConditionalOffer?> GetByIdAsync(int id);
    Task<ConditionalOffer> CreateOfferAsync(ConditionalOffer offer);
    Task<ConditionalOffer> RecordResponseAsync(int offerId, string responseType, int respondedByUserId, string? remarks, string? ipAddress);
    Task ExpireOffersAsync(); // called by background/scheduler
    Task<string> GenerateOfferNumberAsync();
}

public interface IPostApprovalService
{
    Task<PostApprovalChecklist?> GetChecklistByLoanRequestAsync(int loanRequestId);
    Task<PostApprovalChecklist> CreateChecklistAsync(PostApprovalChecklist checklist, List<PostApprovalChecklistItem> items);
    Task<PostApprovalChecklistItem?> GetItemByIdAsync(int itemId);
    Task<PostApprovalChecklistItem> SubmitItemDocumentAsync(int itemId, IFormFile document, int uploadedByUserId);
    Task<PostApprovalChecklistItem> VerifyItemAsync(int itemId, string status, int verifiedByUserId, string? remarks);
}

public interface IDisbursementService
{
    Task<Disbursement?> GetByLoanRequestAsync(int loanRequestId);
    Task<Disbursement> CreateOrUpdateAsync(Disbursement disbursement);
}

public interface IApplicationMonitoringService
{
    Task<ApplicationMonitoring?> GetByLoanRequestAsync(int loanRequestId);
    Task<ApplicationMonitoring> CreateOrUpdateAsync(ApplicationMonitoring monitoring);
}
