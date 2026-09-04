# UI Prompt 07: Overtime (OT) Operations, Approvals & Policy Engine

## 1. Overview & Purpose
A specialized workspace for managing employee overtime (OT). Enables managers and employees to log extra work hours beyond normal shifts, allows managers/admins to review and approve/adjust overtime hours, and provides HR administrators with full control over overtime policy parameters (multiplier rates, minimum thresholds, approval workflows, monthly caps).

---

## 2. Keka-Inspired Visual Architecture
* **Top Tab Navigation**:
  - `[ Pending OT Approvals (4) ]` (Active with amber badge)
  - `[ Overtime Logged History ]`
  - `[ Policy & Rate Configuration ]`
* **OT Approvals Workspace**:
  - Quick KPI summary: `Pending Hours: 14.5 hrs` | `Approved This Month: 86.0 hrs` | `Estimated Payout: ₹48,500`.
  - Request Cards / Table:
    - Employee Name, Role, Department.
    - Shift Date & Scheduled Hours: e.g. `Sep 01, 2026 • 09:00 - 18:00 (9 hrs)`.
    - Actual Clock Times: e.g. `09:02 AM - 21:32 PM (12.5 hrs logged)`.
    - Calculated OT Claim: e.g. `3.5 Hours Overtime`.
    - Reason / Deliverable notes: `"Production release hotfix and data migration support"`.
    - Multiplier Tag: `1.5x Multiplier Rate`.
    - Quick Approval Controls:
      - Stepper / Input field to adjust approved hours (e.g. adjust 3.5 hrs $\rightarrow$ 3.0 hrs).
      - Inline `Approve` (Emerald) and `Reject` (Rose) buttons with prompt for manager remarks.
* **Log Overtime Modal / Drawer**:
  - Header: `Log Overtime Hours`.
  - Employee Selector (prefilled for self-service or searchable for team managers).
  - Date Picker.
  - Number of OT Hours (e.g. `2.5`).
  - Project / Task Association.
  - Reason & Work Performed description.
  - Submit button.
* **Overtime Policy Engine Settings Panel (Admin)**:
  - Settings Card with intuitive toggle switches and numeric controls:
    - `Enable Overtime Tracking`: Toggle switch (On/Off).
    - `Require Manager Approval`: Toggle switch (If disabled, auto-approves from punch calculations).
    - `Minimum Qualifying Minutes`: Input box (e.g., `30 minutes` minimum extra time before OT kicks in).
    - `Overtime Pay Multiplier`: Number stepper (e.g., `1.5x` for standard weekdays, `2.0x` for weekends/holidays).
    - `Monthly Cap on OT Hours`: Number input (e.g., `30.0 hrs max per employee per month`).
    - `Save Policy` CTA button with confirmation alert.

---

## 3. Backend API Contract

### A. Get Overtime Settings
* **Method**: `GET`
* **URL**: `/api/overtime/settings?organizationId=1`
* **Response**:
```json
{
  "success": true,
  "data": {
    "id": 1,
    "organizationId": 1,
    "isOvertimeAllowed": true,
    "minimumMinutesBeforeOT": 30,
    "overtimeMultiplier": 1.5,
    "requireApproval": true,
    "maxMonthlyOTHours": 40.0
  }
}
```

### B. Update Overtime Settings
* **Method**: `PUT`
* **URL**: `/api/overtime/settings/{id}`
* **Request Payload**:
```json
{
  "isOvertimeAllowed": true,
  "minimumMinutesBeforeOT": 45,
  "overtimeMultiplier": 1.5,
  "requireApproval": true,
  "maxMonthlyOTHours": 35.0
}
```
* **Response**:
```json
{
  "success": true,
  "message": "Overtime settings updated successfully.",
  "data": true
}
```

### C. Get Pending Overtime Entries
* **Method**: `GET`
* **URL**: `/api/overtime/pending?organizationId=1`
* **Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": 18,
      "employeeId": 5,
      "employeeName": "Arjun Verma",
      "date": "2026-09-01",
      "claimedHours": 3.5,
      "status": "Pending",
      "reason": "Production release hotfix and deployment monitoring",
      "createdAt": "2026-09-01T22:00:00Z"
    }
  ]
}
```

### D. Process Overtime Approval / Adjustment
* **Method**: `POST`
* **URL**: `/api/overtime/{id}/approval`
* **Request Payload**:
```json
{
  "isApproved": true,
  "approvedHours": 3.0,
  "remarks": "Approved 3.0 hours as 30 mins was standard dinner break"
}
```
* **Response**:
```json
{
  "success": true,
  "message": "Overtime entry processed successfully.",
  "data": true
}
```

### E. Record Overtime Entry (Manual Submission)
* **Method**: `POST`
* **URL**: `/api/overtime/record`
* **Request Payload**:
```json
{
  "employeeId": 5,
  "date": "2026-09-03",
  "otHours": 2.0,
  "reason": "Post-maintenance server restart verification"
}
```
* **Response**:
```json
{
  "success": true,
  "message": "Overtime recorded successfully.",
  "data": true
}
```

---

## 4. Master Prompt for AI UI Generators

```text
Build a Keka HRMS-style Overtime (OT) Management & Approvals Screen in React with Tailwind CSS and Lucide-react.

Layout & Component Sections:
1. Header & Quick Stat Bar:
   - Header: "Overtime Operations & Approvals".
   - 3 Summary Stat Badges:
     * Pending Approval: "14.5 Hours (4 requests)" in Amber badge.
     * Approved Month-to-Date: "92.0 Hours" in Emerald badge.
     * Rate Multiplier: "1.5x Base Rate Active" in Blue badge.
   - Tabs: [Pending Approvals (4)] [Logged Overtime History] [OT Policy & Rules].

2. Pending Approvals Queue:
   - Clean list of cards for fast manager review:
     * Employee info: Avatar, Name, Role, Department badge.
     * Date & Claimed Hours: e.g. "Sep 01, 2026 • 3.5 Claimed OT Hours".
     * Reason card: Light slate callout box showing task details.
     * Hours Adjuster: Editable number stepper allowing approvers to modify approved hours before signing off.
     * Action Buttons:
       - "Reject" (Red outline button).
       - "Approve" (Emerald solid button with check icon).

3. Log Overtime Slide-Over Drawer:
   - "+ Log Overtime" button on top right opens right slide-out panel.
   - Form controls:
     * Employee selector (pre-selected for self-service).
     * Date picker.
     * Overtime Hours (numeric step input e.g. 2.5).
     * Reason textarea.
     * Submit button with loading state.

4. Overtime Policy Configuration Tab:
   - Clean settings form for HR Admins:
     * Toggle: "Allow Overtime across organization".
     * Toggle: "Require Manager Approval prior to payroll credit".
     * Input: "Minimum threshold minutes before OT activates" (e.g. 30 mins).
     * Input: "Standard Overtime Multiplier" (e.g. 1.5x).
     * Input: "Max allowable monthly OT hours per employee" (e.g. 40.0 hrs).
     * "Save Policy Changes" button at bottom right.

Visual Styling:
- High-grade enterprise design matching Keka HR: Clean borders, 12px rounded cards, neutral slate palette with vivid indigo/emerald accents.
```
