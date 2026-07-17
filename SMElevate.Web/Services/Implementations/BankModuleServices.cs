using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Data;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Services.Implementations;

public class BankAssessmentService : IBankAssessmentService
{
    private readonly ApplicationDbContext _db;
    public BankAssessmentService(ApplicationDbContext db) => _db = db;

    public async Task<List<BankAssessment>> GetByLoanRequestAsync(int loanRequestId)
        => await _db.BankAssessments
            .Include(a => a.UpdatedBy)
            .Where(a => a.LoanRequestId == loanRequestId)
            .OrderBy(a => a.AssessmentType)
            .ToListAsync();

    public async Task<BankAssessment?> GetByTypeAsync(int loanRequestId, string assessmentType)
        => await _db.BankAssessments
            .Include(a => a.UpdatedBy)
            .FirstOrDefaultAsync(a => a.LoanRequestId == loanRequestId && a.AssessmentType == assessmentType);

    public async Task<BankAssessment> UpsertAsync(BankAssessment assessment)
    {
        var existing = await _db.BankAssessments
            .FirstOrDefaultAsync(a => a.LoanRequestId == assessment.LoanRequestId && a.AssessmentType == assessment.AssessmentType);

        if (existing == null)
        {
            assessment.CreatedAt = DateTime.UtcNow;
            _db.BankAssessments.Add(assessment);
        }
        else
        {
            existing.Status = assessment.Status;
            existing.CheckDate = assessment.CheckDate;
            existing.ReferenceNumber = assessment.ReferenceNumber;
            existing.ResultSummary = assessment.ResultSummary;
            existing.ScorecardReference = assessment.ScorecardReference;
            existing.Result = assessment.Result;
            existing.RiskCategory = assessment.RiskCategory;
            existing.AssessmentDate = assessment.AssessmentDate;
            existing.CDDStatus = assessment.CDDStatus;
            existing.KYCStatus = assessment.KYCStatus;
            existing.AMLStatus = assessment.AMLStatus;
            existing.SanctionsScreeningStatus = assessment.SanctionsScreeningStatus;
            existing.PEPScreeningStatus = assessment.PEPScreeningStatus;
            existing.ComplianceResult = assessment.ComplianceResult;
            existing.CompletionDate = assessment.CompletionDate;
            existing.Remarks = assessment.Remarks;
            existing.AttachmentPath = assessment.AttachmentPath;
            existing.UpdatedByUserId = assessment.UpdatedByUserId;
            existing.UpdatedAt = DateTime.UtcNow;
            assessment = existing;
        }
        await _db.SaveChangesAsync();
        return assessment;
    }
}

public class AdditionalInfoRequestService : IAdditionalInfoRequestService
{
    private readonly ApplicationDbContext _db;
    private readonly IFileUploadService _fileUpload;

    public AdditionalInfoRequestService(ApplicationDbContext db, IFileUploadService fileUpload)
    {
        _db = db;
        _fileUpload = fileUpload;
    }

