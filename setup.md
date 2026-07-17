# SMElevate Portal – Phase 1 Frontend Setup

## 1. Project Title

**SMElevate**

## 2. Purpose

Create the Phase 1 frontend for the **SMElevate Digital Lending Portal** using the current design/theme. This phase is limited to the **End User module only** and must not include database integration.

The portal will allow SME/business applicants to register or login, complete their profile, verify their email/mobile token, access the dashboard, create an SME loan request, select bank and payment identifier details, and submit the application. The submitted application will later be routed to the Bank Portal, which will be developed in Phase 2.

## 3. Important Scope Restriction

### Phase 1 Development Scope

Only work inside the following folder:

```text
EndUser/
```

Do not modify:

```text
Admin/
Bank/
Shared/
Core/
Infrastructure/
Database/
```

Do not create or update database tables, migrations, seeders, stored procedures, APIs, or backend persistence logic in Phase 1.

Use temporary frontend/static/mock data only where required.

## 4. Design Requirement

Use the attached design/theme as the base UI.

Do not change:

- Existing theme colors
- Layout structure
- Font style
- Button style
- Card design
- Header/footer styling
- Sidebar styling, if already available

Only customize the content, labels, screens, and navigation required for the SMElevate End User journey.

## 5. Portal Branding

The portal title must be:

```text
SMElevate
```

Use this title consistently on:

- Login page
- Header/navbar
- Dashboard
- Profile page
- Loan request screens
- Browser/page title

Suggested subtitle:

```text
Digital Portal for Financing to Business Enterprises
```

## 6. Phase 1 User Role

After successful login, the user must be treated as:

```text
EndUser
```

The EndUser must only see End User screens and must not see Admin or Bank Portal screens.

## 7. Phase 1 Screens

Create the following frontend screens inside the EndUser module.

### 7.1 Login / Registration Screen

Create a professional login screen for SMElevate.

The login page must include:

- Portal title: SMElevate
- Subtitle: Digital Portal for Financing to Business Enterprises
- Login/Register section
- OAuth buttons:
  - Continue with Google
  - Continue with Apple
  - Continue with Microsoft
- Mobile/email based registration option, if suitable in the design
- Link: How to Use the Portal
- Link: Terms & Conditions
- Link: SBP Related Regulations
- Link: SBP SME Finance / relevant SBP website resources

The OAuth buttons are frontend-only placeholders in Phase 1. Do not implement live OAuth integration in this phase.

### 7.2 How to Use the Portal Page / Modal

Add a simple user guide page or modal explaining:

1. Register or sign in using OAuth/mobile/email.
2. Complete applicant profile.
3. Verify email/mobile through token.
4. Open dashboard.
5. Create SME loan request.
6. Enter business and facility details.
7. Select bank and IBAN/RAAST details.
8. Submit application.
9. Use unique request ID to track the application.

### 7.3 Terms & Conditions Page / Modal

Add a frontend page or modal for terms and conditions.

Content should mention:

- Applicant confirms correctness of submitted information.
- Applicant agrees that data may be shared with selected bank for credit assessment.
- Submission does not guarantee loan approval.
- Approval is subject to bank assessment, CDD/KYC, regulatory checks, and documentation.
- Applicant agrees to receive notifications through email/mobile.

### 7.4 SBP Related Links Section

Add links section for SBP-related regulations and schemes.

Use placeholder URLs if exact final URLs are not available, but keep labels professional, for example:

- SBP SME Finance
- SBP Prudential Regulations for SME Financing
- SBP Refinance / Subsidized Finance Schemes
- SBP Official Website

These links should be editable from the frontend file/config later.

### 7.5 Profile Completion Screen

After login, redirect EndUser to profile completion screen if profile is not completed.

Fields required:

- First Name
- Last Name
- Mobile No.
- CNIC
- Business Email Address
- Gender of Proprietor
  - Male
  - Female
  - Other / Prefer not to say

Validation rules:

- First Name: required
- Last Name: required
- Mobile No.: required
- CNIC: required, Pakistani CNIC format recommended
- Business Email Address: required, valid email format
- Gender: required

After submitting profile:

- Show token verification screen
- Token should be simulated in Phase 1
- Display message that verification token has been sent to registered business email/mobile number

### 7.6 Token Verification Screen

Create a verification screen with:

- Verification token input
- Resend token button
- Verify button
- Message area for success/error

Since there is no database/backend in Phase 1, use mock verification logic.

Suggested mock token:

```text
123456
```

After successful verification, redirect to EndUser dashboard.

### 7.7 EndUser Dashboard

Create dashboard page for verified EndUser.

Dashboard must include:

- Welcome message
- Profile status
- Total loan requests
- Submitted requests
- Draft requests
- Latest request status
- Button: Generate SME Loan Request

The dashboard should use the attached theme card/stat layout.

### 7.8 SME Loan Request Form – Section 1: Business & Facility Details

Clicking **Generate SME Loan Request** should open the SME Loan Request form.

Create Section 1 with the following fields:

#### Business Details

- Name of Business
- Contact Person
- Cell / Landline No.
- Business Address
- Annual Sales (Rs.)
- Year of Establishment
- No. of Employees
- NTN No. (If applicable)
- Business Premise
  - Owned
  - Rented
- Business Registration
  - Yes
  - No
- Registration Authority, if business is registered
- Business Status
  - Proprietorship
  - Partnership
  - Pvt Ltd Company
- Business Nature
  - Manufacturing
  - Services
  - Trading
- Business Description
  - Example: textile, surgical, IT, retail, etc.

#### Individual / Shareholding Details

Allow one or more individual/shareholder rows with:

