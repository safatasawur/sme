# setup-phase2.md

# SMElevate – Phase 2 Admin Portal Frontend Setup

## 1. Project Title

**SMElevate**

## 2. Phase

**Phase 2 – Admin Portal Frontend**

## 3. Objective

Create the **Admin Portal frontend** for SMElevate using the current existing design and theme. This phase must focus only on Admin screens and Admin frontend workflows.

The Admin Portal will allow authorized admin users to manage users, roles, SME schemes, banks, lookup fields, and portal settings. This phase is frontend-only and must not include database implementation.

---

# 4. Important Development Restrictions

## 4.1 Folder Restriction

Only edit or create files inside the following folder:

```text
Admin
```

Do not edit any other folder, including but not limited to:

```text
EndUser
Bank
SBP
Shared
Core
Infrastructure
Database
Migrations
wwwroot theme files outside Admin scope
```

## 4.2 Database Restriction

This phase must be developed **without database**.

Do not create:

- Database tables
- SQL scripts
- Entity Framework migrations
- DbContext changes
- Repository layer
- API persistence
- Backend database logic

Use only:

- Static data
- Mock JSON data
- In-memory frontend data
- Browser local storage/session storage if required

## 4.3 Theme Restriction

Use the attached/existing design and theme.

Do not change:

- Theme colors
- Global layout
- Typography
- Existing styling pattern
- Sidebar style
- Header style
- Card design
- Button design
- Input design

The Admin Portal should visually match the existing SMElevate/Phase 1 theme.

---

# 5. Admin Portal Scope

The following modules must be created inside the Admin folder only:

1. Admin Login
2. Admin Dashboard
3. User Management
4. Role Management
5. SME Scheme Management
6. Form Builder
7. Bank Management
8. Bank Member Assignment
9. Master Lookup Field Management
10. Settings Management

---

# 6. Admin Login Screen

## 6.1 Login Requirements

Create an Admin login screen with the title:

```text
SMElevate
```

The login screen must contain:

- Username field
- Password field
- Login button
- Forgot password link/place-holder if suitable

## 6.2 Login Restrictions

The Admin login screen must **not** contain:

- Registration page
- Sign up option
- Google OAuth button
- Apple OAuth button
- Microsoft OAuth button
- How to Use link
- Terms & Conditions link
- SBP regulations related links
- SME subsidy scheme links

## 6.3 Login Behavior

After successful mock login, redirect the user to the Admin Dashboard.

Use mock credentials or frontend-only validation.

Example:

```text
Username: admin
Password: admin123
```

Display proper validation messages for empty username/password.

---

# 7. Admin Dashboard

## 7.1 Dashboard Purpose

The Admin Dashboard must provide a graphical overview of SMElevate portal activity.

## 7.2 Dashboard Cards / Widgets

Show the following count cards:

1. SMEs Registered
2. Total No. of Applications
3. In Process Applications
4. Completed Applications

## 7.3 Graphical View

Create graphical/statistical sections using frontend mock data, such as:

- Application status chart
- SME registration trend
- Bank-wise application summary
- Scheme-wise application summary

Charts may be created using the existing theme chart components or simple frontend chart placeholders.

## 7.4 Dashboard Restriction

The Admin Dashboard must **not** contain:

- Generate SME Loan Request button
- Create loan request button
- Submit loan application option

Admin role has view and configuration rights only. Admin must not create SME loan requests.

---

# 8. Navigation Menu

Create Admin sidebar/navigation menu with the following sections:

```text
Dashboard
User Management
Role Management
SME Scheme Management
Bank Management
Master Lookup Fields
Settings
```

The navigation should remain consistent with the attached theme.

---

# 9. User Management Module

## 9.1 Purpose

Admin can create and manage portal users.

## 9.2 User List Screen

Create a user listing screen with:

- Search box
- Filter by user type
- Filter by active/inactive status
- Add New User button
- User table/list
- Edit action
- View action
- Activate/Deactivate action

## 9.3 User Fields

The Add/Edit User form must contain:

| Field | Requirement |
|---|---|
| Full Name | Mandatory |
| Email Address | Mandatory |
| Mobile No. | Mandatory |
| User Type | Mandatory |
| Active | Mandatory |
| Member Of | Optional |

## 9.4 User Type Options

User Type dropdown must include:

```text
SME
Bank
SBP
Admin
```

## 9.5 Member Of Section

Create a section named:

```text
Member Of
```

This section is used to assign a user to a bank.

Rules:

- Optional field
- Should show bank dropdown/list
- More relevant for Bank users
- Can be hidden/disabled for SME users if required
- Use mock bank records

---

# 10. Role Management Module

## 10.1 Purpose

Admin can manage system roles and assign roles to users.

## 10.2 Default Roles

Create the following default roles:

```text
EndUser
Banks
SBP
Admin
```

## 10.3 Role Screen Requirements

Create Role Management screen with:

- Role list
- Add role button
- Edit role button
- View role details
- Permission matrix placeholder

## 10.4 Permission Matrix

Create a simple frontend permission matrix with modules and permissions:

- View
- Create
- Edit
- Delete
- Approve
- Publish

This can be static/mock in Phase 2.

---

# 11. SME Scheme Management Module

## 11.1 Purpose

Admin can create and manage SME schemes.

## 11.2 Scheme List Screen

Create screen with:

- Scheme list
- Search
- Status filter
- Add New Scheme button
- Edit/View actions
- Publish/Unpublish action

## 11.3 Scheme Fields

Scheme form should contain:

- Scheme Name
- Scheme Code
- Scheme Description
- Eligibility Criteria
- Start Date
- End Date
- Status
- Published flag

## 11.4 Scheme Status Options

Use mock lookup values:

```text
Draft
Published
Inactive
Archived
```

---

# 12. Form Builder Module

## 12.1 Purpose

Admin can create SME loan application forms from scratch and publish them for EndUser role.

## 12.2 Form Builder Requirements

Create a frontend form builder screen that allows Admin to:

- Create a new form
- Add sections
- Add fields
- Configure field type
- Configure required/optional status
- Configure placeholder/help text
- Configure lookup/dropdown values
- Reorder fields if possible
- Preview form
- Save form as draft
- Publish form

## 12.3 Field Types

Form Builder must support frontend mock creation of the following field types:

```text
Text Field
Text Area
Number
Date
Dropdown
Radio Button
Checkbox
Email
Mobile Number
CNIC
File Upload Placeholder
```

## 12.4 Lookup Field Support

Admin must be able to select lookup fields created in the Master Lookup Field module.

Example lookup fields:

- Business Nature
- Facility Type
- Tenor
- Application Status
- Business Premise
- Business Registration

## 12.5 Publish Behavior

After save and publish, show the form as:

```text
Published for EndUser Role
```

Since this phase is frontend-only, the published form can be shown using mock/local storage data.

Do not modify EndUser folder in this phase.

---

# 13. Bank Management Module

## 13.1 Purpose

Admin can create and manage participating banks.

## 13.2 Bank List Screen

Create Bank Management screen with:

- Bank list/table
- Search by bank name
- Add New Bank button
- Edit/View action
- Active/Inactive action
- Update Member button

## 13.3 Bank Fields

The Add/Edit Bank form must contain:

| Field | Requirement |
|---|---|
| Bank Name | Mandatory |
| IBAN Prefix | Mandatory |
| Bank Code | Optional |
| Bank Email Address | Mandatory |
| Active | Mandatory |

## 13.4 Validation Rules

- Bank Name is required
- IBAN Prefix is required
- Bank Email Address is required and must be valid email format
- Bank Code is optional

---

# 14. Bank Member Assignment Module

## 14.1 Purpose

Admin can assign multiple users as members of a bank.

## 14.2 Access

This screen should open after clicking:

```text
Update Member
```

from the Bank Management screen.

## 14.3 Screen Requirements

Create a separate Bank Member Assignment screen/page with:

- Selected Bank summary
- User filter panel
- User list/table
- Checkbox for selecting multiple users
- Assign selected users button
- Remove selected users button
- Current assigned members section

## 14.4 Filters

The filter panel must include:

- Email Address
- Role
- User Type
- Active status

## 14.5 Assignment Behavior

Use mock/static data only.

After assigning users, show frontend confirmation message:

```text
Selected users have been assigned to the bank successfully.
```

---

# 15. Master Lookup Field Module

## 15.1 Purpose

Admin can create reusable lookup fields used in the form builder and other modules.

## 15.2 Lookup List Screen

Create screen with:

- Lookup field list
- Search
- Add New Lookup button
- Edit/View action
- Active/Inactive action

## 15.3 Lookup Field Form

Fields:

- Lookup Name
- Lookup Code
- Description
- Active
- Lookup Values

## 15.4 Lookup Values

Admin should be able to add multiple values under one lookup field.

Example:

Lookup Name:

```text
Application Status
```

Values:

```text
Submitted
In Process
Completed
Rejected
```

Examples of lookup fields:

```text
Status
Tenor
Business Nature
Facility Type
Business Premise
Business Registration
User Type
Role
```

---

# 16. Settings Module

## 16.1 Purpose

Admin can configure portal-level frontend settings.

## 16.2 Settings Sections

Create tabs/sections for:

1. Email Settings
2. Notification Settings
3. OAuth Client Settings
4. General Portal Settings

## 16.3 Email Settings

Fields:

- SMTP Host
- SMTP Port
- SMTP Username
- SMTP Password placeholder
- From Email
- From Name
- Enable SSL/TLS

## 16.4 Notification Settings

Fields/options:

- Enable Email Notifications
- Enable SMS Notifications
- Enable Portal Notifications
- OTP Template placeholder
- Application Submission Template placeholder
- Approval Notification Template placeholder

## 16.5 OAuth Client Settings

Fields:

- Google Client ID
- Google Client Secret placeholder
- Apple Client ID
- Apple Client Secret placeholder
- Microsoft Client ID
- Microsoft Client Secret placeholder

These settings are only frontend placeholders in Phase 2.

## 16.6 General Portal Settings

Fields:

- Portal Name
- Support Email
- Support Phone
- Maintenance Mode toggle
- Session Timeout placeholder

---

# 17. Mock Data Requirements

Use mock/static data for:

- Dashboard counts
- SME users
- Bank users
- SBP users
- Admin users
- Roles
- Banks
- Schemes
- Lookup fields
- Form builder fields
- Settings

Do not call real APIs.
Do not connect to database.

---

# 18. UI/UX Requirements

The Admin Portal should include:

- Clean dashboard cards
- Tables with actions
- Search and filters
- Add/Edit/View forms
- Modal or page-based forms
- Confirmation messages
- Validation messages
- Breadcrumbs if available in theme
- Responsive layout
- Consistent Admin navigation

---

# 19. Security Placeholder Requirements

Since this is frontend-only, create placeholders for:

- Admin authentication
- Role-based menu visibility
- Session timeout
- Logout
- Password masking

Do not implement real authentication backend.

---

# 20. Phase 2 Out of Scope

The following must not be implemented in Phase 2:

- Database
- Real authentication
- Real API integration
- Real email sending
- Real SMS sending
- Real OAuth integration
- Real Microsoft/Google/Apple client validation
- Real loan request creation by Admin
- Editing EndUser module
- Editing Bank portal module
- Editing SBP portal module
- Backend workflow engine
- Credit bureau integration
- RAAST integration
- IBAN validation integration

---

# 21. Acceptance Criteria

Phase 2 is complete when:

1. Admin Portal login screen is created.
2. Login screen has username and password only.
3. No registration link exists on Admin login.
4. No OAuth buttons exist on Admin login.
5. No How to Use, Terms & Conditions, or SBP related links exist on Admin login.
6. Admin Dashboard is created.
7. Dashboard shows counts for:
   - SMEs Registered
   - Total No. of Applications
   - In Process Applications
   - Completed Applications
8. Dashboard has graphical/statistical views.
9. Dashboard does not have Generate SME Loan Request button.
10. User Management module is created.
11. Role Management module is created.
12. SME Scheme Management module is created.
13. Form Builder frontend is created.
14. Bank Management module is created.
15. Bank Member Assignment screen is created.
16. Master Lookup Field module is created.
17. Settings module is created.
18. Only Admin folder is edited.
19. No database work is added.
20. Existing theme/design is preserved.

---

# 22. Notes for Developer / Claude

- Implement Phase 2 only.
- Do not read or modify old setup files unless specifically required for theme understanding.
- Do not change Phase 1 EndUser screens.
- Do not create backend or database logic.
- Keep the project title as SMElevate.
- Use mock data only.
- Keep all Admin portal work inside Admin folder.
- Maintain the same attached design/theme.

