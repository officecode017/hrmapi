# UI Prompt 09: Shifts Catalog & Off-Day Schedule Planner

## 1. Overview & Purpose
The operational hub for scheduling work hours, defining shift policies, assigning shifts to employees (individually or in bulk), and orchestrating organization-wide off-day schedules (e.g. standard weekend offs, 2nd/4th Saturday policies, or alternating rotating rosters).

---

## 2. Keka-Inspired Visual Architecture
* **Top Navigation & Tabs**:
  - `[ Shift Catalog & Rules ]` (Active tab)
  - `[ Employee Shift Assignments ]`
  - `[ Off-Day & Weekend Planner ]`
* **Shift Catalog Grid / Cards**:
  - Cards displaying each configured shift:
    - Shift Name: e.g. `Standard Morning Shift` or `Night Support Shift`.
    - Timing Ribbon: `09:00 AM - 06:00 PM (9h 0m Total)`.
    - Night Shift Badge: If applicable, dark badge with moon icon `🌙 Night Shift`.
    - Rule Metrics:
      - `Grace Period`: `15 mins` (Lateness threshold).
      - `Break Duration`: `60 mins` (Lunch / Refreshment).
      - `Minimum Hours`: `4.5h Half Day • 8.0h Full Day`.
    - Active Staff Counter: e.g. `124 Employees Assigned`.
    - Card Actions: `Edit Shift`, `Assign Employees`, `Delete`.
  - Primary CTA Button: `+ Create New Shift` (Opens Shift Config Modal).
* **Bulk Shift Assignment Modal**:
  - Title: `Assign Shift to Employees`.
  - Target Shift Selector: Dropdown showing shifts with hours.
  - Effective From Date: Date picker.
  - Multi-Select Employee Selector:
    - Filter by Department (e.g. Select all *Support Team* or *Tech Operations*).
    - Searchable checkbox list of employees with current shift tags.
    - Summary Pill: `18 Employees Selected`.
  - Submit Button: `Confirm & Assign Shift`.
* **Off-Day & Weekend Matrix Planner**:
  - Visual 7-day schedule grid with week-of-month rows:
    - Columns: `Sunday`, `Monday`, `Tuesday`, `Wednesday`, `Thursday`, `Friday`, `Saturday`.
    - Rows: `Week 1`, `Week 2`, `Week 3`, `Week 4`, `Week 5`.
    - Each cell is an interactive toggle:
      - 🟢 Working Day
      - ⚪ Scheduled Off-Day (e.g. All Sundays highlighted as Off; 2nd & 4th Saturday toggleable).
  - Quick Template Buttons:
    - `[ 5-Day Week (Sat-Sun Off) ]`
    - `[ 6-Day Week (Sun Only Off) ]`
    - `[ 2nd & 4th Saturday Off ]`
  - Save Changes button.

---

## 3. Backend API Contract