- Name of Individual
- Contact No.
- Email
- CNIC No.
- Shareholding (%)

If possible, add frontend-only button:

```text
+ Add Individual / Shareholder
```

#### Facility Requested

- Facility Requested
- Type of Facility
- Amount
- Tenor

At the bottom of Section 1, add button:

```text
Next
```

Section 2 must remain hidden by default and only become visible after Section 1 validation and clicking Next.

### 7.9 SME Loan Request Form – Section 2: Bank Detail

Section name:

```text
Bank Detail
```

This section must be hidden by default.

After user completes Section 1 and clicks Next, show this section.

Fields:

- Select Bank dropdown
- Select Payment Identifier Type dropdown:
  - IBAN
  - RAAST ID
- Dynamic text field based on selected value:
  - If IBAN selected, show field label: Enter IBAN
  - If RAAST ID selected, show field label: Enter RAAST ID

Add submit button:

```text
Submit Application
```

### 7.10 Application Submission Confirmation

After submission, generate a unique frontend-only SME loan request ID.

Suggested format:

```text
SME-YYYYMMDD-0001
```

Show confirmation screen with:

- Success message
- Unique Request ID
- Applicant Name
- Business Name
- Selected Bank
- Facility Amount
- Submitted Date
- Status: Submitted to Bank Portal

Message:

```text
Your SME loan request has been submitted successfully and will be routed to the selected bank portal in Phase 2.
```

Add buttons:

- Back to Dashboard
- View Application Detail

### 7.11 Application Detail Screen

Create a read-only application detail screen using the unique request ID.

Display:

- Request ID
- Business Details
- Facility Details
- Bank Details
- Current Status
- Submission Date

Status should be frontend/static for Phase 1:

```text
Submitted to Bank Portal
```

## 8. Navigation Requirements

EndUser navigation should include:

- Dashboard
- Complete Profile
- Generate SME Loan Request
- My Applications
- Help / How to Use
- Terms & Conditions
- Logout

Do not show Admin or Bank menus in Phase 1.

## 9. Frontend Validation Requirements

Apply client-side validation for required fields.

Validation should include:

- Required fields
- CNIC format
- Email format
- Mobile number format
- Numeric validation for annual sales, employees, amount, tenor
- Shareholding percentage must be numeric and between 0 and 100
- IBAN/RAAST field required based on dropdown selection

## 10. Mock Data Requirements

Since there is no database in Phase 1, use mock/static data for:

### Banks Dropdown

Use sample banks:

- National Bank of Pakistan
- Habib Bank Limited
- United Bank Limited
- MCB Bank Limited
- Allied Bank Limited
- Bank Alfalah Limited
- Meezan Bank Limited
- Bank of Punjab
- Bank of Khyber
- JS Bank Limited

### Application Status

Use sample statuses:

- Draft
- Submitted to Bank Portal
- Under Review
- Returned for Correction

For Phase 1, submitted application status should be:

```text
Submitted to Bank Portal
```

## 11. Technical Instructions

- Keep the existing project structure.
- Only create or modify files under EndUser folder.
- Do not add database logic.
- Do not add Entity Framework migrations.
- Do not add SQL scripts.
- Do not create backend APIs unless already required by existing frontend routing.
- Use mock data/static JSON/local storage/session storage if needed.
- Keep code clean and modular.
- Use reusable components/partials where possible.
- Follow existing naming conventions of the project.
- Keep UI responsive for desktop, tablet, and mobile.

## 12. Suggested Frontend State Flow

```text
Login/Register
    ↓
Set Role = EndUser
    ↓
Profile Completion
    ↓
Token Verification
    ↓
Dashboard
    ↓
Generate SME Loan Request
    ↓
Business & Facility Details
    ↓
Bank Detail
    ↓
Submit Application
    ↓
Confirmation with Unique Request ID
    ↓
Application Detail / Dashboard
```

## 13. Phase 2 Placeholder

Do not implement Bank Portal in Phase 1.

Only show a status/message after submission:

```text
Submitted to Bank Portal. Bank Portal workflow will be implemented in Phase 2.
```

## 14. Acceptance Criteria

Phase 1 will be considered complete when:

1. SMElevate login screen is created using attached theme.
2. OAuth buttons for Google, Apple, and Microsoft are visible as frontend placeholders.
3. How to Use, Terms & Conditions, and SBP Related Links are available from login page.
4. After login, user is treated as EndUser.
5. EndUser can complete profile.
6. Verification token screen appears after profile submission.
7. Verified user is redirected to dashboard.
8. Dashboard includes Generate SME Loan Request button.
9. SME loan request form includes all required fields.
10. Bank Detail section is hidden by default and appears after clicking Next.
11. User can select bank and IBAN/RAAST ID.
12. Unique SME loan request ID is generated after submission.
13. Confirmation page is displayed after submission.
14. Application detail page can show submitted request details.
15. No database changes are made.
16. No files outside EndUser folder are modified.
17. Existing theme colors and layout are preserved.

## 15. Out of Scope for Phase 1

The following items must not be implemented in Phase 1:

- Bank Portal
- Admin Portal
- Database integration
- Entity Framework migrations
- SQL Server schema
- Real OAuth integration
- Real SMS/email token service
- Credit bureau integration
- NADRA integration
- FBR/NTN verification
- IBAN validation API
- RAAST integration
- Document upload workflow
- Loan approval workflow
- Disbursement workflow
- Reporting module

## 16. Future Phase 2 Summary

Phase 2 will include the Bank Portal where bank users will receive submitted applications, review details, perform credit assessment, update application status, request documents, issue conditional offer letters, and process approval or rejection.
