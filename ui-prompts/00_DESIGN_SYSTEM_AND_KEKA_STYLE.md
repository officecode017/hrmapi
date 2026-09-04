# Keka HRMS Design System & Styling Guidelines
> Master Design Specification for all HRAttendance Frontend Screens.

When building or generating UI for any screen in this collection, you must strictly follow this Keka-inspired enterprise design system.

---

## 1. Visual Theme & Philosophy (Keka HRMS Look & Feel)

Keka is renowned for its **clean, approachable, high-density enterprise SaaS** look:
* **White & Soft Slate Canvas**: Crisp white backgrounds (`#FFFFFF`) with ultra-subtle off-white app canvassing (`#F8FAFC` / `#F1F5F9`).
* **High Contrast & Sharp Typography**: Modern sans-serif (Inter or Outfit), deep slate text (`#0F172A`) for high readability.
* **Vibrant Brand Accents**:
  - Primary Brand Purple/Indigo: `#4F46E5` (Hover: `#4338CA`, Active: `#3730A3`)
  - Secondary Brand Teal/Cyan: `#06B6D4`
* **Status Badges & Semantic Colors**:
  - **Present / Approved**: Soft green pill background `#ECFDF5`, border `#A7F3D0`, text `#065F46`
  - **Absent / Rejected / Overdue**: Soft red pill background `#FEF2F2`, border `#FECACA`, text `#991B1B`
  - **Pending / On Hold / Grace**: Soft amber pill background `#FFFBEB`, border `#FDE68A`, text `#92400E`
  - **Holiday / Off-Day / Special**: Soft purple pill background `#F5F3FF`, border `#DDD6FE`, text `#5B21B6`
  - **Info / Neutral**: Soft blue/slate background `#F0F9FF`, text `#0369A1`

---

## 2. Global Shell & Layout Structure

Every screen (except Auth) operates within a **3-tier application shell**:

```
+---------------------------------------------------------------------------------------+
|  LOGO  | Top Bar: Org Selector [Acme Corp v] | Quick Search (Ctrl+K) | Help | Bell | Avatar |
+--------+------------------------------------------------------------------------------+
| SIDE   | BREADCRUMB: Home > Module > Current Page                    [Action Button] |
| BAR    |------------------------------------------------------------------------------|
|        | [Tab 1: Summary]  [Tab 2: History]  [Tab 3: Requests]                       |
| Icons  |------------------------------------------------------------------------------|
| +      |                                                                              |
| Labels | MAIN CONTENT CANVAS:                                                         |
|        | - KPI Summary Cards (4-column grid)                                          |
| (Coll- | - Left: Primary Panel (Table / Calendar / Terminal)                          |
| apsible| - Right: Secondary Widget Panel (Activity Feed / Policy / Quick Actions)    |
| 240px) |                                                                              |
+--------+------------------------------------------------------------------------------+
```

### Key Elements:
1. **Sidebar Navigation (240px wide)**:
   - Deep slate or clean white with active item highlighted in brand indigo (`#EEF2FF` background with `#4F46E5` left indicator bar).
   - Icons: Feather/Lucide icons (20px).
   - Section headers: Uppercase, 11px, bold, muted (`#94A3B8`).
2. **Top Navigation Bar (60px high)**:
   - Sticky top, white with 1px border bottom (`#E2E8F0`).
   - Shows organization selector dropdown, search input, notification bell with red badge counter, and user profile chip with photo & role.
3. **Card Panels**:
   - `background: #FFFFFF`, `border: 1px solid #E2E8F0`, `border-radius: 12px`, `box-shadow: 0 1px 3px rgba(0,0,0,0.04)`.
4. **Slide-Over Drawers (Sheet Modals)**:
   - Keka uses **Right Slide-over Panels (520px - 640px wide)** for creation forms (e.g., Apply Leave, Onboard Employee, Record Overtime) rather than intrusive popup modals. This preserves context behind the form.

---

## 3. Standard UI Component Library

* **Buttons**:
  - `btn-primary`: Solid indigo background, white text, 8px radius, bold, subtle shadow.
  - `btn-secondary`: White background, 1px slate border `#CBD5E1`, text `#334155`.
  - `btn-danger`: Soft red `#EF4444` or outline red.
* **Data Tables**:
  - Table header: Light slate `#F8FAFC`, uppercase 12px, font-weight 600, color `#64748B`.
  - Table rows: Height 56px, hover `#F8FAFC`, transition smooth.
  - Action column: 3-dot dropdown or quick hover action buttons (`Approve`, `Reject`, `View`).
* **Calendar Cells**:
  - 7-column day grid (Sun–Sat or Mon–Sun).
  - Cell height: 96px min.
  - Distinct colored corner indicators and attendance status pills (`Present 9h`, `Half Day`, `Holiday`).

---

## 4. How to Use the Prompts in this Directory

Feed each markdown prompt into your AI UI generator (such as v0.dev, Cursor, Windsurf, Claude Artifacts, or frontend scaffolding agents). Every prompt includes:
1. **Screen Meta & User Story**
2. **Visual Layout & Keka UX Architecture**
3. **Exact Backend API Integration & JSON Mappings**
4. **Interactive States & Micro-interactions**
5. **Self-contained Master Prompt for One-Click Generation**
