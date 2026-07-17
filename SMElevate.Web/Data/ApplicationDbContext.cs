using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Models.Common;

namespace SMElevate.Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // Core
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<UserExternalLogin> UserExternalLogins => Set<UserExternalLogin>();
    public DbSet<Bank> Banks => Set<Bank>();
    public DbSet<BankMember> BankMembers => Set<BankMember>();
    public DbSet<EndUserProfile> EndUserProfiles => Set<EndUserProfile>();

    // Business Profiles
    public DbSet<BusinessProfile> BusinessProfiles => Set<BusinessProfile>();
    public DbSet<BusinessShareholder> BusinessShareholders => Set<BusinessShareholder>();

    // Loan Request
    public DbSet<LoanRequest> LoanRequests => Set<LoanRequest>();
    public DbSet<LoanRequestShareholder> LoanRequestShareholders => Set<LoanRequestShareholder>();
    public DbSet<LoanRequestStatusHistory> LoanRequestStatusHistories => Set<LoanRequestStatusHistory>();
    public DbSet<LoanRequestAttachment> LoanRequestAttachments => Set<LoanRequestAttachment>();
    public DbSet<LoanRequestFieldValue> LoanRequestFieldValues => Set<LoanRequestFieldValue>();

    // Scheme / Form Builder
    public DbSet<Scheme> Schemes => Set<Scheme>();
    public DbSet<SchemeForm> SchemeForms => Set<SchemeForm>();
    public DbSet<SchemeFormField> SchemeFormFields => Set<SchemeFormField>();
    public DbSet<SchemeFormFieldConfiguration> SchemeFormFieldConfigurations => Set<SchemeFormFieldConfiguration>();

    // Lookups
    public DbSet<MasterLookup> MasterLookups => Set<MasterLookup>();
    public DbSet<MasterLookupValue> MasterLookupValues => Set<MasterLookupValue>();

    // Notifications & Email
    public DbSet<AppNotification> AppNotifications => Set<AppNotification>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();

    // System
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Workflow configuration
    public DbSet<WorkflowStatus> WorkflowStatuses => Set<WorkflowStatus>();
    public DbSet<WorkflowTransition> WorkflowTransitions => Set<WorkflowTransition>();
    public DbSet<DeclineReasonCode> DeclineReasonCodes => Set<DeclineReasonCode>();

    // Application lifecycle modules
    public DbSet<BankAssessment> BankAssessments => Set<BankAssessment>();
    public DbSet<AdditionalInformationRequest> AdditionalInfoRequests => Set<AdditionalInformationRequest>();
    public DbSet<BankDecision> BankDecisions => Set<BankDecision>();
    public DbSet<ConditionalOffer> ConditionalOffers => Set<ConditionalOffer>();
    public DbSet<ConditionalOfferResponse> ConditionalOfferResponses => Set<ConditionalOfferResponse>();
    public DbSet<PostApprovalChecklist> PostApprovalChecklists => Set<PostApprovalChecklist>();
    public DbSet<PostApprovalChecklistItem> PostApprovalChecklistItems => Set<PostApprovalChecklistItem>();
    public DbSet<ApplicationDocument> ApplicationDocuments => Set<ApplicationDocument>();
    public DbSet<Disbursement> Disbursements => Set<Disbursement>();
    public DbSet<ApplicationMonitoring> ApplicationMonitorings => Set<ApplicationMonitoring>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enums stored as strings
        modelBuilder.Entity<ApplicationUser>()
            .Property(u => u.UserType).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<AppNotification>()
            .Property(n => n.NotificationType).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<EmailTemplate>()
            .Property(t => t.RecipientType).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<EmailLog>()
            .Property(l => l.Status).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<SystemSetting>()
            .Property(s => s.SettingCategory).HasConversion<string>().HasMaxLength(30);

        // Unique indexes
        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.EmailAddress).IsUnique();
        modelBuilder.Entity<LoanRequest>()
            .HasIndex(r => r.RequestNo).IsUnique();
        modelBuilder.Entity<MasterLookup>()
            .HasIndex(l => l.LookupCode).IsUnique();
        modelBuilder.Entity<SystemSetting>()
            .HasIndex(s => s.SettingKey).IsUnique();
        modelBuilder.Entity<EmailTemplate>()
            .HasIndex(t => t.TemplateCode).IsUnique();

        // UserExternalLogin -> ApplicationUser
        modelBuilder.Entity<UserExternalLogin>()
            .HasOne(l => l.User).WithMany()
            .HasForeignKey(l => l.UserId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<UserExternalLogin>()
            .HasIndex(l => new { l.Provider, l.ProviderUserId }).IsUnique();
        modelBuilder.Entity<UserExternalLogin>()
            .HasIndex(l => new { l.UserId, l.Provider }).IsUnique();

        // ApplicationUser -> Role (nullable FK, no cascade since role may stay if user deleted)
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Role).WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId).OnDelete(DeleteBehavior.SetNull);

        // ApplicationUser -> Bank (primary bank, nullable FK)
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Bank).WithMany(b => b.Users)
            .HasForeignKey(u => u.BankId).OnDelete(DeleteBehavior.SetNull);

        // EndUserProfile -> ApplicationUser (one-to-one)
        modelBuilder.Entity<EndUserProfile>()
            .HasOne(p => p.User).WithOne(u => u.Profile)
            .HasForeignKey<EndUserProfile>(p => p.UserId).OnDelete(DeleteBehavior.Cascade);

        // BankMember -> Bank
        modelBuilder.Entity<BankMember>()
            .HasOne(m => m.Bank).WithMany(b => b.Members)
            .HasForeignKey(m => m.BankId).OnDelete(DeleteBehavior.Cascade);

        // BankMember -> ApplicationUser (no cascade to avoid cycle)
        modelBuilder.Entity<BankMember>()
            .HasOne(m => m.User).WithMany(u => u.BankMemberships)
            .HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.NoAction);

        // LoanRequest -> ApplicationUser (no cascade - need user for history)
        modelBuilder.Entity<LoanRequest>()
            .HasOne(r => r.User).WithMany(u => u.LoanRequests)
            .HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.NoAction);

        // LoanRequest -> Bank (assigned bank)
        modelBuilder.Entity<LoanRequest>()
            .HasOne(r => r.AssignedBank).WithMany(b => b.AssignedRequests)
            .HasForeignKey(r => r.AssignedBankId).OnDelete(DeleteBehavior.SetNull);

        // LoanRequest -> Status (MasterLookupValue)
        modelBuilder.Entity<LoanRequest>()
            .HasOne(r => r.Status).WithMany()
            .HasForeignKey(r => r.StatusId).OnDelete(DeleteBehavior.SetNull);

        // LoanRequestShareholder -> LoanRequest
        modelBuilder.Entity<LoanRequestShareholder>()
            .HasOne(s => s.LoanRequest).WithMany(r => r.Shareholders)
            .HasForeignKey(s => s.LoanRequestId).OnDelete(DeleteBehavior.Cascade);

        // LoanRequestStatusHistory -> LoanRequest
        modelBuilder.Entity<LoanRequestStatusHistory>()
            .HasOne(h => h.LoanRequest).WithMany(r => r.StatusHistory)
            .HasForeignKey(h => h.LoanRequestId).OnDelete(DeleteBehavior.Cascade);

        // LoanRequestStatusHistory -> OldStatus (MasterLookupValue)
        modelBuilder.Entity<LoanRequestStatusHistory>()
            .HasOne(h => h.OldStatus).WithMany()
            .HasForeignKey(h => h.OldStatusId).OnDelete(DeleteBehavior.NoAction);

        // LoanRequestStatusHistory -> NewStatus (MasterLookupValue)
        modelBuilder.Entity<LoanRequestStatusHistory>()
            .HasOne(h => h.NewStatus).WithMany()
            .HasForeignKey(h => h.NewStatusId).OnDelete(DeleteBehavior.NoAction);

        // LoanRequestStatusHistory -> ChangedBy
        modelBuilder.Entity<LoanRequestStatusHistory>()
            .HasOne(h => h.ChangedBy).WithMany()
            .HasForeignKey(h => h.ChangedByUserId).OnDelete(DeleteBehavior.NoAction);

        // LoanRequest -> Scheme (dynamic form link)
        modelBuilder.Entity<LoanRequest>()
            .HasOne(r => r.Scheme).WithMany()
            .HasForeignKey(r => r.SchemeId).OnDelete(DeleteBehavior.SetNull);

        // LoanRequest -> SchemeForm (dynamic form link)
        modelBuilder.Entity<LoanRequest>()
            .HasOne(r => r.SchemeForm).WithMany()
            .HasForeignKey(r => r.SchemeFormId).OnDelete(DeleteBehavior.SetNull);

        // LoanRequestFieldValue -> LoanRequest
        modelBuilder.Entity<LoanRequestFieldValue>()
            .HasOne(fv => fv.LoanRequest).WithMany(r => r.FieldValues)
            .HasForeignKey(fv => fv.LoanRequestId).OnDelete(DeleteBehavior.Cascade);

        // LoanRequestAttachment -> LoanRequest
        modelBuilder.Entity<LoanRequestAttachment>()
            .HasOne(a => a.LoanRequest).WithMany(r => r.Attachments)
            .HasForeignKey(a => a.LoanRequestId).OnDelete(DeleteBehavior.Cascade);

        // LoanRequestAttachment -> UploadedBy
        modelBuilder.Entity<LoanRequestAttachment>()
            .HasOne(a => a.UploadedBy).WithMany()
            .HasForeignKey(a => a.UploadedByUserId).OnDelete(DeleteBehavior.NoAction);

        // MasterLookupValue -> MasterLookup
        modelBuilder.Entity<MasterLookupValue>()
            .HasOne(v => v.MasterLookup).WithMany(l => l.Values)
            .HasForeignKey(v => v.MasterLookupId).OnDelete(DeleteBehavior.Cascade);

        // Scheme -> CreatedBy
        modelBuilder.Entity<Scheme>()
            .HasOne(s => s.CreatedBy).WithMany()
            .HasForeignKey(s => s.CreatedByUserId).OnDelete(DeleteBehavior.SetNull);

        // Scheme -> SchemeForm (one-to-one via FormId)
        modelBuilder.Entity<Scheme>()
            .HasOne(s => s.Form).WithOne(f => f.Scheme)
            .HasForeignKey<Scheme>(s => s.FormId).OnDelete(DeleteBehavior.SetNull);

        // SchemeFormField -> SchemeForm
        modelBuilder.Entity<SchemeFormField>()
            .HasOne(f => f.SchemeForm).WithMany(sf => sf.Fields)
            .HasForeignKey(f => f.SchemeFormId).OnDelete(DeleteBehavior.Cascade);

        // SchemeFormField -> MasterLookup
        modelBuilder.Entity<SchemeFormField>()
            .HasOne(f => f.Lookup).WithMany(l => l.FormFields)
            .HasForeignKey(f => f.LookupId).OnDelete(DeleteBehavior.SetNull);

        // SchemeFormFieldConfiguration -> Scheme
        modelBuilder.Entity<SchemeFormFieldConfiguration>()
            .HasOne(c => c.Scheme).WithMany()
            .HasForeignKey(c => c.SchemeId).OnDelete(DeleteBehavior.Cascade);

        // AppNotification -> User
        modelBuilder.Entity<AppNotification>()
            .HasOne(n => n.User).WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);

        // AuditLog -> User (nullable, no cascade)
        modelBuilder.Entity<AuditLog>()
            .HasOne(a => a.User).WithMany(u => u.AuditLogs)
            .HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.SetNull);

        // LoanRequest -> IdentifiedBank (nullable FK, no cascade)
        modelBuilder.Entity<LoanRequest>()
            .HasOne(r => r.IdentifiedBank).WithMany()
            .HasForeignKey(r => r.IdentifiedBankId).OnDelete(DeleteBehavior.SetNull);

        // Filtered unique index on CaseId (NULL rows excluded)
        modelBuilder.Entity<LoanRequest>()
            .HasIndex(r => r.CaseId)
            .IsUnique()
            .HasFilter("[CaseId] IS NOT NULL");

        // Workflow configuration
        modelBuilder.Entity<WorkflowStatus>()
            .HasIndex(w => w.StatusCode).IsUnique();
        modelBuilder.Entity<WorkflowTransition>()
            .HasIndex(t => new { t.FromStatusCode, t.ToStatusCode }).IsUnique();
        modelBuilder.Entity<DeclineReasonCode>()
            .HasIndex(d => d.Code).IsUnique();

        // BankAssessment -> LoanRequest
        modelBuilder.Entity<BankAssessment>()
            .HasOne(a => a.LoanRequest).WithMany(r => r.BankAssessments)
            .HasForeignKey(a => a.LoanRequestId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BankAssessment>()
            .HasOne(a => a.UpdatedBy).WithMany()
            .HasForeignKey(a => a.UpdatedByUserId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<BankAssessment>()
            .HasIndex(a => new { a.LoanRequestId, a.AssessmentType }).IsUnique();

        // AdditionalInformationRequest -> LoanRequest
        modelBuilder.Entity<AdditionalInformationRequest>()
            .HasOne(r => r.LoanRequest).WithMany(l => l.AdditionalInfoRequests)
            .HasForeignKey(r => r.LoanRequestId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<AdditionalInformationRequest>()
            .HasOne(r => r.CreatedBy).WithMany()
            .HasForeignKey(r => r.CreatedByUserId).OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<AdditionalInformationRequest>()
            .HasOne(r => r.RespondedBy).WithMany()
            .HasForeignKey(r => r.RespondedByUserId).OnDelete(DeleteBehavior.SetNull);

        // BankDecision -> LoanRequest (one per application)
        modelBuilder.Entity<BankDecision>()
            .HasOne(d => d.LoanRequest).WithOne(r => r.BankDecision)
            .HasForeignKey<BankDecision>(d => d.LoanRequestId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BankDecision>()
            .HasOne(d => d.MadeBy).WithMany()
            .HasForeignKey(d => d.MadeByUserId).OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<BankDecision>()
            .HasOne(d => d.DeclineReasonCode).WithMany()
            .HasForeignKey(d => d.DeclineReasonCodeId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<BankDecision>()
            .Property(d => d.ApprovedAmount).HasPrecision(18, 2);

        // ConditionalOffer -> LoanRequest
        modelBuilder.Entity<ConditionalOffer>()
            .HasOne(o => o.LoanRequest).WithMany(r => r.ConditionalOffers)
            .HasForeignKey(o => o.LoanRequestId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ConditionalOffer>()
            .HasOne(o => o.CreatedBy).WithMany()
            .HasForeignKey(o => o.CreatedByUserId).OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<ConditionalOffer>()
            .HasIndex(o => o.OfferNumber).IsUnique();
        modelBuilder.Entity<ConditionalOffer>()
            .Property(o => o.ApprovedAmount).HasPrecision(18, 2);

        // ConditionalOfferResponse -> ConditionalOffer (one-to-one)
        modelBuilder.Entity<ConditionalOfferResponse>()
            .HasOne(r => r.ConditionalOffer).WithOne(o => o.Response)
            .HasForeignKey<ConditionalOfferResponse>(r => r.ConditionalOfferId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ConditionalOfferResponse>()
            .HasOne(r => r.RespondedBy).WithMany()
            .HasForeignKey(r => r.RespondedByUserId).OnDelete(DeleteBehavior.NoAction);

        // PostApprovalChecklist -> LoanRequest
        modelBuilder.Entity<PostApprovalChecklist>()
            .HasOne(c => c.LoanRequest).WithMany(r => r.PostApprovalChecklists)
            .HasForeignKey(c => c.LoanRequestId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PostApprovalChecklist>()
            .HasOne(c => c.CreatedBy).WithMany()
            .HasForeignKey(c => c.CreatedByUserId).OnDelete(DeleteBehavior.NoAction);

        // PostApprovalChecklistItem -> PostApprovalChecklist
        modelBuilder.Entity<PostApprovalChecklistItem>()
            .HasOne(i => i.Checklist).WithMany(c => c.Items)
            .HasForeignKey(i => i.ChecklistId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PostApprovalChecklistItem>()
            .HasOne(i => i.SubmittedBy).WithMany()
            .HasForeignKey(i => i.SubmittedByUserId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<PostApprovalChecklistItem>()
            .HasOne(i => i.VerifiedBy).WithMany()
            .HasForeignKey(i => i.VerifiedByUserId).OnDelete(DeleteBehavior.SetNull);

        // ApplicationDocument -> LoanRequest
        modelBuilder.Entity<ApplicationDocument>()
            .HasOne(d => d.LoanRequest).WithMany(r => r.Documents)
            .HasForeignKey(d => d.LoanRequestId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ApplicationDocument>()
            .HasOne(d => d.UploadedBy).WithMany()
            .HasForeignKey(d => d.UploadedByUserId).OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<ApplicationDocument>()
            .HasOne(d => d.AdditionalInfoRequest).WithMany(r => r.ResponseDocuments)
            .HasForeignKey(d => d.AdditionalInfoRequestId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<ApplicationDocument>()
            .HasOne(d => d.ChecklistItem).WithMany()
            .HasForeignKey(d => d.ChecklistItemId).OnDelete(DeleteBehavior.SetNull);

        // Disbursement -> LoanRequest (one per application)
        modelBuilder.Entity<Disbursement>()
            .HasOne(d => d.LoanRequest).WithOne(r => r.Disbursement)
            .HasForeignKey<Disbursement>(d => d.LoanRequestId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Disbursement>()
            .HasOne(d => d.UpdatedBy).WithMany()
            .HasForeignKey(d => d.UpdatedByUserId).OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<Disbursement>()
            .Property(d => d.ApprovedAmount).HasPrecision(18, 2);
        modelBuilder.Entity<Disbursement>()
            .Property(d => d.DisbursedAmount).HasPrecision(18, 2);

        // ApplicationMonitoring -> LoanRequest (one per application)
        modelBuilder.Entity<ApplicationMonitoring>()
            .HasOne(m => m.LoanRequest).WithOne(r => r.Monitoring)
            .HasForeignKey<ApplicationMonitoring>(m => m.LoanRequestId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ApplicationMonitoring>()
            .HasOne(m => m.UpdatedBy).WithMany()
            .HasForeignKey(m => m.UpdatedByUserId).OnDelete(DeleteBehavior.NoAction);

        // Decimal precision
        modelBuilder.Entity<LoanRequest>()
            .Property(r => r.AnnualSales).HasPrecision(18, 2);
        modelBuilder.Entity<LoanRequest>()
            .Property(r => r.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<LoanRequestShareholder>()
            .Property(s => s.ShareholdingPercentage).HasPrecision(5, 2);

        // BusinessProfile -> ApplicationUser (no cascade to avoid cycle)
        modelBuilder.Entity<BusinessProfile>()
            .HasOne(b => b.User).WithMany()
            .HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<BusinessProfile>()
            .HasIndex(b => b.UserId);
        modelBuilder.Entity<BusinessProfile>()
            .HasIndex(b => new { b.UserId, b.IsActive });
        modelBuilder.Entity<BusinessProfile>()
            .Property(b => b.AnnualSales).HasPrecision(18, 2);

        // BusinessShareholder -> BusinessProfile
        modelBuilder.Entity<BusinessShareholder>()
            .HasOne(s => s.BusinessProfile).WithMany(b => b.Shareholders)
            .HasForeignKey(s => s.BusinessProfileId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BusinessShareholder>()
            .HasIndex(s => s.BusinessProfileId);
        modelBuilder.Entity<BusinessShareholder>()
            .Property(s => s.ShareholdingPercentage).HasPrecision(5, 2);

        // LoanRequest -> BusinessProfile (nullable, no cascade — preserves historical data)
        modelBuilder.Entity<LoanRequest>()
            .HasOne(r => r.BusinessProfile).WithMany(b => b.LoanRequests)
            .HasForeignKey(r => r.BusinessProfileId).OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<LoanRequest>()
            .HasIndex(r => r.BusinessProfileId);

        // BusinessProfile -> Bank (nullable bank detail — no cascade)
        modelBuilder.Entity<BusinessProfile>()
            .HasOne(b => b.BusinessBank).WithMany()
            .HasForeignKey(b => b.BusinessBankId).OnDelete(DeleteBehavior.NoAction);
    }
}