    public async Task<List<AdditionalInformationRequest>> GetByLoanRequestAsync(int loanRequestId)
        => await _db.AdditionalInfoRequests
            .Include(r => r.CreatedBy)
            .Include(r => r.RespondedBy)
            .Include(r => r.ResponseDocuments)
            .Where(r => r.LoanRequestId == loanRequestId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<AdditionalInformationRequest?> GetByIdAsync(int id)
        => await _db.AdditionalInfoRequests
            .Include(r => r.LoanRequest)
            .Include(r => r.CreatedBy)
            .Include(r => r.RespondedBy)
            .Include(r => r.ResponseDocuments)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<AdditionalInformationRequest> CreateAsync(AdditionalInformationRequest request)
    {
        request.CreatedAt = DateTime.UtcNow;
        request.Status = AdditionalInfoStatus.Pending;
        _db.AdditionalInfoRequests.Add(request);
        await _db.SaveChangesAsync();
        return request;
    }

    public async Task<AdditionalInformationRequest> SubmitResponseAsync(int id, string response, int respondedByUserId, List<IFormFile>? documents)
    {
        var request = await _db.AdditionalInfoRequests.FindAsync(id)
            ?? throw new InvalidOperationException("Information request not found.");

        request.ApplicantResponse = response;
        request.ResponseDate = DateTime.UtcNow;
        request.RespondedByUserId = respondedByUserId;
        request.Status = AdditionalInfoStatus.Responded;

        if (documents != null)
        {
            foreach (var file in documents.Where(f => f.Length > 0))
            {
                var (fileName, origName, filePath, contentType, size) = await _fileUpload.UploadAsync(file, "info-responses");
                _db.ApplicationDocuments.Add(new ApplicationDocument
                {
                    LoanRequestId = request.LoanRequestId,
                    AdditionalInfoRequestId = id,
                    DocumentType = DocumentType.InfoResponse,
                    FileName = fileName, OriginalFileName = origName, FilePath = filePath,
                    ContentType = contentType, FileSize = size, UploadedByUserId = respondedByUserId,
                    UploadedAt = DateTime.UtcNow
                });
            }
        }
        await _db.SaveChangesAsync();
        return request;
    }

    public async Task<AdditionalInformationRequest> CloseAsync(int id)
    {
        var request = await _db.AdditionalInfoRequests.FindAsync(id)
            ?? throw new InvalidOperationException("Information request not found.");
        request.Status = AdditionalInfoStatus.Closed;
        await _db.SaveChangesAsync();
        return request;
    }
}

public class BankDecisionService : IBankDecisionService
{
    private readonly ApplicationDbContext _db;
    public BankDecisionService(ApplicationDbContext db) => _db = db;

    public async Task<BankDecision?> GetByLoanRequestAsync(int loanRequestId)
        => await _db.BankDecisions
            .Include(d => d.MadeBy)
            .Include(d => d.DeclineReasonCode)
            .FirstOrDefaultAsync(d => d.LoanRequestId == loanRequestId);

    public async Task<BankDecision> RecordDecisionAsync(BankDecision decision)
    {
        var existing = await _db.BankDecisions.FirstOrDefaultAsync(d => d.LoanRequestId == decision.LoanRequestId);
        if (existing != null)
        {
            existing.DecisionType = decision.DecisionType;
            existing.DecisionDate = decision.DecisionDate;
            existing.DecisionRemarks = decision.DecisionRemarks;
            existing.DeclineReasonCodeId = decision.DeclineReasonCodeId;
            existing.ApprovedFacilityType = decision.ApprovedFacilityType;
            existing.ApprovedAmount = decision.ApprovedAmount;
            existing.ApprovedTenorMonths = decision.ApprovedTenorMonths;
            existing.AdditionalConditions = decision.AdditionalConditions;
            existing.MadeByUserId = decision.MadeByUserId;
            decision = existing;
        }
        else
        {
            decision.CreatedAt = DateTime.UtcNow;
            _db.BankDecisions.Add(decision);
        }
        await _db.SaveChangesAsync();
        return decision;
    }

    public async Task<List<DeclineReasonCode>> GetActiveReasonCodesAsync()
        => await _db.DeclineReasonCodes
            .Where(c => c.IsActive)
            .OrderBy(c => c.Category).ThenBy(c => c.DisplayOrder)
            .ToListAsync();
}

public class ConditionalOfferService : IConditionalOfferService
{
    private readonly ApplicationDbContext _db;
    public ConditionalOfferService(ApplicationDbContext db) => _db = db;

    public async Task<List<ConditionalOffer>> GetByLoanRequestAsync(int loanRequestId)
        => await _db.ConditionalOffers
            .Include(o => o.CreatedBy)
            .Include(o => o.Response)
            .Where(o => o.LoanRequestId == loanRequestId)
            .OrderByDescending(o => o.OfferVersion)
            .ToListAsync();

    public async Task<ConditionalOffer?> GetByIdAsync(int id)
        => await _db.ConditionalOffers
            .Include(o => o.CreatedBy)
            .Include(o => o.Response).ThenInclude(r => r!.RespondedBy)
            .Include(o => o.LoanRequest)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<ConditionalOffer> CreateOfferAsync(ConditionalOffer offer)
    {
        // Supersede any existing active offers for the same application
        var active = await _db.ConditionalOffers
            .Where(o => o.LoanRequestId == offer.LoanRequestId && o.Status == OfferStatus.Issued)
            .ToListAsync();
        foreach (var o in active) o.Status = OfferStatus.Superseded;

        offer.OfferNumber = await GenerateOfferNumberAsync();
        offer.OfferVersion = active.Count + 1;
        offer.Status = OfferStatus.Issued;
        offer.CreatedAt = DateTime.UtcNow;
        _db.ConditionalOffers.Add(offer);
        await _db.SaveChangesAsync();
        return offer;
    }

