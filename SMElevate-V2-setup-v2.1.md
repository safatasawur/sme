
# SMElevate V2 - Updated Setup Specification (BRD Aligned)
**Version:** 2.1  
**Status:** Updated after BRD Review (Documentation Alignment Only)

> **Important**
> This update does **not** change the project architecture or implementation scope. It aligns the existing specification with the approved Business Requirements Document (BRD).

---

# 1. Project Objective

Develop **SMElevate V2**, a centralized digital portal for SME Financing managed by SBP that enables SMEs to submit a **single standardized Loan Application** to a participating bank while allowing banks to perform their own credit assessment and decision-making.

---

# 2. Existing Modules (No Functional Changes)

- OAuth Authentication (Google, Microsoft, Apple)
- Manual Registration
- OTP Verification
- End User Portal
- Bank Portal
- Admin Portal
- Dynamic Form Builder
- Role-Based Access Control
- Notification Module
- Email Module
- Dashboard
- Reporting
- Audit Logs

---

# 3. Terminology Update

Replace **Loan Request** terminology with **Loan Application** throughout the project documentation and future development.

---

# 4. Standardized Loan Application Form (LAF)

The Dynamic Form Builder shall support an SBP standardized Loan Application Form.

Features:
- Versioning
- Publish/Unpublish
- Dynamic Sections
- Validation Rules
- Future revisions without code changes

---

# 5. Application Lifecycle

Draft
→ Submitted
→ Validated
→ Referred to Bank
→ Credit Bureau Check
→ Risk Assessment
→ Customer Due Diligence (CDD)
→ Compliance Check
→ Conditional Offer
→ Accepted / Declined
→ Post Approval Formalities
→ Disbursement
→ Monitoring
→ Closed

---

# 6. Bank Processing

The portal records status only. The bank performs:

- Credit Bureau Check
- Risk Assessment
- Standardized Credit Scorecard
- Customer Due Diligence (CDD)
- AML & Compliance
- Approval Decision

---

# 7. Conditional Offer Management

Add documentation for:

- Offer Letter
- Accept
- Reject
- Expiry
- Version History
- Digital Acceptance

---

# 8. Post Approval

Support tracking for:

- Legal Documents
- Additional Documents
- Bank Checklist
- Completion Status

---

# 9. Disbursement Tracking

Store:

- Amount
- Value Date
- Destination Account
- Status
- Remarks

The portal tracks status only; banks perform disbursement.

---

# 10. Monitoring

Read-only monitoring:

- Active
- Closed
- Defaulted (future)
- Settled (future)

---

# 11. Consent Management

Capture:

- Consent Checkbox
- Timestamp
- IP Address
- Audit Trail

---

# 12. Case Management

Every application receives a unique immutable Case ID.

---

# 13. Notifications

Generate notifications for:

- Registration
- Submission
- Referral
- Under Assessment
- Additional Information Required
- Conditional Offer
- Offer Accepted
- Offer Rejected
- Post Approval Pending
- Disbursement
- Completion

---

# 14. Reports

Include:

- Application Status
- Turnaround Time
- Referral Report
- Assessment Report
- Offer Report
- Decline Analysis
- Disbursement Summary
- Geographic Analysis
- Bank Performance
- Dashboard Analytics

---

# 15. Dashboard Enhancements

## Applicant
- Active Applications
- Timeline
- Notifications
- Pending Actions

## Bank
- Assigned Applications
- Under Review
- Offers
- Approved
- Declined
- Disbursed

## Admin
- Total Applications
- Total Applicants
- Total Banks
- Approval Rate
- Average TAT
- Geographic Distribution
- Bank Comparison

---

# 16. Future Integration Points

Reserved only (no implementation):

- Credit Bureau
- NADRA
- RAAST
- AML
- KYC
- SMS Gateway
- Email Gateway
- Digital Signature
- Document Verification

---

# 17. Architecture

Retain current architecture.

Documentation shall additionally include:

- Business Architecture
- Application Architecture
- Security Architecture
- Data Architecture
- Integration Architecture
- Infrastructure Architecture
- Deployment Architecture
- Disaster Recovery
- High Availability

---

# 18. Workflow Engine (Future Ready)

Support configurable workflows:

- Scheme-specific workflow
- Configurable statuses
- SLA timers
- Approval levels
- Event-driven notifications

---

# 19. No Project Changes

This specification update **does not require changes** to:

- UI
- Database Schema
- Existing Modules
- Folder Structure
- Authentication
- Controllers
- Services

It only enriches documentation and prepares the platform for future enhancements.
