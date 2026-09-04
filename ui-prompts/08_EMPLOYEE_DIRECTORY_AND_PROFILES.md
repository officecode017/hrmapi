# UI Prompt 08: Employee Directory & 360° Profile Hub

## 1. Overview & Purpose
The organization-wide people directory and employee management suite. Enables HR Admins, Managers, and Staff to browse employee rosters, view detailed 360° professional profiles (department, designation, reporting line, contact details, assigned shift, leave allocations), and provides HR with an onboarding wizard to create and edit employee profiles.

---

## 2. Keka-Inspired Visual Architecture
* **Top Header & View Controls**:
  - Title: `Employee Directory` with subtitle `150 active members across 3 campuses`.
  - Search & Multi-filter Bar:
    - Live Search: Search by name, employee code, email, or skill.
    - Department Filter Dropdown: `All Departments`, `Engineering`, `HR`, `Marketing`, `Sales`.
    - Location Filter Dropdown: `All Locations`, `Bengaluru HQ`, `Hyderabad R&D`, `Pune Hub`.
    - View Switcher: `[ 🪟 Grid / Card View ]` | `[ ☰ List / Table View ]`.
  - Primary Action Button: `+ Add Employee` (Deep Indigo button `#4F46E5`, launches 3-Step Onboarding Modal).
* **Grid Card View (Keka Employee Cards)**:
  - Responsive 3-to-4 column cards:
    - Top card background: Subtle brand pattern or light pastel banner.
    - Centered circular avatar with online status indicator dot.
    - Full Name (`Arjun Verma`) in bold 16px.
    - Designation chip (`Senior Full Stack Engineer`).
    - Department & Location badge (`Engineering • Bengaluru Campus`).
    - Quick action icons: Email button, Phone call button, Chat button.
    - Bottom strip: Employee ID (`EMP-005`) • Assigned Shift (`Morning 9AM-6PM`).
    - Clicking card opens the **Right Slide-Over 360° Profile Drawer**.
* **Table List View**:
  - Clean data table with pagination controls (`Showing 1 - 10 of 150 employees`).
  - Columns:
    1. `Employee`: Avatar, Full Name, Work Email.
    2. `Employee ID`: Monospace badge (`EMP-005`).
    3. `Department`: Blue tag (`Engineering`).
    4. `Designation`: Gray text (`Senior Full Stack Engineer`).
    5. `Location`: Pin icon + (`Bengaluru Campus`).
    6. `Joining Date`: Date (`Jan 15, 2024`).
    7. `Status`: Green badge (`Active`).
    8. `Actions`: Eye icon (View 360 Profile), Pencil icon (Edit), Ellipsis (Deactivate).
* **Slide-Over 360° Employee Drawer (Width: 540px)**:
  - Header with large avatar, employee full name, code, designation, and "Active" status badge.
  - Tabbed sub-sections:
    - `[ Overview ]`: Contact information, emergency contact, date of birth, gender, blood group, work email, personal phone.
    - `[ Job & Organization ]`: Department, reporting manager, date of joining, probation status, branch campus, assigned shift.
    - `[ Attendance & Leave Snapshot ]`: Today's punch status, current month present percentage, remaining leave quotas.
* **3-Step Employee Onboarding Modal / Wizard**:
  - Step Progress Bar: `1. Personal Info` $\rightarrow$ `2. Employment & Shift` $\rightarrow$ `3. Access & Credentials`.
  - Form Fields:
    - **Step 1**: First Name, Last Name, Email, Phone Number, Date of Birth, Gender.
    - **Step 2**: Employee Code (auto-suggested or custom), Department dropdown, Designation dropdown, Location dropdown, Shift dropdown, Date of Joining.
    - **Step 3**: Role selector (`Employee`, `Manager`, `Admin`), temporary password, email invitation toggle.
  - Navigation: `Back`, `Next`, and `Create Employee Profile`.

---

## 3. Backend API Contract

