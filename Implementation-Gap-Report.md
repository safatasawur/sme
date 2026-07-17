# Implementation Gap Report
**Project:** SMElevate V2 — SBP SME Financing Portal  
**Spec Version:** 2.2  
**Report Date:** 2026-07-13  
**Author:** Implementation Analysis  

---

## 1. Executive Summary

The existing SMElevate V2 project provides a solid foundation: custom cookie authentication across three Areas (Admin, Banks, EndUser), a dynamic form builder, a basic 7-status loan request lifecycle, EF Core 10 Code-First on SQL Server, MailKit email, in-app notifications, and the Tabler/Bootstrap 5 UI theme.

The v2.2 specification requires a substantial expansion: 26 configurable lifecycle statuses (vs. 7 today), 12 new database entities, 3 new service modules, complete Bank assessment and decision modules, Conditional Offer management, Post-Approval Formalities, Disbursement Tracking, 7 reports, enhanced dashboards, and External Integration interfaces.

**Nothing in the existing codebase is discarded.** All existing controllers, views, models, and migrations are preserved and extended.

---

## 2. Existing Functionality (What Works Today)

| Module | Status |
|---|---|
| Admin authentication (email + password) | ✅ Complete |
| Bank authentication (email + OTP via email) | ✅ Complete |
| EndUser registration + email/OAuth login | ✅ Complete |
| EndUser profile completion and email verification | ✅ Complete |
| Admin: Users, Roles, Banks, Schemes, Form Builder, Lookups, Settings | ✅ Complete |
| Bank: Assigned applications, approve/reject decision, file upload | ✅ Partial |
| EndUser: Submit loan application, My Applications, Notifications | ✅ Partial |
| Dynamic form builder (SchemeFormFieldConfiguration) | ✅ Complete |
| In-app notifications + email from templates | ✅ Complete |
| Audit logging (partial) | ✅ Partial |
| EF Core migrations (7 applied) | ✅ Complete |

---

## 3. Gap Analysis by Spec Section

### §3 — Terminology
| Gap | Impact |
|---|---|
| UI uses "Loan Request"; spec requires "Loan Application" in all user-facing text | Low — label changes only; internal names preserved |

### §5 — Standardized Loan Application Form
| Gap | Impact |
|---|---|
| `MasterFormTemplate` has 19 fields — covers most required fields | Minor |
| No form versioning (Draft/Published/Archived) at `SchemeForm` level | Medium |
| No `EffectiveDate`, `VersionNumber` on `SchemeForm` | Medium |
| Published forms are not immutable (no guard on re-edit) | Medium |

### §6 — IBAN, RAAST ID, and Consent
| Gap | Impact |
|---|---|
| `LoanRequest` has `IBANOrRaastType` + `IBANOrRaastValue` only — no expanded section | Medium |
| No `IdentifiedBank`, `AccountValidationStatus`, `ValidationMessage` | Medium |
| No `ApplicantConsent` record (checkbox, date, IP, version) | High |
| No RAAST/IBAN validation interface | Low (interface only) |

### §7 — Case ID
| Gap | Impact |
|---|---|
| `RequestNo` uses `SME-YYYYMMDD-NNNNNN` format; spec requires `SME-YYYY-NNNNNN` | Medium |
| No `CaseId` as separate immutable field; `RequestNo` is mutable in concept | High |
| `CaseId` must be generated only on first submission (not on create) | High |

### §8 — Application Lifecycle (26 statuses)
| Gap | Impact |
|---|---|
| Current: 7 statuses in `MasterLookupValues` (Submitted/Assigned/InProcess/Approved/Rejected/MoreInfo/Completed) | **Critical** |
| Required: 26 statuses per spec | **Critical** |
| No `WorkflowStatus` / `WorkflowTransition` tables (configurable transitions) | High |
| `LoanRequestStatusHistory` missing: `ActorType`, `ReasonCode`, `IpAddress` columns | High |
| No transition validation in service layer | High |
| `LoanRequest` has no `Draft` state support (no `IsDraft` or Draft status) | High |

### §9 — Bank Assessment Tracking
| Gap | Impact |
|---|---|
| No `BankAssessment` entity (Credit Bureau Check, Risk Assessment, CDD/KYC/AML) | **Critical** |
| No bank-side assessment UI in Banks area | **Critical** |

### §10 — Additional Information Requests
| Gap | Impact |
|---|---|
| Current bank decision supports only "More Information Required" status with remarks | Partial |
| No `AdditionalInformationRequest` entity with title, description, required docs, due date, response | High |
| EndUser cannot respond to information requests | High |

### §11 — Bank Decision
| Gap | Impact |
|---|---|
| `BankRequestsController.SubmitDecision` handles Approved/Rejected — covers partial case | Partial |
| No `BankDecision` entity with: `DeclineReasonCodeId`, `ApprovedFacilityType`, `ApprovedAmount`, `ApprovedTenor`, `AdditionalConditions` | High |
| No `ConditionallyApproved` decision type | High |
| No `DeclineReasonCode` lookup table or seeded codes | High |

