# UI Prompt 04: Employee Leave Portal & Application Center

## 1. Overview & Purpose
The Employee Leave Portal is the self-service hub where employees review their annual leave allowances, tracked usage, pending approvals, and submit new leave requests. It also allows employees to cancel pending or approved leaves with automated balance credit restitution.

---

## 2. Keka-Inspired Visual Architecture
* **Top Quota Banner**:
  - Horizontal carousel/grid of cards representing each Leave Type (e.g., *Privilege Leave / Annual*, *Casual Leave*, *Sick Leave*, *Maternity/Paternity*, *Comp-Off*).
  - Each card displays:
    - Leave type title + color-coded icon badge.
    - Prominent balance count: e.g. `12 Available` (in 28px bold font).
    - Horizontal progress bar illustrating `Consumed / Total Quota` (e.g., `6 / 18 Used`).
    - Subtext: `0 Pending Approval` • `Carried Over: 2`.
* **Action Bar**:
  - Primary Action Button (top right): `+ Apply Leave` (Deep Indigo `#4F46E5` button with shine effect, opens Slide-Over Drawer).
  - Academic Year selector dropdown: e.g., `2026 - 2027 (Active)`.
  - Filter Tabs: `All Applications` | `Pending` | `Approved` | `Rejected` | `Cancelled`.
* **Applications History Data Table**:
  - Columns:
    1. `Leave Type`: Colored pill + Leave Name (e.g., `Casual Leave`).
    2. `Duration`: Dates with calendar icon (e.g., `Sep 14, 2026 - Sep 16, 2026`).
    3. `Total Days`: Bold chip (e.g., `3 Days` or `0.5 Day (First Half)`).
    4. `Applied On`: Date timestamp (e.g., `Sep 02, 2026, 11:20 AM`).
    5. `Reason`: Truncated note with tooltip on hover.
    6. `Status`: Keka-styled badge (`Pending` yellow dot, `Approved` green check, `Rejected` red cross, `Cancelled` slate slash).
    7. `Actions`: Ellipsis dropdown or inline button: `Cancel Request` (enabled if status is `Pending` or upcoming `Approved`).
* **Slide-Over "Apply Leave" Drawer (Width: 480px)**:
  - Header: `Apply for Leave` with employee name and current academic cycle.
  - Form Fields:
    - `Leave Type`: Select dropdown pre-populated from `/api/leavetype` with real-time balance badge (e.g., *Casual Leave (8 Available)*).
    - `From Date` & `To Date`: Dual date-picker with public holidays & weekly offs highlighted/blocked.
    - `Is Half Day`: Checkbox toggle. If checked, displays radio pills: `[ First Half (Morning) ]` | `[ Second Half (Afternoon) ]`.
    - `Total Calculated Days`: Auto-calculated badge (e.g., `Calculated Days: 2.0`).
    - `Reason for Leave`: Multi-line textarea (min 10 characters validation).
    - `Attachment / Medical Note` (Optional): Drag-and-drop file uploader zone.
  - Balance Impact Preview Box:
    - Dynamic box showing: `Current Balance: 12.0` $\rightarrow$ `Deduction: -2.0` $\rightarrow$ `Remaining Balance: 10.0`.
  - Drawer Footer:
    - Sticky footer with `Cancel` (outlined) and `Submit Leave Application` (indigo solid with loading spinner).

---

## 3. Backend API Contract

### A. Get Employee Leave Balances
* **Method**: `GET`
* **URL**: `/api/leave/balances?employeeId={id}&academicYearId={yearId}`
* **Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": 101,
      "employeeId": 5,
      "leaveTypeId": 1,
      "leaveTypeName": "Privilege / Annual Leave",
      "academicYearId": 2,
      "allocatedDays": 18.0,
      "usedDays": 6.0,
      "pendingDays": 2.0,
      "remainingDays": 10.0,
      "carriedOverDays": 2.0
    },
    {
      "id": 102,
      "employeeId": 5,
      "leaveTypeId": 2,
      "leaveTypeName": "Casual Leave",
      "academicYearId": 2,
      "allocatedDays": 12.0,
      "usedDays": 4.0,
      "pendingDays": 0.0,
      "remainingDays": 8.0,
      "carriedOverDays": 0.0
    },
    {
      "id": 103,
      "employeeId": 5,
      "leaveTypeId": 3,
      "leaveTypeName": "Sick Leave",
      "academicYearId": 2,
      "allocatedDays": 10.0,
      "usedDays": 1.0,
      "pendingDays": 0.0,
      "remainingDays": 9.0,
      "carriedOverDays": 0.0
    }
  ]
}
```

### B. Get My Leaves Application History
* **Method**: `GET`
* **URL**: `/api/leave/my-leaves/{employeeId}`
* **Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": 42,
      "employeeId": 5,
      "employeeName": "Arjun Verma",
      "leaveTypeId": 1,
      "leaveTypeName": "Privilege / Annual Leave",
      "startDate": "2026-09-21",
      "endDate": "2026-09-22",
      "isHalfDay": false,
      "totalDays": 2.0,
      "reason": "Family road trip to Coorg",
      "status": "Approved",
      "approvedBy": 2,
      "approverName": "Sunita Rao",
      "approverRemarks": "Approved. Have a good trip!",
      "createdAt": "2026-09-02T10:15:00Z"
    }
  ]
}
```

