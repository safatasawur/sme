# setup-phase3.md

# SMElevate – Phase 3: Banks Portal Frontend

## 1. Project Title

**SMElevate**

This phase is for creating the **Banks Portal frontend only** using the already attached/existing design/theme from the previous phases.

The implementation must be completed **without database integration** and must use only mock/static/local storage data.

---

## 2. Phase Scope

Phase 3 will create the **Banks role portal** for participating banks.

The Banks Portal will allow bank users to:

- Login using email address and OTP verification
- Access their bank dashboard
- View application statistics assigned to their bank
- View SME loan requests assigned to their bank
- Review readonly loan application details submitted by EndUser
- Approve or reject loan requests
- Add approval/rejection remarks
- Attach supporting documents
- View approved requests
- View rejected requests
- View their bank/user profile information

---

## 3. Strict Development Rules

Claude/developer must follow these rules strictly:

1. **Only edit the `Banks` folder.**
2. Do not edit `EndUser` folder.
3. Do not edit `Admin` folder.
4. Do not edit shared/global theme files unless they already exist inside the `Banks` folder.
5. Do not create or modify database files.
6. Do not create migrations.
7. Do not create API controllers.
8. Do not create backend services.
9. Do not create SQL scripts.
10. Do not implement real authentication.
11. Use mock/static/local storage data only.
12. Preserve the existing attached design/theme.
13. Keep the same project title: **SMElevate**.
14. The Banks role must not have any option to create SME loan requests.
15. Banks users must only view and update status of requests assigned to their bank.

---

## 4. Banks Portal Login Requirements

### 4.1 Login Screen

Create a Banks login screen with the same theme and design style used in previous phases.

The login screen must contain only:

- Email Address field
- Send OTP button
- OTP verification section
- Verify OTP button

### 4.2 Login Flow

The login flow should work as a frontend mock flow:

1. Bank user enters email address.
2. System checks mock user list.
3. If email exists and user role is `Banks`, show OTP sent message.
4. OTP section becomes visible.
5. User enters OTP.
6. If OTP is valid, user is redirected to Banks Dashboard.

### 4.3 Mock OTP

Use a mock OTP such as:

```text
123456
```

Show a frontend message:

```text
OTP has been sent to your registered bank email address.
```

### 4.4 Login Validation

Validation rules:

- Email address is required.
- Email format must be valid.
- Email must exist in mock users list.
- User type/role must be `Banks`.
- OTP is required.
- OTP must match mock OTP.

### 4.5 Login Page Restrictions

The Banks login page must **not** contain:

- Registration link
- Signup option
- How to Use link
- Terms & Conditions link
- SBP related links
- OAuth login buttons
- Google login
- Apple login
- Microsoft login

---

## 5. Banks Portal Layout

After successful OTP verification, the bank user should access the Banks Portal.

The Banks Portal layout should include:

- Sidebar navigation
- Header/top bar
- Logged-in user information
- Bank name display
- Logout option
- Dashboard cards
- Request tables
- Status badges
- Action buttons where applicable

---

## 6. Sidebar Menu

Create the following sidebar items inside Banks Portal:

1. Dashboard
2. Request Management
   - Assigned Requests
   - Approved Requests
   - Rejected Requests
3. Bank Profile
4. Logout

The sidebar should match the attached design/theme.

---

## 7. Banks Dashboard Requirements

The dashboard page must show graphical/statistical cards for requests assigned to the logged-in bank only.

### 7.1 Dashboard Cards

Display the following counts:

1. Total No. of Applications
2. In Process Applications
3. Completed Applications

### 7.2 Dashboard Charts

Add graphical views using frontend-only mock data, such as:

- Application status chart
- Monthly application trend chart
- Approved vs Rejected chart

### 7.3 Dashboard Restrictions

The Banks Dashboard must **not** contain:

- Generate SME Loan Request button
- Submit Loan Request button
- Create Application button
- EndUser profile completion option
- Admin configuration options

Banks users have no right to create loan requests.

---

## 8. Request Management Module

Create a Request Management module for Banks role.

The module must contain the following sections/pages:

1. Assigned Requests
2. Approved Requests
3. Rejected Requests

Each section should show only requests assigned to the logged-in bank.

---

## 9. Assigned Requests Page

The Assigned Requests page should show a table of loan requests assigned to the bank.

### 9.1 Table Columns

Suggested columns:

- Unique Request ID
- SME/Business Name
- Contact Person
- Facility Requested
- Amount
- Tenor
- Submitted Date
- Current Status
- Action

### 9.2 Actions

Each assigned request should have:

- View Details button
- Review/Process button

---

## 10. Request Detail Page

When the bank opens a request, show the complete request details submitted by the EndUser.

### 10.1 Readonly Fields

All EndUser submitted fields must be readonly for Bank role.

Readonly request fields include:

- Unique Request ID
- Name of Business
- Contact Person
- Cell / Landline No.
- Business Address
- Annual Sales (Rs.)
- Year of Establishment
- No. of Employees
- NTN No. if applicable
- Business Premise owned/rented
- Business Registration Yes/No
- Registration Authority
- Business Status
- Name of Individuals
- Contact Details
- CNIC No.
- Shareholding (%)
- Business Nature
- Business Description
- Facility Requested
- Type of Facility
- Amount
- Tenor
- Selected Bank
- IBAN / RAAST Type
- IBAN / RAAST Value
- Submitted Date
- Current Status

### 10.2 Bank Editable Section

Below readonly request details, create a separate section named:

```text
Bank Decision
```

Bank can update only the following fields:

- Decision Status
- Reason / Remarks
- Attachment / Supporting Document

### 10.3 Decision Status Options

Decision Status dropdown should include:

- In Process
- Approved
- Rejected

