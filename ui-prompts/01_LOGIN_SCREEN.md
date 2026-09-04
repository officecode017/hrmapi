# UI Prompt 01: Multi-Tenant Login & Authentication Screen

## 1. Overview & Purpose
The gateway to the HRAttendance portal. Allows employees and administrators to select or enter their Organization ID, sign in using their work email or employee code, and acquire a JWT bearer token.

---

## 2. Keka-Inspired Visual Design
* **Split-Screen Layout**:
  - **Left Side (50%)**: Clean, distraction-free authentication card with brand logo, enterprise badge, and login inputs.
  - **Right Side (50%)**: Vibrant branded showcase background (Indigo `#4338CA` gradient with subtle modern geometric curves, floating testimonial card: *"Streamlining workforce management for 10,000+ teams"*, and live time/clock widget).
* **Card & Form Style**:
  - Subtle floating card on mobile, integrated split on desktop.
  - Floating label inputs with modern focus rings (`ring-2 ring-indigo-500`).
  - Eye icon toggle to reveal/hide password.
  - "Remember Organization" checkbox.

---

## 3. Backend API Contract

### Primary Endpoint
* **Method**: `POST`
* **URL**: `/api/auth/login`
* **Headers**: `Content-Type: application/json`, `Accept: application/json`

### Request Payload
```json
{
  "organizationId": 1,
  "employeeCodeOrEmail": "admin@hrm.local",
  "password": "Password@123"
}
```

### Success Response (HTTP 200)
```json
{
  "success": true,
  "message": "Login successful.",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiration": "2026-09-04T09:24:48.472Z",
    "user": {
      "employeeId": 1,
      "organizationId": 1,
      "employeeCode": "ADMIN001",
      "fullName": "System Administrator",
      "email": "admin@hrm.local",
      "roles": ["Super Admin", "HR/Admin"]
    }
  },
  "errors": null
}
```

### Error Response (HTTP 400 / 401)
```json
{
  "success": false,
  "message": "Invalid email/employee code or password.",
  "data": null,
  "errors": ["Invalid credentials."]
}
```

---

## 4. Components & Interactive States
1. **Organization ID Selector/Input**:
   - Default: `1` (with helper text: *"Default organization is 1"*).
2. **Work Email / Employee Code Field**:
   - Accepts both `admin@hrm.local` or `ADMIN001`.
3. **Password Field**:
   - Masked with eye toggle.
4. **Primary CTA**:
   - `"Sign In to Portal"`.
   - Loading state: Button spinner with disabled interaction.
5. **Session Persistence**:
   - Stores `token` and `user` object in `localStorage` or secure cookie.
   - Redirects to `/dashboard`.
6. **Error Alert Banner**:
   - Red soft pill banner (`#FEF2F2`) with alert icon if credentials fail.

---

## 5. Copy-Paste Master Prompt for AI UI Generators

```text
Create a modern, enterprise B2B SaaS Login page inspired by Keka HRMS.
Aesthetics:
- Use a 50/50 split layout on desktop: Left side is crisp white with the login form; right side is a deep indigo (#4338CA to #312E81) gradient backdrop featuring an illustration of workplace productivity, floating metric cards (e.g. "99.8% On-Time Check-In"), and an employee badge graphic.
- Form components:
  1. Header: Logo with "HRAttendance" text and tagline "Next-Gen Workforce Management".
  2. Input 1: "Organization ID" (number input, defaults to 1, icon: Building2).
  3. Input 2: "Work Email or Employee Code" (text input, placeholder: "e.g., alex@company.com or EMP001", icon: Mail).
  4. Input 3: "Password" (password input with eye icon to toggle visibility, icon: Lock).
  5. Row: "Remember this device" checkbox + "Forgot password?" link.
  6. Button: Full-width brand purple/indigo button (#4F46E5) with text "Sign In to Workspace".
  7. Footer: "Protected by 256-bit enterprise encryption • Version 2.4.0".
- Interactive States:
  - Add loading spinner on button submit.
  - Display a clean, dismissible alert toast or inline banner for invalid credentials.
- Technology: HTML/TailwindCSS or React with TypeScript and Lucide icons. High contrast, sharp Inter font, 8px border radiuses, smooth micro-interactions on hover and focus.
```
