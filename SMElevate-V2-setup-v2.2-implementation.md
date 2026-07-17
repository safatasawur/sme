# SMElevate V2 — Implementation Setup Specification
**Version:** 2.2  
**Status:** Approved for Project Modification  
**Source:** Existing SMElevate V2 project and approved SME Financing BRD

## 1. Purpose
Modify the existing SMElevate V2 ASP.NET Core MVC project so that it aligns with the approved BRD for the Digital Portal for Financing to Business Enterprises.

This is an implementation specification. The project, database, controllers, services, ViewModels, Razor views, dashboards, workflows, reports, notifications, and migrations may be modified where required.

## 2. Mandatory Rules
1. Modify the existing SMElevate V2 project only; do not create a replacement project.
2. Preserve the existing frontend theme, layouts, CSS, JavaScript, navigation, and Areas structure.
3. Keep the existing Areas: `Admin`, `Banks`, and `EndUser`.
4. Reuse and extend existing entities, services, controllers, and views before creating duplicate modules.
5. Do not hard-code banks, schemes, application fields, workflow statuses, reports, or dashboard values.
6. Use database-driven configuration where applicable.
7. Use non-destructive Entity Framework Core migrations and preserve existing data.
8. Implement server-side validation, authorization, ownership checks, logging, audit trails, and error handling.
9. Build the solution after each phase and fix all compilation errors before continuing.
10. External integrations without available APIs or credentials must be implemented as configurable interfaces and clearly marked as pending production integration.

## 3. Terminology
Use **Loan Application** instead of **Loan Request** in all user-facing screens, menus, notifications, reports, and documentation.

Existing internal technical names may remain where renaming would introduce unnecessary risk. Record retained names in the implementation report.

## 4. User Roles
### End User / SME Applicant
The applicant can register, verify the account, complete a profile, create and save a draft application, submit an application, enter IBAN or RAAST ID, provide consent, upload documents, track status, respond to information requests, review and respond to offers, upload post-approval documents, and view disbursement status.

The applicant may access only their own profile and applications.

### Bank User
The bank user can view only applications assigned to their bank, record assessment stages, request information, approve or decline, issue conditional offers, review applicant responses and documents, manage post-approval checklists, record disbursement details, and view bank-specific dashboards and reports.

### Administrator / SBP User
The administrator can manage users, roles, permissions, banks, schemes, forms, workflow statuses, transitions, reason codes, notification templates, reports, audit logs, and system-wide dashboards.

## 5. Standardized Loan Application Form
Enhance the existing Dynamic Form Builder to support the SBP Standardized Loan Application Form.

Required capabilities:
- Scheme-based forms
- Draft, Published, and Archived versions
- Effective date and version number
- Configurable sections and fields
- Field and section order
- Required and optional settings
- Field types, help text, placeholders, and validation rules
- Conditional visibility
- Active and inactive settings
- Published versions must be immutable; changes require a new version

### Required Business Information
- Name of Business
- Business Address
- Contact Person
- Cell or Landline Number
- Business Email Address
- Annual Sales
- Number of Employees
- NTN, where applicable
- Year of Establishment
- Business Premises: Owned or Rented
- Is Business Registered: Yes or No
- Registration Authority, visible only when registered
- Business Status: Proprietorship, Partnership, or Private Limited Company
- Business Nature: Manufacturing, Services, Trading, or Agri-SME
- Product or Service Description
- Facility Type
- Requested Amount
- Requested Tenor

### Partner and Shareholder Details
Create a repeatable section containing:
- Full Name
- Contact Details
- CNIC
- Shareholding Percentage

The combined shareholding must not exceed 100 percent.

## 6. IBAN, RAAST ID, and Consent
Add a dedicated section containing:
- IBAN
- RAAST ID
- Preferred identifier type
- Identified bank
- Account validation status
- Validation message
- Applicant consent checkbox
- Consent date and time
- Consent IP address
- Consent version

At least one of IBAN or RAAST ID is required before submission. The bank should be identified through configurable mapping data. Where automated validation is unavailable, support manual validation and a future integration interface.

## 7. Case ID
Every first-time submitted application must receive a unique, immutable, searchable Case ID.

Recommended format: `SME-YYYY-NNNNNN`

Requirements:
- Generated only on first submission
- Unique database constraint
- Never reused
- Displayed in End User, Bank, and Admin portals
- Included in notifications and reports

## 8. Application Lifecycle
Implement the following configurable statuses:
1. Draft
2. Submitted
3. Validation Pending
4. Validation Failed
5. Validated
6. Referred to Bank
7. Under Credit Bureau Check
8. Under Risk Assessment
9. Under CDD/Compliance Review
10. Additional Information Required
11. Under Bank Decision
12. Conditionally Approved
13. Declined
14. Offer Issued
15. Offer Accepted
16. Offer Rejected
17. Offer Expired
18. Post-Approval Formalities
19. Documents Pending
20. Documents Completed
21. Ready for Disbursement
22. Disbursed
23. Under Monitoring
24. Completed
25. Closed
26. Withdrawn

