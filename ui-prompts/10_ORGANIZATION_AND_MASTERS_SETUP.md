# UI Prompt 10: Organization Settings & Master Data Administration

## 1. Overview & Purpose
The administrative command center where HR System Administrators configure core enterprise master entities: Organization Profile, Office Locations & GPS Geofencing boundaries, Departments, Designations, Academic/Fiscal Year cycles, Public Holidays calendar, and System Roles & Permissions.

---

## 2. Keka-Inspired Visual Architecture
* **Vertical / Horizontal Master Navigation Tabs**:
  - `[ 🏢 Organization Profile ]`
  - `[ 📍 Locations & Geofences ]` (Active)
  - `[ 👥 Departments & Designations ]`
  - `[ 🗓️ Academic Years ]`
  - `[ 🎉 Public Holidays ]`
  - `[ 🔐 Roles & Permissions ]`
* **Section 1: Organization Profile View**:
  - Organization Name, Legal Entity Name, Corporate Tax ID (PAN/GST/EIN), Work Week Policy, Base Currency, Timezone (`Asia/Kolkata - IST`), and Company Logo uploader.
* **Section 2: Locations & Geofence Radar**:
  - Campus Cards & Map Radar:
    - Card: `Bengaluru Innovation Hub (HQ)` • `Lat: 12.9716, Long: 77.5946` • `Geofence Radius: 150m`.
    - Geofence Visualizer: Interactive map or circular radar showing the allowed perimeter circle.
    - Toggle: `Enforce Geofence on Web & Mobile Punches (Yes/No)`.
    - Modal: Add/Edit Location with map picker, address, latitude, longitude, and radius slider (`50m` to `1000m`).
* **Section 3: Departments & Designations Directory**:
  - Split-pane layout:
    - Left: Departments list with employee counts (`Engineering - 84`, `Marketing - 18`, `HR - 6`). Quick `+ Add Department` button.
    - Right: Designations belonging to selected department (`Tech Lead`, `Senior Engineer`, `Junior Engineer`). Quick `+ Add Designation` button.
* **Section 4: Academic Years**:
  - Academic / Financial Cycle table (e.g. `2026 - 2027` Active, `2025 - 2026` Archived).
  - Start Date, End Date, and `Set as Active Cycle` toggle.
* **Section 5: Public Holidays Calendar**:
  - Chronological timeline cards:
    - Date badge: `Aug 15` (Indian Independence Day), `Oct 02` (Gandhi Jayanti), `Dec 25` (Christmas).
    - Type: `Mandatory National Holiday` vs `Optional / Restricted Holiday`.
    - `+ Add Public Holiday` modal.
* **Section 6: Roles & Permissions Matrix**:
  - Permissions matrix grid:
    - Rows: Functional modules (`Attendance`, `Leave`, `Overtime`, `Employees`, `Payroll`).
    - Columns: Roles (`Super Admin`, `Admin / HR`, `Reporting Manager`, `Employee`).
    - Interactive Checkboxes: `View`, `Create`, `Edit`, `Approve`, `Delete`.

---

## 3. Backend API Contract

### A. Location & Geofence Management
* **Method**: `GET`
* **URL**: `/api/location?organizationId=1`
* **Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "organizationId": 1,
      "name": "Bengaluru Innovation Hub",
      "address": "42 Outer Ring Road, Bellandur, Bengaluru",
      "latitude": 12.9716,
      "longitude": 77.5946,
      "radiusMeters": 200,
      "isActive": true
    }
  ]
}
```

* **Create Location**: `POST /api/location`
```json
{
  "organizationId": 1,
  "name": "Hyderabad Tech Park",
  "address": "HITEC City, Madhapur, Hyderabad",
  "latitude": 17.4435,
  "longitude": 78.3772,
  "radiusMeters": 250
}
```

### B. Public Holidays
* **Method**: `GET`
* **URL**: `/api/holiday?organizationId=1&academicYearId=1`
* **Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "organizationId": 1,
      "academicYearId": 1,
      "name": "Independence Day",
      "date": "2026-08-15",
      "description": "National Holiday",
      "isMandatory": true
    },
    {
      "id": 2,
      "organizationId": 1,
      "academicYearId": 1,
      "name": "Gandhi Jayanti",
      "date": "2026-10-02",
      "description": "National Holiday",
      "isMandatory": true
    }
  ]
}
```

### C. Departments & Designations
* **Method**: `GET`
* **URL**: `/api/department?organizationId=1`
* **URL**: `/api/designation?organizationId=1`

### D. Roles & Permissions Assignment
* **Method**: `POST`
* **URL**: `/api/role/permissions/assign`
* **Request Payload**:
```json
{
  "roleId": 3,
  "permissionIds": [1, 2, 5, 8, 12]
}
```

---

## 4. Master Prompt for AI UI Generators

```text
Create an Enterprise Organization Settings & Master Data Hub inspired by Keka HR in React, Tailwind CSS, and Lucide icons.

Layout & Core Features:
1. Header & Tab Navigation:
   - Header: "Settings > Organization & Master Data".
   - Navigation Tabs with icons:
     * [Locations & Geofence (Active)]
     * [Departments & Designations]
     * [Holidays Calendar]
     * [Academic / Fiscal Years]
     * [Roles & Permissions]

2. Locations & Geofencing View:
   - Top banner: "Office Locations & Attendance Geofence Perimeter".
   - "+ Add Campus Location" CTA button.
   - Location Cards:
     * Card Title: "Bengaluru Innovation Hub (Headquarters)".
     * Physical Address with map pin icon.
     * GPS Coordinates badge: "12.9716° N, 77.5946° E".
     * Geofence Radius Pill: "200 meters perimeter".
     * Visual Mock: An interactive or styled radar circle illustrating the geofence perimeter around the building.
     * Edit / Delete buttons.

3. Add Location Modal:
   - Location Name, Address, Latitude & Longitude inputs.
   - Interactive Slider for Geofence Radius (50m to 500m) with live meter badge.
   - Toggle: "Enforce Geofence Validation for Clock-Ins".
   - Save button.

4. Public Holidays Schedule View:
   - Filter by Academic Year dropdown.
   - List of festive holiday cards organized by quarter (Q1, Q2, Q3, Q4):
     * Date block: Month in uppercase, big day number (e.g. "AUG 15").
     * Holiday Name: "Independence Day".
     * Day of week: "Saturday".
     * Badge: "Mandatory Holiday" (Emerald) or "Optional Holiday" (Slate).
     * Action icons: Edit and Delete.

5. Roles & Permissions Matrix View:
   - Interactive table with Sticky Role Header:
     * Columns: Module Name, Super Admin, HR Admin, Manager, Employee.
     * Checkboxes for granular permissions (View Attendance, Approve Leaves, Adjust Balances, Configure Shifts).

Visual Styling:
- Pristine Keka design language: White surface cards, clean 1px borders (#E2E8F0), subtle shadow, Indigo brand accents (#4F46E5).
```
