# SMElevate-v2 - Final Backend Implementation Setup

## 1. Project Title

**SMElevate-v2**

## 2. Objective

The frontend for all three phases is already completed:

- Phase 1: EndUser Portal
- Phase 2: Admin Portal
- Phase 3: Banks Portal

The objective of this final phase is to implement the complete backend using **ASP.NET Core MVC** with **SQL Server** database named:

```text
SMElevate-v2
```

The backend must connect the existing frontend screens to real database-driven functionality without changing the existing frontend design, layout, theme, colors, or user interface.

---

## 3. Critical Instructions

### 3.1 Do Not Change Frontend

Do not redesign or replace any frontend page.

Do not change:

- Existing layout
- Theme colors
- CSS
- Images
- Icons
- Button styles
- Cards
- Sidebar design
- Navbar design
- Existing page look and feel

Only bind the frontend to backend models, controllers, services, database, validation, notifications, and email functionality.

### 3.2 Remove Unused HTML Pages

Remove extra unused `.html` pages that are no longer required by the MVC application.

Before removal:

- Confirm the page is not referenced by any route, layout, sidebar, button, anchor tag, JavaScript, or form action.
- Convert required pages into Razor `.cshtml` views.
- Keep only useful assets such as CSS, JS, images, fonts, and vendor files.
- Do not remove theme assets required by existing frontend pages.

### 3.3 Required Architecture

Use ASP.NET Core MVC structure with module-wise separation.

Required MVC structure:

```text
Controllers/
    Admin/
    Banks/
    EndUser/

Models/
    Admin/
    Banks/
    EndUser/
    Common/

Views/
    Admin/
        Dashboard/
        Users/
        Roles/
        Banks/
        Schemes/
        FormBuilder/
        Lookups/
        Settings/
        Notifications/
    Banks/
        Dashboard/
        Requests/
        Profile/
        Notifications/
    EndUser/
        Account/
        Profile/
        Dashboard/
        LoanRequests/
        Notifications/

Services/
    Interfaces/
    Implementations/

Data/
    ApplicationDbContext.cs

ViewModels/
    Admin/
    Banks/
    EndUser/
    Common/

wwwroot/
    Keep existing frontend theme assets
```

The existing frontend folder/page organization should be mapped to MVC Views like:

```text
Views/Admin/[Admin Pages]
Views/Banks/[Banks Pages]
Views/EndUser/[EndUser Pages]
```

---

## 4. Technology Stack

Use:

- ASP.NET Core MVC
- Entity Framework Core
- SQL Server
- Identity-style authentication or custom authentication using secure password hashing
- Razor Views
- Repository/Service pattern
- Dependency Injection
- Data Annotations validation
- Email service using SMTP settings from database/appsettings
- Session or Cookie Authentication
- Serilog or built-in logging

Do not use:

- Static-only mock data after backend implementation
- Hardcoded users except initial seed admin
- Direct SQL inside controllers
- Database logic inside views
- Business logic inside Razor pages

---

## 5. Database Requirement

Create SQL Server database:

```text
SMElevate-v2
```

Connection string example:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=SMElevate-v2;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

Use EF Core migrations.

Migration name:

```text
InitialCreate_SMElevateV2
```

---

## 6. Core Database Entities

Create the following entities at minimum.

### 6.1 ApplicationUser

Fields:

- Id
- FullName
- EmailAddress
- MobileNo
- PasswordHash
- UserType: SME, Bank, SBP, Admin
- RoleId
- BankId nullable
- IsActive
- IsEmailVerified
- IsMobileVerified
- CreatedAt
- UpdatedAt
- LastLoginAt

### 6.2 Role

Fields:

- Id
- RoleName
- Description
- IsActive
- CreatedAt

Default roles:

- EndUser
- Bank
- SBP
- Admin

### 6.3 Bank

Fields:

- Id
- BankName mandatory
- IBANPrefix mandatory
- BankCode optional
- BankEmailAddress mandatory
- IsActive
- CreatedAt
- UpdatedAt

### 6.4 BankMember

Fields:

- Id
- BankId
- UserId
- IsActive
- AssignedAt

### 6.5 EndUserProfile

Fields:

- Id
- UserId
- FirstName
- LastName
- MobileNo
- CNIC
- BusinessEmailAddress
- GenderOfProprietor
- VerificationToken
- VerificationTokenExpiry
- IsVerified
- CreatedAt
- UpdatedAt

### 6.6 LoanRequest

Fields:

- Id
- RequestNo unique generated value
- UserId
- AssignedBankId
- StatusId
- NameOfBusiness
- ContactPerson
- CellOrLandlineNo
- BusinessAddress
- AnnualSales
- YearOfEstablishment
- NoOfEmployees
- NTNNo nullable
- BusinessPremise owned/rented
- IsBusinessRegistered
- RegistrationAuthority nullable
- BusinessStatus
- IndividualNames
- IndividualContactDetails
- IndividualCNICNo
- ShareholdingPercentage
- BusinessNature
- BusinessDescription
- FacilityRequested
- TypeOfFacility
- Amount
- Tenor
- IBANOrRaastType
- IBANOrRaastValue
- CreatedAt
- SubmittedAt
- UpdatedAt

### 6.7 LoanRequestStatusHistory

Fields:

- Id
- LoanRequestId
- OldStatusId nullable
- NewStatusId
- ChangedByUserId
- Remarks
- AttachmentPath nullable
- CreatedAt

### 6.8 LoanRequestAttachment

Fields:

- Id
- LoanRequestId
- UploadedByUserId
- FileName
- OriginalFileName
- FilePath
- ContentType
- FileSize
- AttachmentType
- UploadedAt

### 6.9 Scheme

Fields:

- Id
- SchemeName
- SchemeDescription
- IsPublished
- CreatedByUserId
- CreatedAt
- UpdatedAt
- PublishedAt nullable

### 6.10 SchemeForm

Fields:

- Id
- SchemeId
- FormName
- FormJson
- IsPublished
- CreatedAt
- UpdatedAt

### 6.11 SchemeFormField

Fields:

- Id
- SchemeFormId
- FieldLabel
- FieldName
- FieldType
- LookupId nullable
- IsRequired
- DisplayOrder
- ValidationRule
- Placeholder
- DefaultValue
- CreatedAt

### 6.12 MasterLookup

Fields:

- Id
- LookupName
- LookupCode
- Description
- IsActive
- CreatedAt

### 6.13 MasterLookupValue

Fields:

- Id
- MasterLookupId
- ValueText
- ValueCode
- DisplayOrder
- IsActive
- CreatedAt

Required default lookup examples:

- Status
- Tenor
- BusinessNature
- BusinessPremise
- BusinessStatus
- FacilityType
- Gender
- UserType

### 6.14 AppNotification

Fields:

- Id
- UserId
- Title
- Message
- NotificationType
- RelatedEntityType
- RelatedEntityId
- IsRead
- CreatedAt
- ReadAt nullable

### 6.15 EmailTemplate

Fields:

- Id
- TemplateCode
- TemplateName
- Subject
- BodyHtml
- RecipientType: EndUser, Bank, Admin
- IsActive
- CreatedAt
- UpdatedAt

### 6.16 EmailLog

Fields:

- Id
- ToEmail
- CcEmail nullable
- Subject
- BodyHtml
- TemplateCode
- Status: Pending, Sent, Failed
- ErrorMessage nullable
- SentAt nullable
- CreatedAt

### 6.17 SystemSetting

Fields:

- Id
- SettingKey
- SettingValue
- SettingCategory
- IsEncrypted
- CreatedAt
- UpdatedAt

Setting categories:

- Email
- Notification
- OAuth
- General

### 6.18 AuditLog

Fields:

- Id
- UserId nullable
- Action
- EntityName
- EntityId nullable
- OldValue nullable
- NewValue nullable
- IPAddress nullable
- UserAgent nullable
- CreatedAt

---

## 7. Authentication Requirements

### 7.1 EndUser Login

EndUser authentication was created in Phase 1 frontend.

Implement backend for:

- OAuth placeholder flow if actual OAuth credentials are not available
- Email/mobile verification token
- Profile completion
- User role assignment as EndUser
- Session/cookie login

### 7.2 Admin Login

Admin login uses:

- Username/email
- Password

No registration page.

Admin users are created by seed data or by existing admin from User Management.

### 7.3 Bank Login

Bank login uses:

- Email address only
- System checks email exists in users table
- User must be active
- User role must be Bank
- User must be mapped with a bank
- Generate OTP
- Send OTP to registered email address
- Verify OTP
- Login to Banks portal after successful OTP verification

No bank registration page.

---

## 8. Module Requirements

# 8.1 Admin Portal Backend

Admin can access only Admin area.

## Admin Dashboard

Show graphical counts:

- SMEs Registered
- Total Number of Applications
- In Process Applications
- Completed Applications
- Approved Applications
- Rejected Applications
- Bank-wise applications
- Scheme-wise applications

## User Management

Admin can:

- Create user
- Edit user
- View user
- Activate/deactivate user
- Assign role
- Assign user type
- Optionally assign bank membership