### A. Get Paginated Employees List
* **Method**: `GET`
* **URL**: `/api/employee?organizationId=1&pageNumber=1&pageSize=10`
* **Headers**: `Authorization: Bearer {{token}}`
* **Response**:
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 5,
        "firstName": "Arjun",
        "lastName": "Verma",
        "employeeCode": "EMP-005",
        "email": "arjun.verma@hrm.local",
        "departmentName": "Engineering",
        "designationTitle": "Senior Full Stack Engineer",
        "locationName": "Bengaluru Innovation Campus",
        "shiftName": "Standard Morning Shift",
        "isActive": true
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 150,
    "totalPages": 15
  }
}
```

### B. Get Single Employee Details (360° Profile)
* **Method**: `GET`
* **URL**: `/api/employee/{id}`
* **Response**:
```json
{
  "success": true,
  "data": {
    "id": 5,
    "firstName": "Arjun",
    "lastName": "Verma",
    "employeeCode": "EMP-005",
    "email": "arjun.verma@hrm.local",
    "phoneNumber": "+91 98765 43210",
    "dateOfBirth": "1994-08-15",
    "gender": "Male",
    "dateOfJoining": "2024-01-15",
    "departmentId": 2,
    "departmentName": "Engineering",
    "designationId": 4,
    "designationTitle": "Senior Full Stack Engineer",
    "locationId": 1,
    "locationName": "Bengaluru Innovation Campus",
    "shiftId": 1,
    "shiftName": "Standard Morning Shift",
    "isActive": true
  }
}
```

### C. Create New Employee
* **Method**: `POST`
* **URL**: `/api/employee`
* **Request Payload**:
```json
{
  "organizationId": 1,
  "firstName": "Rohan",
  "lastName": "Mehta",
  "employeeCode": "EMP-015",
  "email": "rohan.mehta@hrm.local",
  "phoneNumber": "+91 91234 56789",
  "dateOfBirth": "1997-04-12",
  "gender": "Male",
  "dateOfJoining": "2026-09-01",
  "departmentId": 2,
  "designationId": 4,
  "locationId": 1,
  "shiftId": 1
}
```
* **Response**:
```json
{
  "success": true,
  "message": "Employee created successfully.",
  "data": {
    "id": 15,
    "employeeCode": "EMP-015"
  }
}
```

---

## 4. Master Prompt for AI UI Generators

```text
Create a Keka HRMS-inspired Employee Directory & Profile Manager in React, Tailwind CSS, and Lucide icons.

Layout & Core Features:
1. Header & Directory Controls:
   - Header: "Employee Directory" with active employee counter badge ("150 Total Members").
   - Filter Bar:
     * Search input with search icon ("Search by name, ID, or email...").
     * Department Dropdown ("All Departments", "Engineering", "Design", "HR").
     * Location Dropdown ("All Locations", "Bengaluru HQ", "Hyderabad Hub").
     * View Switcher Buttons: [Cards Grid View] | [Table List View].
     * "+ Add Employee" button in Deep Indigo (#4F46E5).

2. Employee Grid Card View:
   - Clean 4-column card grid.
   - Each card features:
     * Pastel header strip with centered rounded avatar and green active status ring.
     * Full Name ("Arjun Verma") and Designation ("Senior Full Stack Engineer").
     * Department & Office badge ("Engineering • Bengaluru").
     * Bottom action row with quick Email and Phone buttons.
     * Clicking anywhere on the card slides open the 360° Detail Drawer.

3. Slide-Over 360° Profile Drawer (Width 500px):
   - Opens smoothly from right edge.
   - Header with employee photo, status tag ("Active • Full Time"), and close button.
   - Tabs: [Personal Info] [Work & Shift] [Leave Balances].
   - Info Grid displaying clean key-value pairs (Work Email, Phone, Date of Birth, Joining Date, Department, Manager, Location, Shift).

4. 3-Step Employee Onboarding Wizard Modal:
   - Step 1: Personal Details (First Name, Last Name, Email, Phone, DOB, Gender).
   - Step 2: Work & Assignment (Employee Code, Department, Designation, Campus Location, Assigned Shift, Joining Date).
   - Step 3: Access & Role (Role Dropdown, Temporary Password, Welcome Email checkbox).
   - Footer buttons: "Back", "Next", "Submit".

Visual Styling:
- Keka enterprise aesthetic: Crisp white backgrounds, rounded-xl cards, subtle borders (#E2E8F0), sharp typography (Inter), deep indigo buttons.
```
