# UI Prompt 03: Unified Employee Attendance Calendar

## 1. Overview & Purpose
A visual monthly attendance calendar that aggregates an employee's **Punches (Check-In & Check-Out)**, **Approved Leaves**, **Public Holidays**, and **Weekly Off-Days** in one unified interactive grid.

---

## 2. Keka-Inspired Visual Architecture
* **Top Controls & Filter Strip**:
  - Month Navigator: `< Prev` `September 2026` `Next >` with a `"Today"` shortcut button.
  - View Switcher Toggle: `[ 📅 Calendar View ]` | `[ ☰ List / Log View ]`.
  - Filter by Status: All, Present, On Leave, Holidays, Late Arrivals.
  - Legend Strip: Color-coded indicator dots:
    - 🟢 Present (`#10B981`)
    - 🟡 Late Arrival (`#F59E0B`)
    - 🔵 On Leave (`#3B82F6`)
    - 🟣 Public Holiday (`#8B5CF6`)
    - ⚪ Weekly Off (`#64748B`)
    - 🔴 Absent (`#EF4444`)
* **Monthly Metric Summary Bar (Above Calendar)**:
  - Horizontal card with 6 segmented counters:
    `Total Days: 30` | `Working Days: 22` | `Present: 20` | `Leaves: 2` | `Holidays: 1` | `Weekly Offs: 5` | `Total Hours: 172.5h` | `Overtime: 3.5h`.
* **The 7-Column Calendar Matrix**:
  - Days of week headers: `SUN`, `MON`, `TUE`, `WED`, `THU`, `FRI`, `SAT`.
  - Cells: Min height 110px.
  - Cell Content:
    - Top row: Day Number (e.g. `15`) + subtle status chip (`Present 8.5h`).
    - Middle: In & Out timestamps (e.g., `09:02 AM - 06:30 PM`).
    - Bottom tag: Context note (e.g., `Overtime +0.5h`, `Independence Day`, or `Casual Leave`).
  - Clicking any cell opens a **Right Slide-Over Day Details Drawer**.

---

## 3. Backend API Contract

### Unified Calendar Endpoint
* **Method**: `GET`
* **URL**: `/api/dashboard/employee/calendar?year=2026&month=9`
* **Headers**: `Authorization: Bearer {{token}}`
* **Sample Response**:
```json
{
  "success": true,
  "data": {
    "employeeId": 5,
    "employeeName": "Arjun Verma",
    "year": 2026,
    "month": 9,
    "monthName": "September",
    "summary": {
      "totalDays": 30,
      "workingDays": 22,
      "presentDays": 20,
      "absentDays": 0,
      "lateArrivals": 1,
      "leaveDays": 2,
      "holidays": 1,
      "weeklyOffDays": 4,
      "totalWorkHours": 172.5,
      "totalOvertimeHours": 3.5
    },
    "days": [
      {
        "date": "2026-09-01",
        "dayOfMonth": 1,
        "dayOfWeek": "Tuesday",
        "status": "Present",
        "isWorkingDay": true,
        "shiftName": "Standard Morning",
        "checkInTime": "2026-09-01T09:02:00Z",
        "checkOutTime": "2026-09-01T18:32:00Z",
        "workDurationHours": 9.5,
        "isLate": false,
        "overtimeHours": 0.5,
        "remarks": "Worked on sprint deliverables"
      },
      {
        "date": "2026-09-06",
        "dayOfMonth": 6,
        "dayOfWeek": "Sunday",
        "status": "WeeklyOff",
        "isWorkingDay": false,
        "remarks": "Scheduled Weekly Off"
      },
      {
        "date": "2026-09-15",
        "dayOfMonth": 15,
        "dayOfWeek": "Tuesday",
        "status": "Holiday",
        "isWorkingDay": false,
        "holidayName": "Engineers Day",
        "remarks": "Public Holiday"
      },
      {
        "date": "2026-09-21",
        "dayOfMonth": 21,
        "dayOfWeek": "Monday",
        "status": "OnLeave",
        "isWorkingDay": false,
        "leaveTypeName": "Annual Privilege Leave",
        "leaveStatus": "Approved",
        "remarks": "Family vacation"
      }
    ]
  }
}
```

---

## 4. Master Prompt for AI UI Generators

```text
Create a Keka-style Unified Monthly Attendance Calendar page in React + Tailwind CSS.
Features & Layout:
1. Header Bar:
   - Left: Breadcrumb "Attendance > My Attendance Calendar" + Title "Monthly Attendance & Schedule".
   - Center: Month Navigator with Prev/Next buttons, month label "September 2026", and a "Today" quick button.
   - Right: View switcher pills: [Calendar View (active)] and [List View].
2. Metric Banner Strip:
   - A clean horizontal card showing 8 key monthly aggregates:
     Total Days (30) | Working Days (22) | Present (20) | Absent (0) | Late (1) | Leaves (2) | Holidays (1) | Total Hours (172.5h).
   - Each metric has a subtle colored icon and bold numeral.
3. Legend Bar:
   - Inline legend with colored dots: Present (Green), Late (Amber), On Leave (Blue), Holiday (Purple), Weekly Off (Slate), Absent (Red).
4. Interactive 7-Column Calendar Grid:
   - Sunday to Saturday columns.
   - Each day cell:
     - Clear white card, subtle border.
     - Day number in top-left.
     - Status pill in top-right (e.g., green badge "Present • 9.5h", purple badge "Holiday", blue badge "On Leave").
     - Body snippet: Check-in/Check-out times (e.g. "09:02 AM - 06:32 PM") or Holiday title ("Engineers Day").
     - Footer snippet: e.g. "+0.5h Overtime" with flame icon.
     - On hover: Border highlights with subtle shadow.
     - On click: Opens a Right Slide-Over Panel.
5. Slide-Over Day Details Drawer (Width 450px):
   - Opens smoothly from right when a calendar day is clicked.
   - Shows:
     - Header: "Day Details - September 01, 2026 (Tuesday)".
     - Status Banner: "Present • On Time" (Emerald).
     - Timeline: Clock In: 09:02 AM • Clock Out: 06:32 PM • Total Duration: 9 hrs 30 mins.
     - Location & Terminal: "Bengaluru Innovation Hub • Web Platform".
     - Overtime Breakdown: "0.5 Hours (Standard Multiplier 1.5x)".
     - Shift Rule applied: "Standard Morning Shift (09:00 - 18:00, Grace: 15m)".
     - Close (X) button.
Aesthetic: Modern, ultra-clean Keka design, crisp Inter font, rounded-xl borders, subtle shadows.
```