### 10.4 Remarks Field

- Required when status is Approved or Rejected
- Multi-line textarea
- Placeholder: `Enter bank decision reason or remarks`

### 10.5 Attachment Field

- File upload field for supporting document
- Allow PDF/Image mock upload
- Show selected file name on screen
- No real file upload required

### 10.6 Submit Decision Button

Add a button:

```text
Submit Decision
```

On click:

- Validate required fields
- Update request status in mock/local state
- Show success message
- Move request to Approved Requests or Rejected Requests based on status

---

## 11. Approved Requests Page

The Approved Requests page should show all approved requests assigned to the logged-in bank.

### 11.1 Table Columns

- Unique Request ID
- SME/Business Name
- Facility Requested
- Amount
- Tenor
- Approved Date
- Remarks
- Action

### 11.2 Action

- View Details

All fields should remain readonly.

---

## 12. Rejected Requests Page

The Rejected Requests page should show all rejected requests assigned to the logged-in bank.

### 12.1 Table Columns

- Unique Request ID
- SME/Business Name
- Facility Requested
- Amount
- Tenor
- Rejected Date
- Rejection Reason
- Action

### 12.2 Action

- View Details

All fields should remain readonly.

---

## 13. Bank Profile Module

Create a Bank Profile page for the logged-in bank user.

The profile page should show readonly bank/user data from mock data.

### 13.1 Bank Information

Display:

- Bank Name
- IBAN Prefix
- Bank Code
- Bank Email Address
- Assigned User Full Name
- Assigned User Email
- Assigned User Mobile No.
- User Role: Banks
- Active Status

### 13.2 Restrictions

Bank users should only view profile data.

They must not edit:

- Bank name
- IBAN prefix
- Bank code
- Email address
- Role
- Active status

---

## 14. Mock Data Requirements

Use mock/static/local storage data for:

- Bank users
- Banks
- Assigned loan requests
- Approved loan requests
- Rejected loan requests
- Dashboard statistics

### 14.1 Sample Mock Bank User

```json
{
  "fullName": "Bank Officer",
  "email": "bank.user@example.com",
  "mobileNo": "03001234567",
  "userType": "Banks",
  "active": true,
  "memberOf": "ABC Bank Limited"
}
```

### 14.2 Sample Bank Data

```json
{
  "bankName": "ABC Bank Limited",
  "ibanPrefix": "ABCD",
  "bankCode": "ABC001",
  "bankEmailAddress": "bank.user@example.com"
}
```

### 14.3 Sample Request Data

Use sample SME loan request records that include:

- Request ID
- Business details
- Facility details
- Bank details
- Status
- Remarks
- Attachment name

Request IDs should be unique, for example:

```text
SME-REQ-2026-0001
SME-REQ-2026-0002
SME-REQ-2026-0003
```

---

## 15. UI/UX Requirements

The Banks Portal should follow the same design language as previous phases.

Use:

- Cards
- Tables
- Badges
- Buttons
- Modal dialogs if suitable
- Responsive layout
- Clean dashboard charts
- Form validation messages
- Success/error alerts

Status badge examples:

- Assigned
- In Process
- Approved
- Rejected
- Completed

---

## 16. Validation Requirements

### Login Validation

- Email required
- Valid email format
- Must exist in mock user list
- Must have role `Banks`
- OTP required
- OTP must match mock OTP

### Decision Validation

- Decision status required
- Remarks required for Approved or Rejected
- Attachment optional unless design requires it

---

## 17. Frontend Flow

```text
Banks Login
↓
Enter Email Address
↓
Validate Email and Banks Role
↓
Send Mock OTP
↓
Verify OTP
↓
Banks Dashboard
↓
Request Management
↓
Open Assigned Request
↓
View Readonly EndUser Request Details
↓
Update Bank Decision
↓
Submit Decision
↓
Request moves to Approved or Rejected list
```

---

## 18. Acceptance Criteria

Phase 3 will be accepted when:

1. Banks Portal is created inside `Banks` folder only.
2. Title remains **SMElevate**.
3. Login page uses email address and OTP only.
4. No registration page exists for Banks role.
5. Login page has no How to Use, Terms & Conditions, SBP links, or OAuth buttons.
6. OTP verification is implemented using mock/static logic.
7. Only users with role `Banks` can access Banks Portal.
8. Dashboard shows Total Applications, In Process Applications, and Completed Applications.
9. Dashboard has no loan request creation button.
10. Request Management module is available.
11. Assigned Requests page is available.
12. Approved Requests page is available.
13. Rejected Requests page is available.
14. EndUser submitted request fields are readonly for Banks role.
15. Bank can only update decision status, reason/remarks, and attachment.
16. Approved requests move to Approved Requests section.
17. Rejected requests move to Rejected Requests section.
18. Bank Profile page is available as readonly.
19. No database, API, migration, or backend persistence is created.
20. Existing attached design/theme is preserved.

---

## 19. Out of Scope for Phase 3

The following are not required in Phase 3:

- Real database
- Real APIs
- Real OTP email sending
- Real authentication
- Real document upload
- Real bank integration
- Real approval workflow engine
- Admin portal changes
- EndUser portal changes
- SBP portal changes
- Payment or disbursement integration
- Credit bureau integration
- NADRA/FBR integration
- RAAST/IBAN validation integration

---

## 20. Important Notes for Claude/Developer

- This is frontend-only Phase 3.
- Use the same title and branding as previous phases: **SMElevate**.
- Use attached theme/design.
- Only the `Banks` folder is allowed to be edited.
- Banks role is not allowed to create loan requests.
- Banks role can only process requests assigned to their bank.
- EndUser submitted information must remain readonly.
- Use realistic mock data so the portal looks complete for demonstration.
