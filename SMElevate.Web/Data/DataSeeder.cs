using Microsoft.AspNetCore.Identity;
using SMElevate.Web.Models.Common;

namespace SMElevate.Web.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        await SeedRolesAsync(db);
        await SeedLookupsAsync(db);
        await SeedNewLookupValuesAsync(db);
        await SeedAdminUserAsync(db);
        await SeedEmailTemplatesAsync(db);
        await SeedNewEmailTemplatesAsync(db);
        await SeedSystemSettingsAsync(db);
        await SeedDemoBanksAndUsersAsync(db);
        await SeedWorkflowStatusesAsync(db);
        await SeedWorkflowTransitionsAsync(db);
        await SeedDeclineReasonCodesAsync(db);
        await SeedBusinessProfileEmailTemplatesAsync(db);
    }

    private static async Task SeedRolesAsync(ApplicationDbContext db)
    {
        var roles = new[] { "EndUser", "Bank", "SBP", "Admin" };
        foreach (var name in roles)
        {
            if (!db.Roles.Any(r => r.RoleName == name))
            {
                db.Roles.Add(new Role { RoleName = name, Description = $"{name} role", IsActive = true, CreatedAt = DateTime.UtcNow });
            }
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedLookupsAsync(ApplicationDbContext db)
    {
        var lookups = new Dictionary<string, (string Name, string Desc, string[] Values)>
        {
            [LookupCodes.Status] = ("Application Status", "SME loan application statuses",
                new[] { AppStatus.Submitted, AppStatus.Assigned, AppStatus.InProcess,
                    AppStatus.Approved, AppStatus.Rejected, AppStatus.MoreInformationRequired, AppStatus.Completed }),
            [LookupCodes.Tenor] = ("Tenor", "Facility tenor in months",
                new[] { "6", "12", "24", "36", "60" }),
            [LookupCodes.BusinessNature] = ("Business Nature", "Nature of business operations",
                new[] { "Manufacturing", "Services", "Trading" }),
            [LookupCodes.BusinessPremise] = ("Business Premise", "Ownership of business premises",
                new[] { "Owned", "Rented" }),
            [LookupCodes.BusinessStatus] = ("Business Status", "Legal structure of business",
                new[] { "Proprietorship", "Partnership", "Pvt Ltd Company" }),
            [LookupCodes.FacilityType] = ("Facility Type", "Type of credit facility",
                new[] { "Running Finance", "Term Finance", "Trade Finance", "Lease Finance", "Demand Finance" }),
            [LookupCodes.Gender] = ("Gender", "Gender of proprietor",
                new[] { "Male", "Female", "Other / Prefer not to say" }),
            [LookupCodes.UserTypeLookup] = ("User Type", "Portal user categories",
                new[] { "SME", "Bank", "SBP", "Admin" }),
            [LookupCodes.IsBusinessRegistered] = ("Is Business Registered", "Business registration status",
                new[] { "Yes", "No" }),
            [LookupCodes.TypeOfFacility] = ("Type of Facility", "Secured or unsecured financing",
                new[] { "Secured", "Unsecured" }),
            [LookupCodes.IBANOrRaastType] = ("Payment Identifier Type", "IBAN or RAAST ID",
                new[] { "IBAN", "RAAST ID" }),
        };

        foreach (var (code, (name, desc, values)) in lookups)
        {
            if (!db.MasterLookups.Any(l => l.LookupCode == code))
            {
                var lookup = new MasterLookup { LookupName = name, LookupCode = code, Description = desc, IsActive = true, CreatedAt = DateTime.UtcNow };
                db.MasterLookups.Add(lookup);
                await db.SaveChangesAsync();

                var order = 0;
                foreach (var val in values)
                {
                    db.MasterLookupValues.Add(new MasterLookupValue
                    {
                        MasterLookupId = lookup.Id,
                        ValueText = val,
                        ValueCode = val.Replace(" ", "_").ToUpper(),
                        DisplayOrder = order++,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                await db.SaveChangesAsync();
            }
        }
    }

    private static async Task SeedAdminUserAsync(ApplicationDbContext db)
    {
        const string adminEmail = "admin@smeelevate.local";
        if (db.Users.Any(u => u.EmailAddress == adminEmail)) return;

        var adminRole = db.Roles.FirstOrDefault(r => r.RoleName == "Admin");
        var hasher = new PasswordHasher<ApplicationUser>();
        var admin = new ApplicationUser
        {
            FullName = "System Admin",
            EmailAddress = adminEmail,
            MobileNo = "03000000000",
            UserType = UserType.Admin,
            RoleId = adminRole?.Id,
            IsActive = true,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow
        };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin@12345");
        db.Users.Add(admin);
        await db.SaveChangesAsync();
    }

    private static async Task SeedEmailTemplatesAsync(ApplicationDbContext db)
    {
        var templates = new[]
        {
            ("ENDUSER_PROFILE_VERIFICATION", "EndUser: Profile Verification OTP", "Verify your SMElevate account",
                "<p>Dear {{FullName}},</p><p>Your verification token for SMElevate is: <strong>{{VerificationToken}}</strong></p><p>This token expires in 24 hours.</p><p>Portal: {{PortalUrl}}</p>",
                EmailRecipientType.EndUser),
            ("ENDUSER_PROFILE_VERIFIED", "EndUser: Profile Verified", "Your SMElevate account is verified",
                "<p>Dear {{FullName}},</p><p>Your SMElevate profile has been verified successfully. You can now submit SME loan requests.</p><p>Portal: {{PortalUrl}}</p>",
                EmailRecipientType.EndUser),
            ("ENDUSER_LOAN_SUBMITTED", "EndUser: Loan Request Submitted", "SME Loan Request {{RequestNo}} Submitted",
                "<p>Dear {{FullName}},</p><p>Your loan request <strong>{{RequestNo}}</strong> for <strong>{{BusinessName}}</strong> has been submitted to <strong>{{BankName}}</strong> on {{SubmittedDate}}.</p><p>Track at: {{PortalUrl}}</p>",
                EmailRecipientType.EndUser),
            ("ENDUSER_STATUS_ASSIGNED", "EndUser: Request Assigned to Bank", "Your request {{RequestNo}} has been assigned",
                "<p>Dear {{FullName}},</p><p>Request <strong>{{RequestNo}}</strong> has been assigned to <strong>{{BankName}}</strong> for review.</p>",
                EmailRecipientType.EndUser),
            ("ENDUSER_STATUS_IN_PROCESS", "EndUser: Request In Process", "Your request {{RequestNo}} is being reviewed",
                "<p>Dear {{FullName}},</p><p>Request <strong>{{RequestNo}}</strong> is now In Process at <strong>{{BankName}}</strong>.</p><p>Remarks: {{Remarks}}</p>",
                EmailRecipientType.EndUser),
            ("ENDUSER_STATUS_APPROVED", "EndUser: Request Approved", "Congratulations! Request {{RequestNo}} Approved",
                "<p>Dear {{FullName}},</p><p>We are pleased to inform you that your loan request <strong>{{RequestNo}}</strong> for <strong>{{BusinessName}}</strong> has been <strong>Approved</strong> by <strong>{{BankName}}</strong>.</p><p>Remarks: {{Remarks}}</p>",
                EmailRecipientType.EndUser),
            ("ENDUSER_STATUS_REJECTED", "EndUser: Request Rejected", "SME Loan Request {{RequestNo}} Rejected",
                "<p>Dear {{FullName}},</p><p>We regret to inform you that your loan request <strong>{{RequestNo}}</strong> has been Rejected by <strong>{{BankName}}</strong>.</p><p>Reason: {{Remarks}}</p>",
                EmailRecipientType.EndUser),
            ("ENDUSER_STATUS_MORE_INFORMATION_REQUIRED", "EndUser: More Information Required", "More Information Required for {{RequestNo}}",
                "<p>Dear {{FullName}},</p><p><strong>{{BankName}}</strong> requires additional information for your request <strong>{{RequestNo}}</strong>.</p><p>Remarks: {{Remarks}}</p>",
                EmailRecipientType.EndUser),
            ("ENDUSER_STATUS_COMPLETED", "EndUser: Request Completed", "Request {{RequestNo}} Completed",
                "<p>Dear {{FullName}},</p><p>Your loan request <strong>{{RequestNo}}</strong> has been marked as Completed by <strong>{{BankName}}</strong>.</p>",
                EmailRecipientType.EndUser),
            ("BANK_OTP_LOGIN", "Bank: OTP Login", "SMElevate Banks Portal - Login OTP",
                "<p>Dear {{FullName}},</p><p>Your one-time password (OTP) to access the SMElevate Banks Portal is: <strong>{{OTP}}</strong></p><p>This OTP is valid for 10 minutes. Do not share it with anyone.</p>",
                EmailRecipientType.Bank),
            ("BANK_NEW_REQUEST_ASSIGNED", "Bank: New Request Assigned", "New SME Loan Request {{RequestNo}} Assigned to Your Bank",
                "<p>A new SME loan request has been assigned to your bank.</p><p>Request No: <strong>{{RequestNo}}</strong><br>Business: <strong>{{BusinessName}}</strong><br>Submitted: {{SubmittedDate}}</p><p>Login to review: {{PortalUrl}}</p>",
                EmailRecipientType.Bank),
            ("BANK_REQUEST_UPDATED", "Bank: Request Status Updated", "Status Update Confirmation - {{RequestNo}}",
                "<p>Status for request <strong>{{RequestNo}}</strong> has been updated to <strong>{{Status}}</strong> on {{UpdatedDate}}.</p>",
                EmailRecipientType.Bank),
            ("BANK_STATUS_APPROVED_CONFIRMATION", "Bank: Approval Confirmation", "Loan Approval Confirmed - {{RequestNo}}",
                "<p>The approval for request <strong>{{RequestNo}}</strong> has been recorded. The applicant has been notified.</p>",
                EmailRecipientType.Bank),
            ("BANK_STATUS_REJECTED_CONFIRMATION", "Bank: Rejection Confirmation", "Loan Rejection Confirmed - {{RequestNo}}",
                "<p>The rejection for request <strong>{{RequestNo}}</strong> has been recorded. The applicant has been notified with reason: {{Remarks}}</p>",
                EmailRecipientType.Bank),
        };

        foreach (var (code, name, subject, body, recipientType) in templates)
        {
            if (!db.EmailTemplates.Any(t => t.TemplateCode == code))
            {
                db.EmailTemplates.Add(new EmailTemplate
                {
                    TemplateCode = code,
                    TemplateName = name,
                    Subject = subject,
                    BodyHtml = body,
                    RecipientType = recipientType,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedSystemSettingsAsync(ApplicationDbContext db)
    {
        var defaults = new (string Key, string? Value, SettingCategory Cat, bool Encrypted)[]
        {
            ("Smtp.Enabled", "false", SettingCategory.Email, false),
            ("Smtp.Host", "", SettingCategory.Email, false),
            ("Smtp.Port", "587", SettingCategory.Email, false),
            ("Smtp.Username", "", SettingCategory.Email, false),
            ("Smtp.Password", "", SettingCategory.Email, true),
            ("Smtp.FromEmail", "no-reply@smelevate.local", SettingCategory.Email, false),
            ("Smtp.FromName", "SMElevate Portal", SettingCategory.Email, false),
            ("Smtp.EnableSsl", "true", SettingCategory.Email, false),
            ("Notification.EmailEnabled", "false", SettingCategory.Notification, false),
            ("Notification.SmsEnabled", "false", SettingCategory.Notification, false),
            ("Notification.PortalEnabled", "true", SettingCategory.Notification, false),
            ("OAuth.GoogleClientId", "", SettingCategory.OAuth, false),
            ("OAuth.GoogleClientSecret", "", SettingCategory.OAuth, true),
            ("OAuth.AppleClientId", "", SettingCategory.OAuth, false),
            ("OAuth.AppleClientSecret", "", SettingCategory.OAuth, true),
            ("OAuth.MicrosoftClientId", "", SettingCategory.OAuth, false),
            ("OAuth.MicrosoftClientSecret", "", SettingCategory.OAuth, true),
            ("General.PortalName", "SMElevate", SettingCategory.General, false),
            ("General.SupportEmail", "support@smelevate.local", SettingCategory.General, false),
            ("General.SupportPhone", "", SettingCategory.General, false),
            ("General.MaintenanceMode", "false", SettingCategory.General, false),
            ("General.SessionTimeoutMinutes", "480", SettingCategory.General, false),
        };

        foreach (var (key, value, cat, encrypted) in defaults)
        {
            if (!db.SystemSettings.Any(s => s.SettingKey == key))
            {
                db.SystemSettings.Add(new SystemSetting
                {
                    SettingKey = key,
                    SettingValue = value,
                    SettingCategory = cat,
                    IsEncrypted = encrypted,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedNewLookupValuesAsync(ApplicationDbContext db)
    {
        // Add new STATUS values (v2.2 lifecycle — skip if already present)
        var statusLookup = db.MasterLookups.FirstOrDefault(l => l.LookupCode == LookupCodes.Status);
        if (statusLookup != null)
        {
            var newStatuses = new[]
            {
                AppStatus.Draft, AppStatus.ValidationPending, AppStatus.ValidationFailed,
                AppStatus.Validated, AppStatus.ReferredToBank, AppStatus.UnderCreditBureauCheck,
                AppStatus.UnderRiskAssessment, AppStatus.UnderCDDComplianceReview,
                AppStatus.AdditionalInformationRequired, AppStatus.UnderBankDecision,
                AppStatus.ConditionallyApproved, AppStatus.Declined,
                AppStatus.OfferIssued, AppStatus.OfferAccepted, AppStatus.OfferRejected, AppStatus.OfferExpired,
                AppStatus.PostApprovalFormalities, AppStatus.DocumentsPending, AppStatus.DocumentsCompleted,
                AppStatus.ReadyForDisbursement, AppStatus.Disbursed, AppStatus.UnderMonitoring,
                AppStatus.Closed, AppStatus.Withdrawn
            };
            var existingCodes = db.MasterLookupValues
                .Where(v => v.MasterLookupId == statusLookup.Id)
                .Select(v => v.ValueText).ToHashSet();

            var order = db.MasterLookupValues.Where(v => v.MasterLookupId == statusLookup.Id).Count();
            foreach (var status in newStatuses.Where(s => !existingCodes.Contains(s)))
            {
                db.MasterLookupValues.Add(new MasterLookupValue
                {
                    MasterLookupId = statusLookup.Id,
                    ValueText = status,
                    ValueCode = status.Replace(" ", "_").Replace("/", "_").Replace("-", "_").ToUpper(),
                    DisplayOrder = order++,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        // Add "Agri-SME" to BusinessNature if missing
        var natureL = db.MasterLookups.FirstOrDefault(l => l.LookupCode == LookupCodes.BusinessNature);
        if (natureL != null && !db.MasterLookupValues.Any(v => v.MasterLookupId == natureL.Id && v.ValueText == "Agri-SME"))
        {
            var order = db.MasterLookupValues.Where(v => v.MasterLookupId == natureL.Id).Count();
            db.MasterLookupValues.Add(new MasterLookupValue { MasterLookupId = natureL.Id, ValueText = "Agri-SME", ValueCode = "AGRI_SME", DisplayOrder = order, IsActive = true, CreatedAt = DateTime.UtcNow });
        }

        // Seed RISK_CATEGORY lookup if missing
        if (!db.MasterLookups.Any(l => l.LookupCode == LookupCodes.RiskCategory))
        {
            var riskL = new MasterLookup { LookupName = "Risk Category", LookupCode = LookupCodes.RiskCategory, Description = "Risk assessment categories", IsActive = true, CreatedAt = DateTime.UtcNow };
            db.MasterLookups.Add(riskL);
            await db.SaveChangesAsync();
            var vals = new[] { "Low", "Medium", "High", "Very High" };
            for (int i = 0; i < vals.Length; i++)
                db.MasterLookupValues.Add(new MasterLookupValue { MasterLookupId = riskL.Id, ValueText = vals[i], ValueCode = vals[i].Replace(" ", "_").ToUpper(), DisplayOrder = i, IsActive = true, CreatedAt = DateTime.UtcNow });
        }

        // Seed COMPLIANCE_STATUS lookup if missing
        if (!db.MasterLookups.Any(l => l.LookupCode == LookupCodes.ComplianceStatus))
        {
            var compL = new MasterLookup { LookupName = "Compliance Status", LookupCode = LookupCodes.ComplianceStatus, Description = "CDD/KYC/AML compliance statuses", IsActive = true, CreatedAt = DateTime.UtcNow };
            db.MasterLookups.Add(compL);
            await db.SaveChangesAsync();
            var vals = new[] { "Pending", "Passed", "Failed", "Waived", "UnderReview" };
            for (int i = 0; i < vals.Length; i++)
                db.MasterLookupValues.Add(new MasterLookupValue { MasterLookupId = compL.Id, ValueText = vals[i], ValueCode = vals[i].ToUpper(), DisplayOrder = i, IsActive = true, CreatedAt = DateTime.UtcNow });
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedWorkflowStatusesAsync(ApplicationDbContext db)
    {
        var statuses = new[]
        {
            (AppStatus.Draft, "Draft", true, false, "EndUser", "badge-secondary", 1),
            (AppStatus.Submitted, "Submitted", false, false, "EndUser,System", "badge-primary", 2),
            (AppStatus.ValidationPending, "Validation Pending", false, false, "Admin,System", "badge-warning", 3),
            (AppStatus.ValidationFailed, "Validation Failed", false, false, "Admin", "badge-danger", 4),
            (AppStatus.Validated, "Validated", false, false, "Admin", "badge-info", 5),
            (AppStatus.ReferredToBank, "Referred to Bank", false, false, "Admin", "badge-cyan", 6),
            (AppStatus.UnderCreditBureauCheck, "Under Credit Bureau Check", false, false, "Bank", "badge-azure", 7),
            (AppStatus.UnderRiskAssessment, "Under Risk Assessment", false, false, "Bank", "badge-azure", 8),
            (AppStatus.UnderCDDComplianceReview, "Under CDD/Compliance Review", false, false, "Bank", "badge-azure", 9),
            (AppStatus.AdditionalInformationRequired, "Additional Information Required", false, false, "Bank", "badge-warning", 10),
            (AppStatus.UnderBankDecision, "Under Bank Decision", false, false, "Bank", "badge-purple", 11),
            (AppStatus.ConditionallyApproved, "Conditionally Approved", false, false, "Bank", "badge-teal", 12),
            (AppStatus.Declined, "Declined", false, false, "Bank", "badge-danger", 13),
            (AppStatus.OfferIssued, "Offer Issued", false, false, "Bank", "badge-indigo", 14),
            (AppStatus.OfferAccepted, "Offer Accepted", false, false, "EndUser", "badge-success", 15),
            (AppStatus.OfferRejected, "Offer Rejected", false, false, "EndUser", "badge-danger", 16),
            (AppStatus.OfferExpired, "Offer Expired", false, false, "System", "badge-secondary", 17),
            (AppStatus.PostApprovalFormalities, "Post-Approval Formalities", false, false, "Bank", "badge-lime", 18),
            (AppStatus.DocumentsPending, "Documents Pending", false, false, "Bank,EndUser", "badge-yellow", 19),
            (AppStatus.DocumentsCompleted, "Documents Completed", false, false, "Bank", "badge-green", 20),
            (AppStatus.ReadyForDisbursement, "Ready for Disbursement", false, false, "Bank", "badge-teal", 21),
            (AppStatus.Disbursed, "Disbursed", false, false, "Bank", "badge-success", 22),
            (AppStatus.UnderMonitoring, "Under Monitoring", false, false, "Bank", "badge-blue", 23),
            (AppStatus.Completed, "Completed", false, true, "Bank,Admin", "badge-success", 24),
            (AppStatus.Closed, "Closed", false, true, "Admin,Bank", "badge-secondary", 25),
            (AppStatus.Withdrawn, "Withdrawn", false, true, "EndUser,Admin", "badge-secondary", 26),
        };

        foreach (var (code, name, isInitial, isFinal, actors, color, order) in statuses)
        {
            if (!db.WorkflowStatuses.Any(w => w.StatusCode == code))
            {
                db.WorkflowStatuses.Add(new WorkflowStatus
                {
                    StatusCode = code, StatusName = name, IsInitial = isInitial, IsFinal = isFinal,
                    AllowedActorTypes = actors, ColorClass = color, DisplayOrder = order, IsActive = true
                });
            }
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedWorkflowTransitionsAsync(ApplicationDbContext db)
    {
        var transitions = new[]
        {
            // (FromCode, ToCode, Actors, ActionLabel, RequiresRemarks, RequiresReasonCode, Order)
            (AppStatus.Draft, AppStatus.Submitted, "EndUser", "Submit Application", false, false, 1),
            (AppStatus.Draft, AppStatus.Withdrawn, "EndUser", "Withdraw Draft", false, false, 2),
            (AppStatus.Submitted, AppStatus.ValidationPending, "Admin,System", "Start Validation", false, false, 3),
            (AppStatus.Submitted, AppStatus.Withdrawn, "EndUser", "Withdraw", true, false, 4),
            (AppStatus.ValidationPending, AppStatus.Validated, "Admin", "Mark Validated", false, false, 5),
            (AppStatus.ValidationPending, AppStatus.ValidationFailed, "Admin", "Fail Validation", true, false, 6),
            (AppStatus.ValidationFailed, AppStatus.Submitted, "EndUser", "Resubmit", false, false, 7),
            (AppStatus.ValidationFailed, AppStatus.Withdrawn, "EndUser", "Withdraw", true, false, 8),
            (AppStatus.Validated, AppStatus.ReferredToBank, "Admin", "Refer to Bank", false, false, 9),
            (AppStatus.ReferredToBank, AppStatus.UnderCreditBureauCheck, "Bank", "Start Credit Bureau Check", false, false, 10),
            (AppStatus.UnderCreditBureauCheck, AppStatus.UnderRiskAssessment, "Bank", "Proceed to Risk Assessment", false, false, 11),
            (AppStatus.UnderCreditBureauCheck, AppStatus.AdditionalInformationRequired, "Bank", "Request Information", true, false, 12),
            (AppStatus.UnderRiskAssessment, AppStatus.UnderCDDComplianceReview, "Bank", "Proceed to CDD Review", false, false, 13),
            (AppStatus.UnderRiskAssessment, AppStatus.AdditionalInformationRequired, "Bank", "Request Information", true, false, 14),
            (AppStatus.UnderCDDComplianceReview, AppStatus.UnderBankDecision, "Bank", "Proceed to Decision", false, false, 15),
            (AppStatus.UnderCDDComplianceReview, AppStatus.AdditionalInformationRequired, "Bank", "Request Information", true, false, 16),
            (AppStatus.AdditionalInformationRequired, AppStatus.UnderCreditBureauCheck, "Bank", "Return to Credit Bureau", false, false, 17),
            (AppStatus.AdditionalInformationRequired, AppStatus.UnderRiskAssessment, "Bank", "Return to Risk Assessment", false, false, 18),
            (AppStatus.AdditionalInformationRequired, AppStatus.UnderCDDComplianceReview, "Bank", "Return to CDD Review", false, false, 19),
            (AppStatus.AdditionalInformationRequired, AppStatus.UnderBankDecision, "Bank", "Proceed to Decision", false, false, 20),
            (AppStatus.UnderBankDecision, AppStatus.ConditionallyApproved, "Bank", "Conditionally Approve", false, false, 21),
            (AppStatus.UnderBankDecision, AppStatus.Declined, "Bank", "Decline", true, true, 22),
            (AppStatus.ConditionallyApproved, AppStatus.OfferIssued, "Bank", "Issue Offer", false, false, 23),
            (AppStatus.OfferIssued, AppStatus.OfferAccepted, "EndUser", "Accept Offer", false, false, 24),
            (AppStatus.OfferIssued, AppStatus.OfferRejected, "EndUser", "Reject Offer", true, false, 25),
            (AppStatus.OfferIssued, AppStatus.OfferExpired, "System", "Expire Offer", false, false, 26),
            (AppStatus.OfferAccepted, AppStatus.PostApprovalFormalities, "Bank", "Start Post-Approval", false, false, 27),
            (AppStatus.PostApprovalFormalities, AppStatus.DocumentsPending, "Bank", "Mark Documents Pending", false, false, 28),
            (AppStatus.DocumentsPending, AppStatus.DocumentsCompleted, "Bank", "Mark Documents Completed", false, false, 29),
            (AppStatus.DocumentsCompleted, AppStatus.ReadyForDisbursement, "Bank", "Mark Ready for Disbursement", false, false, 30),
            (AppStatus.ReadyForDisbursement, AppStatus.Disbursed, "Bank", "Record Disbursement", false, false, 31),
            (AppStatus.Disbursed, AppStatus.UnderMonitoring, "Bank", "Start Monitoring", false, false, 32),
            (AppStatus.UnderMonitoring, AppStatus.Completed, "Bank,Admin", "Mark Completed", false, false, 33),
            (AppStatus.Completed, AppStatus.Closed, "Admin,Bank", "Close", false, false, 34),
            (AppStatus.Declined, AppStatus.Closed, "Admin", "Close", false, false, 35),
            (AppStatus.OfferRejected, AppStatus.Closed, "Admin", "Close", false, false, 36),
            (AppStatus.OfferExpired, AppStatus.Closed, "Admin", "Close", false, false, 37),
            (AppStatus.Withdrawn, AppStatus.Closed, "Admin", "Close", false, false, 38),
        };

        foreach (var (from, to, actors, label, reqRemarks, reqReason, order) in transitions)
        {
            if (!db.WorkflowTransitions.Any(t => t.FromStatusCode == from && t.ToStatusCode == to))
            {
                db.WorkflowTransitions.Add(new WorkflowTransition
                {
                    FromStatusCode = from, ToStatusCode = to, AllowedActorTypes = actors,
                    ActionLabel = label, RequiresRemarks = reqRemarks, RequiresReasonCode = reqReason,
                    IsActive = true, DisplayOrder = order
                });
            }
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedDeclineReasonCodesAsync(ApplicationDbContext db)
    {
        if (db.DeclineReasonCodes.Any()) return;

        var codes = new[]
        {
            ("DRC-001", "Insufficient Credit History", "Credit", 1),
            ("DRC-002", "Low Credit Score", "Credit", 2),
            ("DRC-003", "Non-Performing Loan (NPL) on Record", "Credit", 3),
            ("DRC-004", "High Debt-to-Income Ratio", "Credit", 4),
            ("DRC-005", "Insufficient Collateral", "Credit", 5),
            ("DRC-006", "Business Not Viable", "Credit", 6),
            ("DRC-007", "AML / KYC Non-Compliance", "Compliance", 7),
            ("DRC-008", "Sanctions Screening Hit", "Compliance", 8),
            ("DRC-009", "Politically Exposed Person (PEP) Related", "Compliance", 9),
            ("DRC-010", "Fraudulent or Misrepresented Documentation", "Compliance", 10),
            ("DRC-011", "Incomplete Documentation", "Documentation", 11),
            ("DRC-012", "Insufficient Business History", "Documentation", 12),
            ("DRC-013", "Business Eligibility Criteria Not Met", "Other", 13),
            ("DRC-014", "Scheme Eligibility Criteria Not Met", "Other", 14),
            ("DRC-015", "Applicant Requested Withdrawal", "Other", 15),
        };

        foreach (var (code, desc, cat, order) in codes)
            db.DeclineReasonCodes.Add(new DeclineReasonCode { Code = code, Description = desc, Category = cat, DisplayOrder = order, IsActive = true });

        await db.SaveChangesAsync();
    }

    private static async Task SeedNewEmailTemplatesAsync(ApplicationDbContext db)
    {
        var templates = new[]
        {
            ("ENDUSER_CASE_ID_GENERATED", "EndUser: Case ID Generated", "Your Application Case ID: {{CaseId}}",
                "<p>Dear {{FullName}},</p><p>Your SME loan application has been assigned Case ID: <strong>{{CaseId}}</strong>. Use this ID to track your application.</p><p>Track at: {{PortalUrl}}</p>",
                EmailRecipientType.EndUser),
            ("ENDUSER_REFERRED_TO_BANK", "EndUser: Referred to Bank", "Your Application {{CaseId}} Has Been Referred",
                "<p>Dear {{FullName}},</p><p>Your application <strong>{{CaseId}}</strong> has been referred to <strong>{{BankName}}</strong> for assessment.</p><p>Track at: {{PortalUrl}}</p>",
                EmailRecipientType.EndUser),
            ("ENDUSER_ADDITIONAL_INFO_REQUESTED", "EndUser: Additional Information Requested", "Action Required: Information Requested for {{CaseId}}",
                "<p>Dear {{FullName}},</p><p><strong>{{BankName}}</strong> has requested additional information for your application <strong>{{CaseId}}</strong>.</p><p>Request Title: <strong>{{RequestTitle}}</strong></p><p>Due Date: {{DueDate}}</p><p>Please log in to respond: {{PortalUrl}}</p>",
                EmailRecipientType.EndUser),
            ("ENDUSER_OFFER_ISSUED", "EndUser: Conditional Offer Issued", "Conditional Offer Available for {{CaseId}}",
                "<p>Dear {{FullName}},</p><p>A conditional offer has been issued for your application <strong>{{CaseId}}</strong>.</p><p>Offer Number: <strong>{{OfferNumber}}</strong><br>Expiry Date: {{ExpiryDate}}</p><p>Log in to review and respond: {{PortalUrl}}</p>",
                EmailRecipientType.EndUser),
            ("ENDUSER_OFFER_EXPIRED", "EndUser: Offer Expired", "Offer {{OfferNumber}} Has Expired",
                "<p>Dear {{FullName}},</p><p>The conditional offer <strong>{{OfferNumber}}</strong> for application <strong>{{CaseId}}</strong> has expired without a response.</p>",
                EmailRecipientType.EndUser),
            ("ENDUSER_APPLICATION_DECLINED", "EndUser: Application Declined", "SME Application {{CaseId}} - Decision",
                "<p>Dear {{FullName}},</p><p>We regret to inform you that your application <strong>{{CaseId}}</strong> has been declined by <strong>{{BankName}}</strong>.</p><p>Reason: {{DeclineReason}}</p><p>You may contact us for further guidance.</p>",
                EmailRecipientType.EndUser),
            ("ENDUSER_DISBURSED", "EndUser: Disbursement Recorded", "Disbursement Recorded for {{CaseId}}",
                "<p>Dear {{FullName}},</p><p>Disbursement has been recorded for your application <strong>{{CaseId}}</strong>.</p><p>Amount Disbursed: <strong>{{DisbursedAmount}}</strong><br>Value Date: {{ValueDate}}</p>",
                EmailRecipientType.EndUser),
            ("ENDUSER_APPLICATION_COMPLETED", "EndUser: Application Completed", "Application {{CaseId}} - Completed",
                "<p>Dear {{FullName}},</p><p>Your SME loan application <strong>{{CaseId}}</strong> has been marked as completed.</p><p>Thank you for using SMElevate.</p>",
                EmailRecipientType.EndUser),
            ("BANK_ADDITIONAL_INFO_RESPONDED", "Bank: Applicant Responded to Info Request", "Response Received for {{CaseId}} - {{RequestTitle}}",
                "<p>The applicant has responded to the information request for application <strong>{{CaseId}}</strong>.</p><p>Request: <strong>{{RequestTitle}}</strong><br>Response Date: {{ResponseDate}}</p><p>Login to review: {{PortalUrl}}</p>",
                EmailRecipientType.Bank),
        };

        foreach (var (code, name, subject, body, recipientType) in templates)
        {
            if (!db.EmailTemplates.Any(t => t.TemplateCode == code))
            {
                db.EmailTemplates.Add(new EmailTemplate
                {
                    TemplateCode = code, TemplateName = name, Subject = subject, BodyHtml = body,
                    RecipientType = recipientType, IsActive = true, CreatedAt = DateTime.UtcNow
                });
            }
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedDemoBanksAndUsersAsync(ApplicationDbContext db)
    {
        var hasher = new PasswordHasher<ApplicationUser>();
        var bankRole = db.Roles.FirstOrDefault(r => r.RoleName == "Bank");
        var endUserRole = db.Roles.FirstOrDefault(r => r.RoleName == "EndUser");

        // Seed demo banks
        if (!db.Banks.Any(b => b.BankName == "ABC Bank Limited"))
        {
            db.Banks.Add(new Bank { BankName = "ABC Bank Limited", IBANPrefix = "ABCD", BankCode = "ABC001", BankEmailAddress = "bank.user@example.com", IsActive = true, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }
        if (!db.Banks.Any(b => b.BankName == "Meezan Bank Limited"))
        {
            db.Banks.Add(new Bank { BankName = "Meezan Bank Limited", IBANPrefix = "MEZN", BankCode = "MEZ001", BankEmailAddress = "sara.bank@example.com", IsActive = true, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        var abcBank = db.Banks.First(b => b.BankName == "ABC Bank Limited");
        var meezanBank = db.Banks.First(b => b.BankName == "Meezan Bank Limited");

        // Seed bank users
        if (!db.Users.Any(u => u.EmailAddress == "bank.user@example.com"))
        {
            var bankUser = new ApplicationUser { FullName = "Bank Officer", EmailAddress = "bank.user@example.com", MobileNo = "03001234567", UserType = UserType.Bank, RoleId = bankRole?.Id, BankId = abcBank.Id, IsActive = true, IsEmailVerified = true, CreatedAt = DateTime.UtcNow };
            bankUser.PasswordHash = hasher.HashPassword(bankUser, "Bank@12345");
            db.Users.Add(bankUser);
            await db.SaveChangesAsync();
            db.BankMembers.Add(new BankMember { BankId = abcBank.Id, UserId = bankUser.Id, IsActive = true, AssignedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        if (!db.Users.Any(u => u.EmailAddress == "sara.bank@example.com"))
        {
            var sara = new ApplicationUser { FullName = "Sara Ahmed", EmailAddress = "sara.bank@example.com", MobileNo = "03007654321", UserType = UserType.Bank, RoleId = bankRole?.Id, BankId = meezanBank.Id, IsActive = true, IsEmailVerified = true, CreatedAt = DateTime.UtcNow };
            sara.PasswordHash = hasher.HashPassword(sara, "Bank@12345");
            db.Users.Add(sara);
            await db.SaveChangesAsync();
            db.BankMembers.Add(new BankMember { BankId = meezanBank.Id, UserId = sara.Id, IsActive = true, AssignedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        // Seed demo EndUser
        if (!db.Users.Any(u => u.EmailAddress == "enduser@example.com"))
        {
            var endUser = new ApplicationUser { FullName = "Demo EndUser", EmailAddress = "enduser@example.com", MobileNo = "03009998888", UserType = UserType.SME, RoleId = endUserRole?.Id, IsActive = true, IsEmailVerified = true, CreatedAt = DateTime.UtcNow };
            endUser.PasswordHash = hasher.HashPassword(endUser, "EndUser@12345");
            db.Users.Add(endUser);
            await db.SaveChangesAsync();
            db.EndUserProfiles.Add(new EndUserProfile { UserId = endUser.Id, FirstName = "Demo", LastName = "EndUser", MobileNo = "03009998888", CNIC = "35202-9999999-9", BusinessEmailAddress = "enduser@example.com", GenderOfProprietor = "Male", IsVerified = true, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedBusinessProfileEmailTemplatesAsync(ApplicationDbContext db)
    {
        var templates = new[]
        {
            ("BP_EMAIL_OTP", "Business Profile: Email OTP",
                "Verify Your Business Email - OTP: {{OtpCode}}",
                "<p>Dear Business Owner,</p><p>Your OTP for verifying the business email of <strong>{{BusinessName}}</strong> is:</p><h2 style='letter-spacing:8px'>{{OtpCode}}</h2><p>This OTP expires in <strong>{{ExpiryMinutes}} minutes</strong>. Do not share this code with anyone.</p>",
                EmailRecipientType.EndUser),
            ("BP_EMAIL_OTP_RESEND", "Business Profile: Email OTP Resend",
                "Your Business Email Verification OTP (Resent)",
                "<p>Dear Business Owner,</p><p>A new OTP has been generated for verifying the business email of <strong>{{BusinessName}}</strong>:</p><h2 style='letter-spacing:8px'>{{OtpCode}}</h2><p>This OTP expires in <strong>{{ExpiryMinutes}} minutes</strong>. Do not share this code with anyone.</p>",
                EmailRecipientType.EndUser),
            ("BP_MOBILE_OTP", "Business Profile: Mobile OTP",
                "Business Mobile Verification OTP",
                "<p>Your SMElevate business mobile verification OTP for <strong>{{BusinessName}}</strong> is <strong>{{OtpCode}}</strong>. Valid for {{ExpiryMinutes}} minutes. Do not share.</p>",
                EmailRecipientType.EndUser),
            ("BP_MOBILE_OTP_RESEND", "Business Profile: Mobile OTP Resend",
                "Business Mobile Verification OTP (Resent)",
                "<p>Your new SMElevate business mobile verification OTP for <strong>{{BusinessName}}</strong> is <strong>{{OtpCode}}</strong>. Valid for {{ExpiryMinutes}} minutes. Do not share.</p>",
                EmailRecipientType.EndUser),
            ("BP_VERIFIED", "Business Profile: Fully Verified",
                "Your Business Profile is Now Verified",
                "<p>Congratulations!</p><p>Your business profile <strong>{{BusinessName}}</strong> has been fully verified. You can now use it when submitting loan applications on SMElevate.</p><p>Log in at: {{PortalUrl}}</p>",
                EmailRecipientType.EndUser),
        };

        foreach (var (code, name, subject, body, recipientType) in templates)
        {
            if (!db.EmailTemplates.Any(t => t.TemplateCode == code))
            {
                db.EmailTemplates.Add(new EmailTemplate
                {
                    TemplateCode = code, TemplateName = name, Subject = subject, BodyHtml = body,
                    RecipientType = recipientType, IsActive = true, CreatedAt = DateTime.UtcNow
                });
            }
        }
        await db.SaveChangesAsync();
    }
}