    public async Task<ConditionalOffer> RecordResponseAsync(int offerId, string responseType, int respondedByUserId, string? remarks, string? ipAddress)
    {
        var offer = await _db.ConditionalOffers.Include(o => o.Response).FirstOrDefaultAsync(o => o.Id == offerId)
            ?? throw new InvalidOperationException("Offer not found.");

        if (offer.Status != OfferStatus.Issued)
            throw new InvalidOperationException($"Offer cannot be responded to in its current status: {offer.Status}");
        if (offer.ExpiryDate < DateTime.UtcNow)
            throw new InvalidOperationException("This offer has expired.");

        offer.Status = responseType == "Accepted" ? OfferStatus.Accepted : OfferStatus.Rejected;

        if (offer.Response == null)
        {
            _db.ConditionalOfferResponses.Add(new ConditionalOfferResponse
            {
                ConditionalOfferId = offerId, ResponseType = responseType,
                ResponseDate = DateTime.UtcNow, RespondedByUserId = respondedByUserId,
                IpAddress = ipAddress, Remarks = remarks, OfferVersion = offer.OfferVersion
            });
        }
        await _db.SaveChangesAsync();
        return offer;
    }

    public async Task ExpireOffersAsync()
    {
        var expired = await _db.ConditionalOffers
            .Where(o => o.Status == OfferStatus.Issued && o.ExpiryDate < DateTime.UtcNow)
            .ToListAsync();
        foreach (var o in expired) o.Status = OfferStatus.Expired;
        await _db.SaveChangesAsync();
    }

    public async Task<string> GenerateOfferNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"CO-{year}-";
        var last = await _db.ConditionalOffers
            .Where(o => o.OfferNumber.StartsWith(prefix))
            .OrderByDescending(o => o.OfferNumber)
            .Select(o => o.OfferNumber)
            .FirstOrDefaultAsync();
        int seq = 1;
        if (last != null && int.TryParse(last.Substring(prefix.Length), out int n)) seq = n + 1;
        return $"{prefix}{seq:D6}";
    }
}

public class PostApprovalService : IPostApprovalService
{
    private readonly ApplicationDbContext _db;
    private readonly IFileUploadService _fileUpload;

    public PostApprovalService(ApplicationDbContext db, IFileUploadService fileUpload)
    {
        _db = db;
        _fileUpload = fileUpload;
    }

    public async Task<PostApprovalChecklist?> GetChecklistByLoanRequestAsync(int loanRequestId)
        => await _db.PostApprovalChecklists
            .Include(c => c.Items).ThenInclude(i => i.SubmittedBy)
            .Include(c => c.Items).ThenInclude(i => i.VerifiedBy)
            .Include(c => c.CreatedBy)
            .FirstOrDefaultAsync(c => c.LoanRequestId == loanRequestId);

    public async Task<PostApprovalChecklist> CreateChecklistAsync(PostApprovalChecklist checklist, List<PostApprovalChecklistItem> items)
    {
        checklist.CreatedAt = DateTime.UtcNow;
        _db.PostApprovalChecklists.Add(checklist);
        await _db.SaveChangesAsync();
        foreach (var item in items)
        {
            item.ChecklistId = checklist.Id;
            item.VerificationStatus = ChecklistItemStatus.Pending;
            _db.PostApprovalChecklistItems.Add(item);
        }
        await _db.SaveChangesAsync();
        return checklist;
    }

    public async Task<PostApprovalChecklistItem?> GetItemByIdAsync(int itemId)
        => await _db.PostApprovalChecklistItems.Include(i => i.Checklist).FirstOrDefaultAsync(i => i.Id == itemId);