### §12 — Conditional Offer Management
| Gap | Impact |
|---|---|
| No `ConditionalOffer` entity | **Critical** |
| No `ConditionalOfferResponse` entity | **Critical** |
| No EndUser offer view/accept/reject UI | **Critical** |
| No offer versioning or expiry logic | **Critical** |

### §13 — Post-Approval Formalities
| Gap | Impact |
|---|---|
| No `PostApprovalChecklist` or `PostApprovalChecklistItem` entity | **Critical** |
| No bank-side checklist management UI | **Critical** |
| No EndUser document upload for checklist items | **Critical** |

### §14 — Disbursement Tracking
| Gap | Impact |
|---|---|
| No `Disbursement` entity | **Critical** |
| No bank-side disbursement recording UI | **Critical** |

### §15 — Monitoring
| Gap | Impact |
|---|---|
| No `ApplicationMonitoring` entity | Medium |
| Post-disbursement statuses not seeded | Medium |

### §16 — Notifications
| Gap | Impact |
|---|---|
| Existing: Registration/verification, submission, bank decision email/in-app | Partial |
| Missing: Case ID generated, referral, assessment update, decline, offer events, post-approval, disbursement, completed | High |
| No retry mechanism for failed notifications | Medium |
| SMS/WhatsApp — configurable interface only required | Low |

### §17 — Dashboards
| Gap | Impact |
|---|---|
| Admin dashboard uses EF aggregates — partially dynamic | Partial |
| Admin dashboard missing: Offers Issued/Accepted, Disbursed, Approval Rate, Conversion Rate, ATT, Geographic | High |
| Bank dashboard missing: New Referrals, Under Review, Info Pending, Offers, Ready for Disbursement, ATT | High |
| EndUser dashboard missing: Draft count, Active Offers, Pending Actions, Disbursed | High |

### §18 — Reports
| Gap | Impact |
|---|---|
| No reporting module exists | **Critical** |
| Required: 7 report types with date filter and export | **Critical** |

### §19 — Audit Logging
| Gap | Impact |
|---|---|
| `AuditLog` entity exists; `IAuditService` logs some actions | Partial |
| Missing: logout, bank assignment, assessment updates, offer events, disbursement, document events | Medium |

### §20 — Authorization and Data Isolation
| Gap | Impact |
|---|---|
| EndUser ownership check on `Detail` — present | ✅ |
| Bank isolation on `Assigned/Approved/Rejected` — present | ✅ |
| New entities need ownership checks added | Medium |

### §23 — External Integrations
| Gap | Impact |
|---|---|
| No interfaces for RAAST, IBAN, Credit Bureau, NADRA, AML, SMS, WhatsApp, etc. | Low (interfaces only) |

### §24 — Security
| Gap | Impact |
|---|---|
| No account lockout on repeated failed logins | Medium |
| No OTP attempt limit | Medium |
| CSRF protection present via `[ValidateAntiForgeryToken]` | ✅ |
| File type/size validation present in `IFileUploadService` | ✅ |

---

## 4. Required New Database Entities

| Entity | Maps To Spec |
|---|---|
| `WorkflowStatus` | §8 — configurable status definitions |
| `WorkflowTransition` | §8 — allowed transitions |
| `DeclineReasonCode` | §11 — decline reason codes |
| `BankAssessment` | §9 — assessment stages per application |
| `AdditionalInformationRequest` | §10 — bank-to-applicant info requests |
| `BankDecision` | §11 — formal bank decision record |
| `ConditionalOffer` | §12 — conditional offer issued by bank |
| `ConditionalOfferResponse` | §12 — applicant accept/reject response |
| `PostApprovalChecklist` | §13 — checklist per application |
| `PostApprovalChecklistItem` | §13 — individual items |
| `ApplicationDocument` | §13/§4 — applicant document uploads |
| `Disbursement` | §14 — disbursement record |
| `ApplicationMonitoring` | §15 — post-disbursement monitoring |

---

## 5. Required Extensions to Existing Tables

| Table | Changes |
|---|---|
| `LoanRequests` | Add `CaseId` (VARCHAR 20, nullable, unique), `IsDraft` (bit), `ConsentGiven` (bit), `ConsentDate`, `ConsentIpAddress`, `ConsentVersion`, `IdentifiedBankId` (nullable FK), `AccountValidationStatus`, `AccountValidationMessage`, `PreferredIdentifierType` |
| `LoanRequestStatusHistory` | Add `ActorType` (VARCHAR 20), `ReasonCode` (nullable), `IpAddress` (nullable VARCHAR 50) |
| `SchemeForm` | Add `VersionNumber` (int, default 1), `EffectiveDate` (datetime), `Status` (Draft/Published/Archived) |

