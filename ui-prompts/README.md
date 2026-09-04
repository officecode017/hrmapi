# HRAttendance UI Screen Prompts Collection (Keka HRMS Theme)

A comprehensive, standardized collection of high-fidelity UI design prompts tailored specifically for the **HRAttendance** .NET 10 API ecosystem. Every screen prompt is designed to deliver the polished, modern, and human-centric enterprise look and feel of **Keka HRMS**.

---

## 🎨 Design System & Visual Identity
All prompts in this repository adhere to the master design rules specified in [`00_DESIGN_SYSTEM_AND_KEKA_STYLE.md`](00_DESIGN_SYSTEM_AND_KEKA_STYLE.md):
- **Base Canvas**: Soft Off-White / Slate `#F8FAFC`, Pristine White Cards `#FFFFFF`.
- **Primary Accent**: Deep Indigo `#4F46E5` with `#4338CA` hover states.
- **Semantic Accents**: Emerald Green `#10B981` (Present / Approved), Amber `#F59E0B` (Late / Pending), Rose Red `#EF4444` (Absent / Rejected), Slate `#64748B` (Weekly Offs).
- **Typography**: Crisp `Inter` or `Plus Jakarta Sans` with tight heading tracking.
- **Micro-Interactions**: Right slide-over drawers (450px-540px), sticky action bars, 4-metric KPI summary grids, live ticking clock badges, and geofence radar visualizers.

---

## 📂 Screen Prompts Directory

| File | Screen / Feature | Target Audience | Primary API Endpoints |
| :--- | :--- | :--- | :--- |
| [`00_DESIGN_SYSTEM_AND_KEKA_STYLE.md`](00_DESIGN_SYSTEM_AND_KEKA_STYLE.md) | **Master Design System** | Designers & AI Models | Global Tokens, Typography, Palette, Layout Shell |
| [`01_LOGIN_SCREEN.md`](01_LOGIN_SCREEN.md) | **Authentication & Login** | All Users | `POST /api/auth/login` |
| [`02_EMPLOYEE_DASHBOARD.md`](02_EMPLOYEE_DASHBOARD.md) | **Employee Self-Service Dashboard** | Employees | `GET /api/dashboard/employee`, `POST /api/attendance/check-in`, `POST /api/attendance/check-out` |
| [`03_EMPLOYEE_ATTENDANCE_CALENDAR.md`](03_EMPLOYEE_ATTENDANCE_CALENDAR.md) | **Unified Attendance Calendar** | Employees & Managers | `GET /api/dashboard/employee/calendar?year={y}&month={m}` |
| [`04_LEAVE_PORTAL_AND_APPLICATIONS.md`](04_LEAVE_PORTAL_AND_APPLICATIONS.md) | **Leave Portal & Apply Center** | Employees | `GET /api/leave/balances`, `GET /api/leave/my-leaves/{id}`, `POST /api/leave`, `POST /api/leave/{id}/cancel` |
| [`05_LEAVE_APPROVALS_AND_BALANCES.md`](05_LEAVE_APPROVALS_AND_BALANCES.md) | **Leave Approvals & Quota Ledger** | Managers & HR Admins | `GET /api/leave/pending`, `POST /api/leave/{id}/approve`, `POST /api/leave/{id}/reject`, `POST /api/leave/balances/adjust` |
| [`06_ATTENDANCE_OPERATIONS_ADMIN.md`](06_ATTENDANCE_OPERATIONS_ADMIN.md) | **Company Attendance Operations** | HR Admins & Operations | `GET /api/attendance/history`, `GET /api/attendance/today`, `POST /api/attendance/check-in` (Admin override) |
| [`07_OVERTIME_MANAGEMENT.md`](07_OVERTIME_MANAGEMENT.md) | **Overtime Tracking & Policy** | Employees, Managers, HR | `GET /api/overtime/settings`, `PUT /api/overtime/settings/{id}`, `GET /api/overtime/pending`, `POST /api/overtime/{id}/approval`, `POST /api/overtime/record` |
| [`08_EMPLOYEE_DIRECTORY_AND_PROFILES.md`](08_EMPLOYEE_DIRECTORY_AND_PROFILES.md) | **Directory & 360° Profiles** | All Staff & HR Admins | `GET /api/employee`, `GET /api/employee/{id}`, `POST /api/employee`, `PUT /api/employee/{id}` |
| [`09_SHIFTS_AND_OFFDAY_PLANNER.md`](09_SHIFTS_AND_OFFDAY_PLANNER.md) | **Shifts & Off-Day Scheduling** | HR Admins & Planners | `GET /api/shift`, `POST /api/shift`, `POST /api/shift/assign/bulk`, `GET /api/offday`, `POST /api/offday` |
| [`10_ORGANIZATION_AND_MASTERS_SETUP.md`](10_ORGANIZATION_AND_MASTERS_SETUP.md) | **Organization Masters & Geofence** | Super Admins & HR Admins | `GET /api/organization`, `GET /api/location`, `POST /api/location`, `GET /api/department`, `GET /api/holiday`, `POST /api/role/permissions/assign` |

---

## 🚀 How to Use These Prompts with AI UI Generators

You can copy and paste the markdown files directly into your preferred AI frontend code generator:

### 1. Using with v0.dev / Lovable.dev / Bolt.new
1. Open the desired screen prompt (e.g. `02_EMPLOYEE_DASHBOARD.md`).
2. Copy the content inside **Section 4: Master Prompt for AI UI Generators**.
3. If you want maximum fidelity, also append **Section 3: Backend API Contract** so the AI generates matching TypeScript interfaces and mock API handlers.
4. Submit the prompt to generate responsive React + Tailwind CSS components.

### 2. Using with Cursor / Windsurf / GitHub Copilot
1. When generating a new component inside your frontend project, prompt your AI assistant:
   ```text
   Read the design specification in ui-prompts/03_EMPLOYEE_ATTENDANCE_CALENDAR.md and generate a production-ready Next.js / React component adhering to the Keka design tokens, layout, and API DTO interfaces defined in that document.
   ```

### 3. Using with Claude Artifacts / ChatGPT
1. Copy the entire file (Sections 1 through 4) and provide the prompt:
   ```text
   Please render an interactive single-file React component preview implementing this complete UI specification. Use Tailwind CSS and Lucide icons.
   ```

---

## 🔗 Direct API Integration Note
Every prompt includes the live JSON request/response contracts matching the .NET 10 Web API backend (`src/HRAttendance.API`). All endpoint paths, parameter names, and DTO structures correspond 1:1 with the backend controllers.