    public async Task<PostApprovalChecklistItem> SubmitItemDocumentAsync(int itemId, IFormFile document, int uploadedByUserId)
    {
        var item = await _db.PostApprovalChecklistItems.Include(i => i.Checklist).FirstOrDefaultAsync(i => i.Id == itemId)
            ?? throw new InvalidOperationException("Checklist item not found.");

        var (fileName, origName, filePath, contentType, size) = await _fileUpload.UploadAsync(document, "post-approval");
        item.SubmittedDocumentPath = filePath;
        item.SubmittedAt = DateTime.UtcNow;
        item.SubmittedByUserId = uploadedByUserId;
        item.VerificationStatus = ChecklistItemStatus.Submitted;

        _db.ApplicationDocuments.Add(new ApplicationDocument
        {
            LoanRequestId = item.Checklist.LoanRequestId,
            ChecklistItemId = itemId,
            DocumentType = DocumentType.PostApproval,
            FileName = fileName, OriginalFileName = origName, FilePath = filePath,
            ContentType = contentType, FileSize = size, UploadedByUserId = uploadedByUserId,
            UploadedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return item;
    }

    public async Task<PostApprovalChecklistItem> VerifyItemAsync(int itemId, string status, int verifiedByUserId, string? remarks)
    {
        var item = await _db.PostApprovalChecklistItems.FindAsync(itemId)
            ?? throw new InvalidOperationException("Checklist item not found.");
        item.VerificationStatus = status;
        item.VerifiedByUserId = verifiedByUserId;
        item.VerifiedAt = DateTime.UtcNow;
        item.VerificationRemarks = remarks;
        await _db.SaveChangesAsync();
        return item;
    }
}

public class DisbursementService : IDisbursementService
{
    private readonly ApplicationDbContext _db;
    public DisbursementService(ApplicationDbContext db) => _db = db;

    public async Task<Disbursement?> GetByLoanRequestAsync(int loanRequestId)
        => await _db.Disbursements.Include(d => d.UpdatedBy).FirstOrDefaultAsync(d => d.LoanRequestId == loanRequestId);

    public async Task<Disbursement> CreateOrUpdateAsync(Disbursement disbursement)
    {
        var existing = await _db.Disbursements.FirstOrDefaultAsync(d => d.LoanRequestId == disbursement.LoanRequestId);
        if (existing == null)
        {
            disbursement.CreatedAt = DateTime.UtcNow;
            _db.Disbursements.Add(disbursement);
        }
        else
        {
            existing.DisbursementStatus = disbursement.DisbursementStatus;
            existing.DisbursedAmount = disbursement.DisbursedAmount;
            existing.ValueDate = disbursement.ValueDate;
            existing.DisbursementAccount = disbursement.DisbursementAccount;
            existing.BankReferenceNumber = disbursement.BankReferenceNumber;
            existing.Remarks = disbursement.Remarks;
            existing.UpdatedByUserId = disbursement.UpdatedByUserId;
            existing.UpdatedAt = DateTime.UtcNow;
            disbursement = existing;
        }
        await _db.SaveChangesAsync();
        return disbursement;
    }
}

public class ApplicationMonitoringService : IApplicationMonitoringService
{
    private readonly ApplicationDbContext _db;
    public ApplicationMonitoringService(ApplicationDbContext db) => _db = db;

    public async Task<ApplicationMonitoring?> GetByLoanRequestAsync(int loanRequestId)
        => await _db.ApplicationMonitorings.Include(m => m.UpdatedBy).FirstOrDefaultAsync(m => m.LoanRequestId == loanRequestId);

    public async Task<ApplicationMonitoring> CreateOrUpdateAsync(ApplicationMonitoring monitoring)
    {
        var existing = await _db.ApplicationMonitorings.FirstOrDefaultAsync(m => m.LoanRequestId == monitoring.LoanRequestId);
        if (existing == null)
        {
            monitoring.CreatedAt = DateTime.UtcNow;
            _db.ApplicationMonitorings.Add(monitoring);
        }
        else
        {
            existing.MonitoringStatus = monitoring.MonitoringStatus;
            existing.Notes = monitoring.Notes;
            existing.NextReviewDate = monitoring.NextReviewDate;
            existing.UpdatedByUserId = monitoring.UpdatedByUserId;
            existing.UpdatedAt = DateTime.UtcNow;
            monitoring = existing;
        }
        await _db.SaveChangesAsync();
        return monitoring;
    }
}
