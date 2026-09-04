# UI Prompt 02: Employee Dashboard & Quick Punch Terminal

## 1. Overview & Purpose
The everyday home dashboard for employees. Offers one-click GPS/Web check-in and check-out, live work duration counter, current shift schedule, month-to-date attendance KPIs, leave quota summary, and upcoming company holidays.

---

## 2. Keka-Inspired Visual Architecture
* **Top Greeting & Context Header**:
  - Greeting: *"Good Morning, Arjun Verma 👋"* with dynamic time-based greeting.
  - Employee Subtitle: *"Staff Software Architect • Product Engineering • Bengaluru Campus"*.
  - Right Header Badge: Active Shift pill: `Shift: Standard Morning (09:00 - 18:00) | Grace: 15m`.
* **Two-Column Main Layout**:
  - **Left Column (380px fixed)**: Sticky **"Web Clock-In / Terminal"** card.
  - **Right Column (Fluid remaining width)**:
    - Row 1: **4-KPI Metric Stat Cards** (Attendance Rate, Present Days, Late Count, Overtime Hours).
    - Row 2: **Leave Balances Horizontal Carousel / Grid** with quick "Apply Leave" action.
    - Row 3: **Split Grid (60/40)**:
      - Left: Recent Punch Log Timeline (last 7 days with in/out timestamps, hours, and status pill).
      - Right: Upcoming Holidays Card (next 60 days with colorful date badges).

---

## 3. Backend API Contracts

### A. Load Dashboard Data
* **Method**: `GET`
* **URL**: `/api/dashboard/employee`
* **Headers**: `Authorization: Bearer {{token}}`
* **Sample Response**:
```json
{
  "success": true,
  "data": {
    "employeeId": 5,
    "employeeCode": "EMP15316",
    "fullName": "Arjun Verma",
    "designationName": "Staff Software Architect",
    "departmentName": "Product Engineering",
    "locationName": "Bengaluru Innovation Hub",
    "shift": {
      "name": "Standard Morning Shift",
      "inTime": "09:00:00",
      "outTime": "18:00:00",
      "graceMinutes": 15,
      "breakMinutes": 60
    },
    "todayAttendance": {
      "status": "CheckedIn",
      "inTime": "2026-09-04T09:05:00Z",
      "outTime": null,
      "hoursWorked": 3.75,
      "isLate": false,
      "locationName": "Bengaluru Innovation Hub"
    },
    "monthlyStats": {
      "year": 2026,
      "month": 9,
      "daysPresent": 4,
      "daysAbsent": 0,
      "lateArrivalsCount": 0,
      "totalOvertimeHours": 2.50,
      "attendanceRatePercentage": 100.0
    },
    "leaveBalances": [
      { "leaveTypeId": 1, "leaveTypeName": "Casual Leave", "credited": 12, "taken": 2, "available": 10 },
      { "leaveTypeId": 2, "leaveTypeName": "Annual Privilege Leave", "credited": 20, "taken": 3, "available": 17 }
    ],
    "upcomingHolidays": [
      { "id": 1, "name": "Independence Day", "date": "2026-08-15", "dayOfWeek": "Saturday" }
    ],
    "recentPunches": [
      { "date": "2026-09-04", "inTime": "2026-09-04T09:05:00Z", "status": "Active", "durationHours": 3.75 }
    ]
  }
}
```

### B. Punch In (Check-In)
* **Method**: `POST`
* **URL**: `/api/attendance/check-in`
* **Payload**:
```json
{
  "employeeId": 5,
  "locationId": 2,
  "latitude": 12.92792,
  "longitude": 77.68331,
  "macId": "00:1B:44:11:3A:B7",
  "inNetworkSource": 1,
  "inPlatform": 1,
  "appVersion": "1.0.0"
}
```

### C. Punch Out (Check-Out)
* **Method**: `POST`
* **URL**: `/api/attendance/check-out`
* **Payload**:
```json
{
  "employeeId": 5,
  "latitude": 12.92792,
  "longitude": 77.68331,
  "macId": "00:1B:44:11:3A:B7",
  "outNetworkSource": 1,
  "outPlatform": 1,
  "remark": "Completed sprint backlog items"
}
```

---

## 4. Master Prompt for AI UI Generators

```text
Build a high-fidelity Employee Dashboard page modeled after Keka HRMS.
Theme & Layout:
- Clean off-white canvas (#F8FAFC) with pure white (#FFFFFF) elevated cards, 1px subtle borders (#E2E8F0), 12px rounded corners, and crisp Inter typography.
- Top Header:
  - Personalized greeting: "Hello, Arjun Verma 👋"
  - Subtitle: "Staff Software Architect • Product Engineering • Bengaluru Hub"
  - Current Shift badge: "Morning General (09:00 - 18:00) • Grace: 15 mins" (soft indigo pill #EEF2FF text #4F46E5).
- Left Sidebar Terminal (Width 360px):
  - Card Title: "Web Clock-In Terminal" with live digital clock (HH:MM:SS AM/PM) and pulsing green dot.
  - Status display: Big badge "You are Clocked In" (emerald green) or "Not Clocked In" (amber).
  - Time elapsed counter: e.g. "Working for: 04 hrs 12 mins".
  - Geofence info: Pin icon with text "Bengaluru Campus (GPS Verified • 500m radius)".
  - Action Button:
    - When not checked in: Large purple/indigo button with LogIn icon and text "Web Clock-In".
    - When checked in: Large danger-outline/red button with LogOut icon, a text input for "Checkout Remarks", and text "Web Clock-Out".
- Right Main Area:
  1. KPI Stat Cards (4 columns):
     - Card 1: "Attendance Rate" -> "96.4%" (with mini circular progress ring, emerald icon).
     - Card 2: "Days Present" -> "18 Days" (with calendar check icon, blue theme).
     - Card 3: "Late Arrivals" -> "1 Time" (with clock alert icon, amber theme).
     - Card 4: "Overtime Logged" -> "3.5 Hours" (with flame/timer icon, purple theme).
  2. Leave Balances Section:
     - Header: "My Leave Balances" with right-aligned button "+ Apply Leave".
     - 3 Cards: "Casual Leave (10/12 Available)", "Sick Leave (6/8 Available)", "Annual Privilege Leave (17/20 Available)". Each card has a subtle progress bar showing used vs available quota.
  3. Two-Column Bottom Widgets:
     - Widget A: "Recent Punch History" (Table showing Date, Check-In, Check-Out, Duration, and Status Pill: Present/Late).
     - Widget B: "Upcoming Holidays" (List showing Date block with month/day, holiday name like "Gandhi Jayanti", and "Mandatory/Optional" tag).
Interactive behaviors:
- On clicking "Web Clock-In", simulate a loading spinner, verify GPS, update the terminal to "Clocked In", and trigger a success notification toast.
```