### A. Get Shifts
* **Method**: `GET`
* **URL**: `/api/shift?organizationId=1`
* **Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "organizationId": 1,
      "name": "Standard Morning Shift",
      "startTime": "09:00:00",
      "endTime": "18:00:00",
      "graceMinutes": 15,
      "breakDurationMinutes": 60,
      "isNightShift": false,
      "halfDayHours": 4.5,
      "fullDayHours": 8.0,
      "isActive": true
    },
    {
      "id": 2,
      "organizationId": 1,
      "name": "Night Operations Shift",
      "startTime": "21:00:00",
      "endTime": "06:00:00",
      "graceMinutes": 15,
      "breakDurationMinutes": 45,
      "isNightShift": true,
      "halfDayHours": 4.5,
      "fullDayHours": 8.0,
      "isActive": true
    }
  ]
}
```

### B. Create Shift
* **Method**: `POST`
* **URL**: `/api/shift`
* **Request Payload**:
```json
{
  "organizationId": 1,
  "name": "Mid-Day Flexible Shift",
  "startTime": "11:00:00",
  "endTime": "20:00:00",
  "graceMinutes": 20,
  "breakDurationMinutes": 60,
  "isNightShift": false,
  "halfDayHours": 4.0,
  "fullDayHours": 8.0
}
```
* **Response**:
```json
{
  "success": true,
  "message": "Shift created successfully.",
  "data": {
    "id": 3,
    "name": "Mid-Day Flexible Shift"
  }
}
```

### C. Bulk Assign Shift to Employees
* **Method**: `POST`
* **URL**: `/api/shift/assign/bulk`
* **Request Payload**:
```json
{
  "employeeIds": [5, 12, 18, 22],
  "shiftId": 2,
  "effectiveFrom": "2026-09-08"
}
```
* **Response**:
```json
{
  "success": true,
  "message": "Shift assigned to 4 employees successfully.",
  "data": true
}
```

### D. Get Scheduled Off-Days
* **Method**: `GET`
* **URL**: `/api/offday?organizationId=1&academicYearId=1`
* **Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "organizationId": 1,
      "academicYearId": 1,
      "dayOfWeek": 0,
      "weekOfMonth": 0,
      "description": "Weekly Off - All Sundays",
      "isActive": true
    },
    {
      "id": 2,
      "organizationId": 1,
      "academicYearId": 1,
      "dayOfWeek": 6,
      "weekOfMonth": 2,
      "description": "Weekly Off - 2nd Saturday",
      "isActive": true
    },
    {
      "id": 3,
      "organizationId": 1,
      "academicYearId": 1,
      "dayOfWeek": 6,
      "weekOfMonth": 4,
      "description": "Weekly Off - 4th Saturday",
      "isActive": true
    }
  ]
}
```

---

## 4. Master Prompt for AI UI Generators

```text
Create a Keka HRMS-inspired Shift Management & Off-Day Scheduling Studio in React with Tailwind CSS and Lucide-react.

Layout & Core Features:
1. Header & Navigation:
   - Header: "Shift Rules & Work Schedule Planner".
   - Tabs:
     * "Shift Catalog & Policies" (Active).
     * "Employee Shift Allocation".
     * "Weekly Off-Day Matrix".
   - Top Right Action: "+ Create New Shift" (Indigo button #4F46E5).

2. Shift Catalog Card View:
   - Responsive grid of shift cards:
     * Card Title: "Standard Morning Shift" with active green dot.
     * Large Time Banner: "09:00 AM - 06:00 PM" with clock icon.
     * Specification chips:
       - "Grace: 15 mins"
       - "Break: 60 mins"
       - "Min Full Day: 8.0h"
     * Footer: "124 Employees Assigned" with a "Bulk Assign" text button and 3-dots dropdown menu (Edit, Delete).

3. Create / Edit Shift Modal:
   - Shift Name input.
   - Start Time & End Time pickers.
   - Night Shift toggle switch (displays moon icon when enabled).
   - Numeric inputs for Grace Period (minutes), Break Duration (minutes), and Half Day threshold (hours).
   - Save Shift button.

4. Bulk Shift Assignment Modal:
   - Target Shift dropdown.
   - Effective Date picker.
   - Multi-select employee list with department filter tabs (All, Engineering, Operations, Sales).
   - Checkbox next to each employee row with current assigned shift tag.
   - Confirm button displaying "Assign to X Employees".

5. Off-Day Calendar Matrix:
   - Visual 7-column grid representing days of the week (Sunday through Saturday).
   - Interactive toggle cards for "All Sundays", "2nd Saturday", "4th Saturday".
   - Template pills: "5-Day Week (Sat-Sun Off)" and "Alternate Saturdays (2nd & 4th Off)".

Visual Styling:
- Keka aesthetic: Clean light theme (#F8FAFC canvas), crisp borders (#E2E8F0), rounded-xl cards, Indigo #4F46E5 primary brand color.
```