### C. Apply for Leave
* **Method**: `POST`
* **URL**: `/api/leave`
* **Request Payload**:
```json
{
  "employeeId": 5,
  "leaveTypeId": 2,
  "startDate": "2026-09-28",
  "endDate": "2026-09-29",
  "isHalfDay": false,
  "reason": "Attending sibling's graduation ceremony"
}
```
* **Response**:
```json
{
  "success": true,
  "message": "Leave application submitted successfully.",
  "data": {
    "id": 43,
    "employeeId": 5,
    "status": "Pending",
    "totalDays": 2.0
  }
}
```

### D. Self-Cancel Leave
* **Method**: `POST`
* **URL**: `/api/leave/{id}/cancel`
* **Response**:
```json
{
  "success": true,
  "message": "Leave application cancelled and balance restored successfully."
}
```

---

## 4. Master Prompt for AI UI Generators

```text
Create a modern Keka HRMS-inspired Employee Leave Portal in React, Tailwind CSS, and Lucide icons.

Layout & Structure:
1. Top Section - Leave Balance Cards:
   - Horizontal responsive grid of 4 cards:
     * Privilege Leave: Emerald gradient bar, "10.0 Days Available", subtext "6.0 of 18.0 Used • 2.0 Pending".
     * Casual Leave: Indigo gradient bar, "8.0 Days Available", subtext "4.0 of 12.0 Used".
     * Sick Leave: Amber gradient bar, "9.0 Days Available", subtext "1.0 of 10.0 Used".
     * Comp-Off: Violet gradient bar, "2.0 Days Available", subtext "Valid till Oct 31".
   - Each card features a clean two-tone progress ring or mini progress bar.

2. Controls & Filter Bar:
   - Left: Filter tabs with counts: [All (12)] [Pending (1)] [Approved (9)] [Rejected (1)] [Cancelled (1)].
   - Right: Search input field + Academic Year Selector ("AY 2026-27") + "+ Apply Leave" primary CTA button (Indigo #4F46E5 with hover shadow).

3. Leave Applications Table:
   - Columns: Type, Period, Duration, Reason, Applied On, Approver, Status, Actions.
   - Status Badges:
     * Pending: Amber background (#FEF3C7), text #92400E, pulsating dot icon.
     * Approved: Emerald background (#D1FAE5), text #065F46, checkmark icon.
     * Rejected: Rose background (#FFE4E6), text #9F1239.
     * Cancelled: Slate background (#F1F5F9), text #475569.
   - For rows where status is "Pending" or upcoming "Approved", display a subtle red text button: "Cancel".
   - Clicking "Cancel" prompts a confirmation modal: "Cancel Leave Application? 2 days will be credited back to your Casual Leave balance."

4. Slide-Over "Apply Leave" Drawer:
   - Sliders in from the right edge with a backdrop overlay.
   - Form controls:
     - Leave Type select: Displays leave balance inline next to options.
     - Date Range Picker: Start Date & End Date with date range counter.
     - Half-day checkbox + Morning/Afternoon radio selector.
     - Reason textarea with character count indicator.
     - Real-time Balance Forecast callout: "Current: 8 days -> Deducting: 2 days -> New Balance: 6 days".
   - Footer: Sticky footer with "Discard" and "Submit Request" buttons.

Style Guidelines:
- Clean white card backgrounds, light gray borders (#E2E8F0), rounded-xl cards.
- Inter typography with tight tracking on headings.
```