---

## 6. Required New Service Interfaces

| Interface | Purpose |
|---|---|
| `IWorkflowService` | Validate transitions, advance status, record history |
| `IBankAssessmentService` | CRUD for bank assessment stages |
| `IAdditionalInfoRequestService` | Create and respond to information requests |
| `IBankDecisionService` | Record formal bank decisions |
| `IConditionalOfferService` | Issue, track, and respond to conditional offers |
| `IPostApprovalService` | Checklist management and document verification |
| `IDisbursementService` | Record and update disbursement details |
| `IReportService` | Generate 7 report types with date filtering |
| `IExternalIntegrationService` | Stub interfaces for RAAST, IBAN, Credit Bureau, etc. |

---

## 7. Required New Controllers/Views

| Controller | Area | Views |
|---|---|---|
| `AdminLoanApplicationsController` | Admin | Index, Detail, Assign |
| `AdminReportsController` | Admin | Index, each report type |
| `AdminWorkflowController` | Admin | Statuses, Transitions, ReasonCodes |
| `AdminEmailTemplatesController` | Admin | Index, Edit |
| `BankAssessmentController` | Banks | Index (per application) |
| `BankDecisionController` | Banks | Index (per application) |
| `BankAdditionalInfoController` | Banks | Index, Create, View Response |
| `BankConditionalOfferController` | Banks | Create, View |
| `BankPostApprovalController` | Banks | Checklist, Verify |
| `BankDisbursementController` | Banks | Record, View |
| `EndUserAdditionalInfoController` | EndUser | Respond |
| `EndUserOfferController` | EndUser | View, Accept, Reject |
| `EndUserDocumentsController` | EndUser | Upload, List |

---

## 8. Technical Debt / Existing Issues

| Issue | Priority |
|---|---|
| `BankAuthController` and `AdminAuthController` have no Logout actions | High |
| `OtpService` stores OTPs in `IMemoryCache` — won't survive restarts | Medium |
| No account lockout on repeated failed logins | Medium |
| `ISchemeService.SaveFormAsync/PublishFormAsync` declared but not used (form builder uses `SchemeFormFieldConfiguration` path) | Low |
| `LoanRequest.RequestNo` format `SME-YYYYMMDD-NNNNNN` conflicts with spec `SME-YYYY-NNNNNN` for Case ID | Medium — resolve by adding separate `CaseId` |

---

## 9. Internal Name Retention Log

Per §3, the following internal names are retained to avoid breaking changes:

| Internal Name | User-Facing Label |
|---|---|
| `LoanRequest` (entity/service names) | "Loan Application" |
| `RequestNo` (DB column) | "Request No" (internal) |
| `CaseId` (new column) | "Case ID" (user-facing) |
| `AppStatus.*` constants | Will be extended; existing values kept |
| `LookupCodes.Status = "STATUS"` | Retained |

---

## 10. Reusable Components (No Changes Needed)

- `IAuthService`, `IBankService`, `IUserService`, `IRoleService` — unchanged
- `IEmailService`, `IEmailTemplateService` — reused for all new notification triggers
- `INotificationService`, `IOtpService` — unchanged
- `IAuditService`, `IFileUploadService`, `IRequestNumberService` — reused/extended
- All existing Razor layouts, CSS, JavaScript, Tabler theme — unchanged
- `DataSeeder` — extended (not replaced) for new seed data

---

## 11. Risk Register

| Risk | Mitigation |
|---|---|
| STATUS lookup expansion breaks existing status references | Add new values; existing IDs/ValueCodes preserved |
| Migration on live data with new NOT NULL columns | Use nullable with defaults; populate via migration |
| `LoanRequest.CaseId` unique constraint on nullable column | Filtered unique index (WHERE CaseId IS NOT NULL) |
| New entities introduce FK cycles on SQL Server | Use `OnDelete(DeleteBehavior.NoAction)` where needed |
| Workflow transition enforcement may break bank flow | Unit test all transitions before enforcing |

---

## 12. Implementation Sequence

| Phase | Description |
|---|---|
| **Phase 2** | New entities, extended columns, migration, seed 26 statuses + transitions + reason codes |
| **Phase 3** | `IWorkflowService`, lifecycle enforcement, extended `ILoanRequestService`, status history audit fields |
| **Phase 4** | EndUser: Draft/Submit flow, IBAN/RAAST expansion, consent, timeline, info responses, offers, docs |
| **Phase 5** | Bank: Assessment, info requests, decision, conditional offer, post-approval, disbursement |
| **Phase 6** | Admin: Loan application management, workflow config, reason codes, reports, email templates |
| **Phase 7** | Notifications for all new events, report exports, audit completion |
| **Phase 8** | Build verification, migration test, role-based workflow test, final reports |

---

*End of Implementation Gap Report*
