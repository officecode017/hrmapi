# UI Prompt 06: Company-Wide Attendance Operations & Live Feed

## 1. Overview & Purpose
The central command center for HR Managers and Operations leads to monitor company-wide daily attendance in real-time. It provides live headcount stats (Present, Late, On Leave, Absent), a live check-in/out activity stream, GPS geofencing radius validation, and the ability for HR to record/regularize punches on behalf of employees.

---

## 2. Keka-Inspired Visual Architecture
* **Top Metric Banner (4 Dynamic KPI Cards)**:
  - `Total Staffed Today`: Count e.g. `148 Employees` with subtle trend indicator.
  - `Clocked In / Present`: Emerald card e.g. `132 (89.2%)` with mini ring progress meter.
  - `Late Arrivals`: Amber card e.g. `11 Employees (7.4%)` (Clocked in past shift grace threshold).
  - `On Leave / Absent`: Blue/Rose segmented card e.g. `5 Approved Leaves • 0 Unaccounted`.
* **Live Attendance Control & Action Bar**:
  - Date Picker: Quick controls `[ < ]` `Today: Sep 04, 2026` `[ > ]`.
  - Shift Filter: `All Shifts`, `Morning Shift (09:00 - 18:00)`, `Night Shift (20:00 - 05:00)`.
  - Department Dropdown: `All Departments`, `Engineering`, `Human Resources`, `Finance`.
  - Search Box: Instant search by employee name or badge number.
  - Action Buttons:
    - `📥 Export Attendance CSV / Excel`
    - `+ Manual Attendance Entry` (for biometric device failures or on-duty client visits).
* **Live Punch Feed & Roster Table**:
  - Columns:
    1. `Employee`: Avatar, Full Name, Employee Code (`Pooja Sharma • EMP-012`).
    2. `Department & Designation`: `Quality Assurance • Lead QA Engineer`.
    3. `Shift`: Shift badge (e.g., `General Day 09:00 - 18:00`).
    4. `First In`: Timestamp + On-time/Late pill (e.g. `08:58 AM` 🟢 On Time or `09:22 AM` 🟡 Late +22m).
    5. `Last Out`: Timestamp (e.g., `06:05 PM` or `--:--` if still working).
    6. `Total Duration`: Live clock counter (e.g., `8h 12m`).
    7. `Geofence & IP Radar`: Campus location badge (`Bengaluru Campus Hub • Within Radius (14m)` or `Remote / Out of Geofence ⚠️`).
    8. `Status`: Pill badge (`Present`, `Late`, `On Leave`, `Half Day`).
    9. `Actions`: Eye icon (View Day Timeline) and Edit icon (Manual Punch Override).
* **Manual Punch Entry Modal (Admin Privileged)**:
  - Header: `Record Manual Punch for Employee`.
  - Employee autocomplete search selector.
  - Punch Type: Radio pills `[ Check-In ]` | `[ Check-Out ]`.
  - Date & Exact Time Picker.
  - Location Selector: Campus dropdown or `Remote/Client Site`.
  - Mandatory Audit Justification: Dropdown (`Biometric Hardware Failure`, `On-Duty Client Meeting`, `Forgot Badge`) + remarks notes.

---

## 3. Backend API Contract

### A. Get Attendance History / Daily Roster
* **Method**: `GET`
* **URL**: `/api/attendance/history?fromDate=2026-09-04&toDate=2026-09-04`
* **Headers**: `Authorization: Bearer {{token}}`
* **Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": 892,
      "employeeId": 5,
      "employeeName": "Arjun Verma",
      "date": "2026-09-04",
      "checkInTime": "2026-09-04T09:02:15Z",
      "checkOutTime": "2026-09-04T18:05:00Z",
      "workDurationMinutes": 542,
      "status": "Present",
      "isLate": false,
      "lateMinutes": 0,
      "locationId": 1,
      "locationName": "Bengaluru Innovation Campus",
      "remarks": "Web Clock-In"
    },
    {
      "id": 893,
      "employeeId": 12,
      "employeeName": "Pooja Sharma",
      "date": "2026-09-04",
      "checkInTime": "2026-09-04T09:24:10Z",
      "checkOutTime": null,
      "workDurationMinutes": 210,
      "status": "Late",
      "isLate": true,
      "lateMinutes": 24,
      "locationId": 1,
      "locationName": "Bengaluru Innovation Campus",
      "remarks": "Metro delay reported"
    }
  ]
}
```

### B. Admin Manual Check-In on Behalf of Employee
* **Method**: `POST`
* **URL**: `/api/attendance/check-in`
* **Request Payload**:
```json
{
  "employeeId": 12,
  "locationId": 1,
  "latitude": 12.9716,
  "longitude": 77.5946,
  "deviceInfo": "Admin Manual Override (Portal)",
  "ipAddress": "192.168.1.50",
  "remarks": "Biometric device offline, manually verified by HR"
}
```
* **Response**:
```json
{
  "success": true,
  "message": "Check-in recorded successfully.",
  "data": {
    "id": 895,
    "employeeId": 12,
    "checkInTime": "2026-09-04T09:00:00Z",
    "status": "Present"
  }
}
```

---

## 4. Master Prompt for AI UI Generators

```text
Create a Keka HRMS-inspired Daily Attendance Operations Dashboard in React, Tailwind CSS, and Lucide icons.

Layout & Core Elements:
1. Header & KPI Cards Strip:
   - Header: Breadcrumb "Attendance > Live Operations & Logs" and date selector with Quick Day buttons (Today, Yesterday).
   - 4 Top Metric Cards:
     * Card 1: "Total Workforce" -> 150 employees with green dot "100% Scheduled".
     * Card 2: "Present Today" -> 138 employees (92%) with mini circular SVG progress ring.
     * Card 3: "Late Arrivals" -> 9 employees (6%) with amber alert badge and icon.
     * Card 4: "On Leave" -> 3 approved leaves (2 Privilege, 1 Sick).

2. Filtering Bar:
   - Search bar: "Search employee by name, code, or email...".
   - Filter pills: [All Staff (150)] [Present (138)] [Late (9)] [On Leave (3)] [Absent (0)].
   - Dropdown selectors: "All Departments" and "All Shifts".
   - Primary action button: "+ Manual Punch Override" (Indigo button).

3. Live Attendance Feed Table:
   - Modern clean table with border-slate-200.
   - Rows show:
     * Avatar with green active pulse badge if currently clocked in.
     * Name, Employee Code, and Designation.
     * Shift badge (e.g. "General Day 9AM - 6PM").
     * Clock In Time with badge (e.g. "09:02 AM" with green "On Time" tag or "09:25 AM" with yellow "Late +25m" tag).
     * Clock Out Time (e.g. "--:--" if active, or "06:15 PM").
     * Active Duration counter (e.g., "5h 42m" with animated ticking dot).
     * Location Pill (e.g. "Bengaluru Campus (GPS Verified)").
     * Actions: "Details" drawer button and "Edit Punch" button.

4. Manual Punch Override Modal:
   - Modal dialog with clean backdrop blur.
   - Form fields:
     - Employee Autocomplete search.
     - Punch Action: [Check-In] or [Check-Out] toggle.
     - Timestamp Picker (Date & Time).
     - Reason dropdown (Hardware Issue, Travel/Client Visit, Other).
     - Remarks note box.
     - Submit button triggers green success toast.

Visual Styling:
- Keka enterprise aesthetic: Crisp white backgrounds, rounded-xl borders, soft shadows, Indigo accent (#4F46E5), Emerald for present, Amber for late arrivals.
```