User fields:

- Full Name
- Email Address
- Mobile No
- User Type: SME, Bank, SBP, Admin
- Active
- Member Of Bank optional

## Role Management

Admin can:

- Create roles
- Edit roles
- Activate/deactivate roles
- Assign role to user

Default roles must not be deleted.

## Bank Management

Admin can:

- Create bank
- Edit bank
- View bank
- Activate/deactivate bank
- Assign multiple members to bank

Bank fields:

- Bank Name mandatory
- IBAN Prefix mandatory
- Bank Code optional
- Bank Email Address mandatory

Bank member assignment page:

- Filter by email address
- Filter by role
- Filter by user type
- Select multiple users
- Save selected members

## SME Scheme Module

Admin can:

- Create SME scheme
- Edit SME scheme
- Publish scheme
- Unpublish scheme
- Create form using form builder
- Add fields from scratch
- Use lookup fields
- Save form as draft
- Publish form

Published form must be visible to EndUser loan request flow.

## Form Builder

Support field types:

- Textbox
- Textarea
- Number
- Date
- Dropdown
- Radio button
- Checkbox
- File upload
- Email
- Mobile number
- CNIC
- NTN

Field properties:

- Label
- Field name
- Field type
- Required yes/no
- Placeholder
- Default value
- Display order
- Lookup binding
- Validation rule

## Master Lookup Field Module

Admin can:

- Create lookup
- Edit lookup
- Activate/deactivate lookup
- Add lookup values
- Edit lookup values
- Sort lookup values

Examples:

- Status
- Tenor
- Facility Type
- Business Nature
- Gender
- Business Status

## Settings Module

Admin can configure:

- SMTP host
- SMTP port
- SMTP username
- SMTP password
- Sender email
- Sender name
- Enable/disable email notifications
- Enable/disable app notifications
- Google Client ID
- Google Client Secret
- Apple Client ID
- Apple Client Secret
- Microsoft Client ID
- Microsoft Client Secret

Sensitive settings must be encrypted or protected.

---

# 8.2 Banks Portal Backend

Bank user can access only Banks area.

## Bank Dashboard

Show counts only for requests assigned to the logged-in user's bank:

- Total Number of Applications
- In Process Applications
- Completed Applications
- Approved Applications
- Rejected Applications

## Request Management

Sidebar sections:

- Assigned Requests
- Approved Requests
- Rejected Requests

Bank users can:

- View requests assigned to their bank
- Open request details
- View readonly EndUser submitted data
- Change status
- Add remarks/reason
- Upload decision/supporting document

Bank users cannot:

- Create SME loan request
- Edit applicant-submitted fields
- View requests assigned to other banks
- Access Admin or EndUser modules

## Status Update

Allowed statuses:

- Assigned
- In Process
- Approved
- Rejected
- Completed
- More Information Required

When bank changes status:

1. Save status in LoanRequest.
2. Save status history in LoanRequestStatusHistory.
3. Create AppNotification for EndUser.
4. Send status-wise email to EndUser.
5. Send status-wise email to Bank user or bank email where required.
6. Save EmailLog.
7. Save AuditLog.

## Bank Profile

Bank user can view:

- Bank Name
- IBAN Prefix
- Bank Code
- Bank Email Address
- Assigned members
- Logged-in user information

---

# 8.3 EndUser Portal Backend

EndUser can access only EndUser area.

## Profile Completion

After login, EndUser completes profile:

- First Name
- Last Name
- Mobile No
- CNIC
- Business Email Address
- Gender of Proprietor

System sends verification token to:

- Business email address
- Mobile number if SMS gateway is configured

After verification:

- Mark profile as verified
- Redirect to dashboard

## EndUser Dashboard

Show:

- Total loan requests
- In process requests
- Approved requests
- Rejected requests
- Completed requests
- Recent notifications
- Recent applications

EndUser can create SME loan request.

## Loan Request Creation

EndUser can create and submit loan request with fields:

- Name of Business
- Contact Person
- Cell / Landline No.
- Business Address
- Annual Sales (Rs.)
- Year of establishment
- No. of employees
- NTN No if applicable
- Business premise owned/rented
- Business Registration Yes/No
- Registration authority if business is registered
- Business status: Proprietorship/Partnership/Pvt Ltd Company
- Name of individuals
- Contact details: cell no, email
- CNIC No
- Shareholding %
- Business Nature: Manufacturing/Services/Trading
- Business description such as textile, surgical, IT, retail etc.
- Facility requested
- Type of facility
- Amount
- Tenor

After clicking Next:

Show Bank Detail section:

- Select bank dropdown
- Select IBAN/RAAST ID dropdown
- Show text field based on selected type
- Submit application

On submit:

- Generate unique RequestNo
- Save loan request
- Assign selected bank
- Set initial status as Assigned/Submitted
- Create notification for EndUser
- Create notification for selected bank users
- Send email to EndUser
- Send email to selected bank
- Show confirmation screen with RequestNo

## Loan Request Detail

EndUser can:

- View request detail by RequestNo
- View status
- View remarks from bank
- View status history
- View uploaded bank documents if allowed
- View notifications

EndUser cannot:

- Change bank decision
- Edit submitted application after submission unless status allows correction
- Access Admin or Banks modules

---

## 9. Notification Requirements

Create in-app notification system.

### 9.1 EndUser Notifications

Generate notification when:

- Profile verification token is issued
- Profile verified successfully
- Loan request submitted
- Bank changes status
- Bank approves request
- Bank rejects request
- Bank requests more information
- Application completed

### 9.2 Bank Notifications

Generate notification when:

- New loan request assigned to bank
- EndUser submits application
- EndUser uploads additional document
- Admin changes bank membership

### 9.3 Notification UI

Bind notifications to existing frontend notification icon/page if available.

Required features:

- Notification list
- Unread count
- Mark as read
- Link to related loan request

---

## 10. Email Template Requirements

Create seed email templates for status-wise communication.

### 10.1 EndUser Email Templates

Create templates:

- ENDUSER_PROFILE_VERIFICATION
- ENDUSER_PROFILE_VERIFIED
- ENDUSER_LOAN_SUBMITTED
- ENDUSER_STATUS_ASSIGNED
- ENDUSER_STATUS_IN_PROCESS
- ENDUSER_STATUS_APPROVED
- ENDUSER_STATUS_REJECTED
- ENDUSER_STATUS_MORE_INFORMATION_REQUIRED
- ENDUSER_STATUS_COMPLETED

### 10.2 Bank Email Templates

Create templates:

- BANK_OTP_LOGIN
- BANK_NEW_REQUEST_ASSIGNED
- BANK_REQUEST_UPDATED
- BANK_STATUS_APPROVED_CONFIRMATION
- BANK_STATUS_REJECTED_CONFIRMATION

### 10.3 Email Placeholders

Support placeholders:

```text
{{FullName}}
{{RequestNo}}
{{BusinessName}}
{{BankName}}
{{Status}}
{{Remarks}}
{{PortalUrl}}
{{OTP}}
{{VerificationToken}}
{{SubmittedDate}}
{{UpdatedDate}}
```

### 10.4 Email Sending Rules

- Use SMTP settings from SystemSetting/appsettings.
- Log every email in EmailLog.
- If email fails, do not break main transaction.
- Save failed email with error message.
- Support resend later if possible.

---

## 11. Unique Request Number Requirement

Generate unique SME loan request ID.

Format example:

```text
SME-YYYYMMDD-000001
```

Example:

```text
SME-20260701-000001
```

Rules:

- Must be unique
- Must be searchable
- Must be displayed on confirmation page
- Must be used for detail lookup
- Must be included in all emails and notifications

---

## 12. Authorization Requirements

Implement role-based access.

### EndUser

Allowed:

- EndUser dashboard
- Profile
- Create loan request
- View own loan requests
- View own notifications

Denied:

- Admin pages
- Banks pages
- Other users' requests

### Bank

Allowed:

- Banks dashboard
- Assigned bank requests
- Bank profile
- Bank notifications

Denied:

- Admin pages
- EndUser request creation
- Requests from other banks

### Admin

Allowed:

- Admin dashboard
- User management
- Role management
- Bank management
- Scheme management
- Form builder
- Master lookup
- Settings
- Reports

Denied:

- Creating EndUser loan request from Admin portal

---

## 13. Validation Requirements

Apply server-side validation and preserve frontend validation where available.

Examples:

- Required fields must be validated.
- Email must be valid format.
- CNIC should be numeric and valid length.
- Mobile number should be valid format.
- Amount must be numeric and greater than zero.
- Annual sales must be numeric and greater than or equal to zero.
- Year of establishment must be valid year.
- NTN optional but validate format if entered.
- Bank Name, IBAN Prefix, and Bank Email are mandatory.
- User email must be unique.
- RequestNo must be unique.

---

## 14. File Upload Requirements

Support attachment upload for bank decision documents and future applicant documents.

Rules:

- Store files under protected folder or controlled wwwroot upload folder.
- Save metadata in LoanRequestAttachment.
- Allowed file types: PDF, JPG, JPEG, PNG, DOC, DOCX.
- Maximum file size should be configurable.
- Do not allow executable files.
- Generate safe file names.