### Workflow History
Store for each transition:
- Application ID
- Previous Status
- New Status
- Changed By
- Actor Type
- Date and Time
- Remarks
- Reason Code
- IP Address, where available

Invalid transitions must be blocked in the service layer. UI actions must be based on allowed transitions.

## 9. Bank Assessment Tracking
The portal records assessment stages but does not perform the bank's internal lending decision.

### Credit Bureau Check
- Status
- Check Date
- Reference Number
- Result Summary
- Remarks
- Attachment, where permitted

### Risk Assessment
- Status
- Scorecard Reference
- Result
- Risk Category
- Assessment Date
- Remarks

### CDD, KYC, AML, and Compliance
- CDD Status
- KYC Status
- AML Status
- Sanctions Screening Status
- PEP Screening Status
- Compliance Result
- Completion Date
- Remarks

Sensitive internal bank information must not be exposed to the applicant unless explicitly configured.

## 10. Additional Information Requests
Allow banks to request additional information using:
- Request Title
- Request Description
- Required Documents
- Due Date
- Status
- Created By and Created Date
- Applicant Response
- Response Date

The applicant must receive a notification and see the request in the application timeline.

## 11. Bank Decision
Allow authorized bank users to record:
- Approved, Conditionally Approved, or Declined
- Decision Date
- Decision Remarks
- Decline Reason Code
- Approved Facility Type
- Approved Amount
- Approved Tenor
- Additional Conditions

A decline must generate an immediate in-app and email notification. SMS and WhatsApp remain configurable channels.

## 12. Conditional Offer Management
Create a Conditional Offer module containing:
- Offer Number
- Application ID
- Offer Version
- Issue Date
- Expiry Date
- Facility Type
- Approved Amount
- Tenor
- Pricing or markup summary, where permitted
- Terms and Conditions
- Conditions Precedent
- Offer Letter Attachment
- Created By
- Status

The applicant can view, download, accept, or reject a valid offer. Record action, date and time, User ID, IP address, remarks, and offer version. Expired or superseded offers cannot be accepted.

## 13. Post-Approval Formalities
Create a configurable checklist module with:
- Checklist Item
- Description
- Required or Optional
- Document Required
- Due Date
- Applicant Submission
- Bank Verification Status
- Verified By
- Verification Date
- Remarks

Support multiple documents per checklist item where required.

## 14. Disbursement Tracking
The portal must not execute financial transactions. It records bank-provided disbursement details only:
- Application ID
- Disbursement Status
- Approved Amount
- Disbursed Amount
- Value Date
- Disbursement Account
- Bank Reference Number
- Remarks
- Updated By
- Updated Date

## 15. Monitoring
Provide informational post-disbursement statuses:
- Active
- Under Monitoring
- Completed
- Closed
- Settled, future
- Defaulted, future

## 16. Notifications
Use database-driven templates for:
- Registration and verification
- Application saved and submitted
- Case ID generated
- Referral to bank
- Additional information requested and received
- Assessment status updated
- Application declined
- Conditional offer issued, accepted, rejected, or expired
- Post-approval document requested or verified
- Ready for disbursement
- Disbursed
- Application completed

Channels:
- In-app
- Email
- SMS, configurable or future
- WhatsApp, configurable or future
- Push notification, configurable or future

Notification failure must not roll back the core transaction. Log the failure and support retry.

## 17. Dashboards
All values must be generated dynamically from the database.

### End User Dashboard
- Total Applications
- Draft Applications
- Submitted Applications
- Pending Applicant Actions
- Active Offers
- Disbursed Applications
- Recent Notifications
- Application Timeline

### Bank Dashboard
- Assigned Applications
- New Referrals
- Under Review
- Additional Information Pending
- Conditional Offers
- Approved
- Declined
- Ready for Disbursement
- Disbursed
- Average Turnaround Time

Only the logged-in bank's data may be displayed.

### Admin Dashboard
- Total Applicants
- Total Participating Banks
- Total Applications
- Submitted
- Referred
- Approved
- Declined
- Offers Issued
- Offers Accepted
- Disbursed
- Approval Rate
- Conversion Rate
- Average Turnaround Time
- Bank Comparison
- Geographic Distribution

## 18. Reports
Implement role-based, date-filtered reports with export where currently supported:
- Application Status Report
- Turnaround Time Report
- Assessment and Referral Report
- Decline Analysis
- Disbursement Summary
- Geographic Spread
- Bank Performance Report

## 19. Audit Logging
Audit:
- Login and logout
- Registration and verification
- Profile changes
- Application creation and submission
- Status changes
- Bank assignment
- Assessment updates
- Information requests and responses
- Offer creation and response
- Document upload and verification
- Disbursement updates
- User, role, permission, bank, scheme, form, workflow, and notification-template changes

