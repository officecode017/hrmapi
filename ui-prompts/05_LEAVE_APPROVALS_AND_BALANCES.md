# UI Prompt 05: Leave Approvals Queue & Quota Administration

## 1. Overview & Purpose
Designed for HR Admins and Reporting Managers to review, approve, or reject employee leave requests with contextual team calendar visibility. It also includes the administrative Leave Balance Adjuster allowing HR to grant comp-offs, adjust quotas, or execute annual carry-over corrections.

---

## 2. Keka-Inspired Visual Architecture
* **Top Navigation & Counter Strip**:
  - Segmented Page Tabs:
    - `[ Pending Approvals (3) ]` (Active with blue counter chip)
    - `[ All Applications History ]`
    - `[ Employee Balances Ledger ]`
    - `[ Leave Policy / Types Config ]`
  - Quick Search & Filter: Filter by Department (e.g. *Engineering, HR, Sales*), Leave Type, Date Range.
* **Pending Approval Action Cards & Table**:
  - Split view or expandable list view:
    - Employee Avatar, Name, Employee Code, and Designation (`Arjun Verma • EMP-005 • Senior Full Stack Engineer`).
    - Department Pill: `Engineering`.
    - Leave Request Capsule: `Casual Leave • Sep 28 - Sep 29 (2.0 Days)`.
    - Reason snippet: `"Attending sibling's graduation ceremony"`.
    - **Contextual Peer Impact Widget**:
      - Micro indicator: `"⚠️ 1 other team member on leave during this period (Neha Patel)"`.
    - Remaining Quota Badge: `Available: 8.0 Days`.
    - Inline Action Buttons:
      - Quick Approve Button: Emerald green outline button with checkmark icon (`Approve`).
      - Quick Reject Button: Rose red outline button with cross icon (`Reject`).
      - Clicking opens an inline or modal dialog requesting mandatory or optional `Approver Remarks`.
* **Leave Balance Adjustment Modal (HR Admin Privilege)**:
  - Header: `Adjust Employee Leave Balance`.
  - Form Fields:
    - `Employee`: Autocomplete dropdown searchable by name or code.
    - `Academic Year`: Selector (e.g., `2026 - 2027`).
    - `Leave Type`: Dropdown (e.g., `Privilege Leave`, `Casual Leave`, `Comp-Off`).
    - `Adjustment Type`: Segmented control: `[ + Credit Days ]` | `[ - Debit Days ]`.
    - `Days to Adjust`: Numeric step input (e.g., `+2.0`).
    - `Effective Balance Preview`: `Current: 10.0` $\rightarrow$ `New: 12.0 Days`.
    - `Reason / Audit Justification`: Mandatory textarea (e.g., *"Compensatory off for production deployment on Sunday, Aug 30"*).
  - Footer: `Cancel` and `Save Balance Adjustment`.

---

## 3. Backend API Contract

### A. Get Pending Leave Requests
* **Method**: `GET`
* **URL**: `/api/leave/pending?organizationId=1`
* **Headers**: `Authorization: Bearer {{token}}`
* **Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": 43,
      "employeeId": 5,
      "employeeName": "Arjun Verma",
      "leaveTypeId": 2,
      "leaveTypeName": "Casual Leave",
      "startDate": "2026-09-28",
      "endDate": "2026-09-29",
      "isHalfDay": false,
      "totalDays": 2.0,
      "reason": "Attending sibling's graduation ceremony",
      "status": "Pending",
      "createdAt": "2026-09-04T08:30:00Z"
    }
  ]
}
```

### B. Approve Leave Request
* **Method**: `POST`
* **URL**: `/api/leave/{id}/approve`
* **Request Payload**:
```json
{
  "isApproved": true,
  "remarks": "Approved. Please ensure handoff is shared with the sprint lead."
}
```
* **Response**:
```json
{
  "success": true,
  "message": "Leave application approved successfully.",
  "data": true
}
```

### C. Reject Leave Request
* **Method**: `POST`
* **URL**: `/api/leave/{id}/reject`
* **Request Payload**:
```json
{
  "isApproved": false,
  "remarks": "Critical production deployment scheduled for this week. Please reschedule."
}
```
* **Response**:
```json
{
  "success": true,
  "message": "Leave application rejected.",
  "data": true
}
```

### D. Adjust Leave Balance (HR Admin)
* **Method**: `POST`
* **URL**: `/api/leave/balances/adjust`
* **Request Payload**:
```json
{
  "employeeId": 5,
  "leaveTypeId": 1,
  "academicYearId": 2,
  "adjustmentDays": 2.0,
  "reason": "Comp-off credited for weekend release on Aug 30"
}
```
* **Response**:
```json
{
  "success": true,
  "message": "Leave balance adjusted successfully.",
  "data": true
}
```

---

## 4. Master Prompt for AI UI Generators

```text
Build a Keka HRMS-style Leave Approvals & Balance Management Dashboard in React with Tailwind CSS and Lucide-react.

Layout & Core Features:
1. Top Bar & Tabbed Navigation:
   - Page Title: "Leave Management & Approvals".
   - Tabs:
     * "Pending Approvals" (highlighted pill with red badge "3 pending").
     * "Past Approvals & History".
     * "Employee Balances Directory".
     * "Leave Types Policy".
   - Right Side CTAs:
     * "+ Adjust Balance" button (White button with border, opens Adjustment Modal).
     * Filter by Department select dropdown.

2. Pending Approvals Queue:
   - Present a list of request cards designed for rapid manager review.
   - Each Card:
     * Left: Avatar with initials, employee full name, employee ID, designation, and department.
     * Middle: Leave Type pill (e.g. "Casual Leave"), Date range ("Sep 28 - Sep 29, 2026"), and duration pill ("2.0 Days").
     * Reason quote: Light gray box with quote icon containing user's reason.
     * Context Bar: "Team Availability: 8/9 available • Remaining balance: 8 days".
     * Right: Action buttons:
       - "Reject" (Red outline button).
       - "Approve" (Emerald solid button with check icon).
   - Empty State: If no pending requests, display a celebratory illustration with "You're all caught up! No leaves awaiting review."

3. Quick Approval / Rejection Remarks Modal:
   - When "Approve" or "Reject" is clicked, show a compact modal:
     * Title: "Approve Leave Request" or "Reject Leave Request".
     * Textarea: "Approver Remarks / Comments (Optional for approval, mandatory for rejection)".
     * Primary action button changes to "Confirm Approval" or "Confirm Rejection".

4. Balance Adjustment Modal (HR View):
   - Title: "Adjust Employee Leave Balance".
   - Autocomplete searchable employee selector.
   - Dropdown for Leave Type (Casual, Sick, Privilege).
   - Radio buttons for "Credit (+)" vs "Debit (-)".
   - Number input with stepper buttons (+1, -1, +0.5).
   - Mandatory Reason textarea.
   - Save button triggers toast notification: "Balance updated successfully."

Visual Style:
- Professional enterprise UI inspired by Keka HR. Clean white panels (#FFFFFF), slate border outlines (#E2E8F0), soft shadow-sm.
```