---

## 15. Audit Requirements

Create audit log for:

- Login
- Logout
- User create/update
- Role changes
- Bank create/update
- Bank member assignment
- Scheme create/update/publish
- Lookup changes
- Loan request submission
- Bank status change
- Attachment upload
- Settings changes

---

## 16. Reports Requirements

Admin reports should support:

- Total applications
- Applications by bank
- Applications by status
- Applications by business nature
- Applications by facility type
- Applications by date range
- Approved/rejected applications
- In-process applications

Banks reports should show only their own assigned applications.

EndUser reports should show only their own applications.

---

## 17. Implementation Guidelines

### 17.1 Controllers

Create separate controllers by module.

Examples:

```text
Controllers/Admin/AdminDashboardController.cs
Controllers/Admin/AdminUsersController.cs
Controllers/Admin/AdminRolesController.cs
Controllers/Admin/AdminBanksController.cs
Controllers/Admin/AdminSchemesController.cs
Controllers/Admin/AdminLookupsController.cs
Controllers/Admin/AdminSettingsController.cs

Controllers/Banks/BankDashboardController.cs
Controllers/Banks/BankAuthController.cs
Controllers/Banks/BankRequestsController.cs
Controllers/Banks/BankProfileController.cs

Controllers/EndUser/EndUserAuthController.cs
Controllers/EndUser/EndUserProfileController.cs
Controllers/EndUser/EndUserDashboardController.cs
Controllers/EndUser/EndUserLoanRequestsController.cs
Controllers/EndUser/EndUserNotificationsController.cs
```

### 17.2 Services

Create services:

```text
IUserService
IRoleService
IBankService
ILoanRequestService
INotificationService
IEmailService
IEmailTemplateService
ILookupService
ISchemeService
IFormBuilderService
IAuditService
IFileUploadService
IRequestNumberService
ISettingsService
IAuthService
IOtpService
```

### 17.3 ViewModels

Do not bind database entities directly to views where complex forms are used.

Use ViewModels for:

- Login
- OTP verification
- Profile completion
- Loan request create/edit/detail
- Bank decision
- User create/edit
- Bank create/edit
- Scheme form builder
- Settings

---

## 18. Seed Data

Seed:

- Default Admin user
- Default roles
- Default statuses
- Default lookups
- Default email templates
- Default settings with placeholder values

Default admin example:

```text
Email: admin@smeelevate.local
Password: Admin@12345
Role: Admin
```

Password must be stored as hash, not plain text.

---

## 19. Status Master

Create default statuses:

- Submitted
- Assigned
- In Process
- Approved
- Rejected
- More Information Required
- Completed

Use lookup or dedicated status table. Prefer lookup if the existing Master Lookup module is used.

---

## 20. Acceptance Criteria

Implementation is complete when:

1. Database `SMElevate-v2` is created through EF Core migration.
2. Frontend design remains unchanged.
3. Extra unused HTML pages are removed after verification.
4. Existing frontend pages are converted/bound to Razor MVC views.
5. Admin can login and manage users, roles, banks, schemes, lookups, and settings.
6. Admin dashboard shows real database counts.
7. Bank user can login through email OTP.
8. Bank dashboard shows only assigned bank applications.
9. Bank can view readonly loan request detail.
10. Bank can approve/reject/change status with remarks and attachment.
11. EndUser can complete profile and verify token.
12. EndUser can submit loan request.
13. Loan request gets unique RequestNo.
14. Selected bank receives assigned request.
15. EndUser gets app notification when bank changes status.
16. EndUser receives status-wise email.
17. Bank receives related emails.
18. Email templates are stored and used dynamically.
19. All major actions are logged in AuditLog.
20. Role-based access prevents unauthorized module access.
21. No static mock data remains for implemented backend functionality.

---

## 21. Out of Scope for This Final Setup

Do not implement unless explicitly requested later:

- Real Google OAuth production integration
- Real Apple OAuth production integration
- Real Microsoft OAuth production integration
- Real SMS gateway integration
- Real NADRA integration
- Real FBR integration
- Real eCIB integration
- Real RAAST validation integration
- Real IBAN account ownership validation
- Payment gateway
- Mobile app API

Use placeholders/configuration-ready structure for these integrations.

---

## 22. Final Instruction

Implement the backend carefully without changing the completed frontend design. Use the same SMElevate title and the same Admin, Banks, and EndUser module structure. Ensure the application is production-ready, secure, maintainable, and ready for future integrations.