Store user, role, action, entity, entity ID, old value, new value, timestamp, IP address, and result where applicable.

## 20. Authorization and Data Isolation
- End users may access only their own records.
- Bank users may access only records assigned to their Bank ID.
- Admin users receive system-wide access according to permission.
- Apply authorization in controllers and database queries.
- Do not rely on hidden buttons or menus as security controls.

## 21. Database Changes
Inspect the current schema before adding new entities. Reuse and extend existing tables where appropriate.

Likely entities include:
- LoanApplications
- LoanApplicationStatusHistory
- LoanApplicationPartners
- BankAccountIdentifiers
- ApplicantConsents
- BankAssessments
- AdditionalInformationRequests
- BankDecisions
- ConditionalOffers
- ConditionalOfferResponses
- PostApprovalChecklists
- PostApprovalChecklistItems
- ApplicationDocuments
- Disbursements
- ApplicationMonitoring
- NotificationTemplates
- Notifications
- WorkflowStatuses
- WorkflowTransitions
- DeclineReasonCodes
- AuditLogs

Add appropriate primary keys, foreign keys, unique constraints, indexes, string lengths, decimal precision, and timestamps.

## 22. Validation
Minimum rules:
- Unique email
- Unique CNIC
- Valid mobile and CNIC formats
- Valid IBAN format
- At least IBAN or RAAST ID
- Consent required before referral
- Requested amount and tenor greater than zero
- Shareholding total not greater than 100 percent
- Registration Authority required only when the business is registered
- Offer expiry later than issue date
- Disbursed amount cannot exceed approved amount unless authorized and documented
- All required published form fields completed before submission
- Invalid workflow transitions blocked

## 23. External Integrations
Create interfaces and configuration for:
- RAAST validation
- IBAN validation
- Credit Bureau
- NADRA or eKYC
- AML or KYC
- SMS Gateway
- WhatsApp Gateway
- Email Gateway
- Digital Signature
- Document Verification

Do not fake production integration. Use development-safe placeholder implementations when credentials are unavailable and document pending configuration.

## 24. Security
- Anti-forgery protection
- Secure production cookies
- HTTPS enforcement
- ASP.NET Core Identity password hashing
- Account lockout
- OTP expiry and attempt limits
- File type and size validation
- Malware-scanning integration point
- No sensitive data in logs
- Secrets through secure configuration providers
- Parameterized EF Core access
- Output encoding
- Query-level ownership protection against IDOR

## 25. Performance and Reliability
- Async database and I/O operations
- Pagination, filtering, and sorting on large lists
- Avoid N+1 queries
- Add indexes for common filters
- Transactions for submission, referral, offer response, and disbursement updates
- Retry handling for transient notification failures
- Preserve data consistency when integrations fail

## 26. Delivery Phases
### Phase 1 — Analysis and Gap Report
Inspect the solution and create `Implementation-Gap-Report.md` before major code changes.

### Phase 2 — Database and Domain
Add or extend entities, mappings, migrations, seed data, statuses, transitions, and reason codes.

### Phase 3 — Application and Workflow Services
Implement lifecycle, ownership, bank isolation, validation, status history, and audit logging.

### Phase 4 — End User Portal
Implement application submission, IBAN or RAAST, consent, timeline, information responses, offers, and post-approval documents.

### Phase 5 — Bank Portal
Implement assessment, information requests, decisions, offers, post-approval checklists, and disbursement tracking.

### Phase 6 — Admin Portal
Implement workflow configuration, reason codes, form versioning, reports, dashboards, and notification templates.

### Phase 7 — Notifications, Reports, and Audit
Complete notifications, reports, exports, and audit coverage.

### Phase 8 — Testing and Stabilization
Build, migrate, test all roles, verify isolation, fix defects, and produce reports.

## 27. Acceptance Criteria
1. Applicant registration, verification, profile completion, draft, and submission work.
2. Submission generates a unique Case ID.
3. IBAN or RAAST ID and consent are captured.
4. The application is assigned to the correct bank.
5. Bank users can access only their bank's applications.
6. Banks can record assessments and request information.
7. Banks can approve, conditionally approve, or decline with a reason.
8. Banks can issue versioned conditional offers.
9. Applicants can accept or reject valid offers.
10. Post-approval documents and checklist items are tracked.
11. Banks can record disbursement details.
12. All dashboards use live database values.
13. BRD reports are available according to role.
14. Major lifecycle events generate notifications.
15. Status changes appear in history and audit logs.
16. Existing working functionality and frontend design remain intact.
17. The complete solution builds without errors.
18. Migrations apply without deleting existing data.

## 28. Final Deliverables
- Updated SMElevate V2 source code
- EF Core migrations and seed data
- Updated configuration templates
- Updated main setup documentation
- Implementation Gap Report
- Implementation Progress Report
- Database Change Log
- Test Report
- Pending External Integrations Report
- Final build confirmation
