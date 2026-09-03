# HRAttendance - SaaS Advanced Attendance & HR Management System

An enterprise-ready, multi-tenant SaaS Advanced Attendance and HR Management boilerplate built as a **pragmatic 3-tier layered modular monolith** using **.NET 10**, **C#**, **Entity Framework Core**, **SQL Server**, and a **React + TypeScript + Vite** web client.

---

## Architecture Overview

This project is intentionally designed as a clean, maintainable 3-tier layered modular monolith without microservices, CQRS, MediatR, or over-engineered abstractions.

```
                    ┌────────────────────────────┐
                    │ React (Vite + TypeScript)  │
                    └─────────────┬──────────────┘
                                  │ (REST / JSON)
                                  ▼
                    ┌────────────────────────────┐
                    │     HRAttendance.API       │  (Thin Controllers, Middleware, Auth/Swagger DI, Validators)
                    └─────────────┬──────────────┘
                                  │
                                  ▼
                    ┌────────────────────────────┐
                    │   HRAttendance.Business    │  (Business Rules, Calculations, Services, DTO Mappings)
                    └─────────────┬──────────────┘
                                  │
                                  ▼
                    ┌────────────────────────────┐
                    │     HRAttendance.Data      │  (DbContext, 24 ERD Models, Configurations, Repositories, DTOs)
                    └─────────────┬──────────────┘
                                  │
                                  ▼
                    ┌────────────────────────────┐
                    │    SQL Server Database     │
                    └────────────────────────────┘
```

### Request Flow
1. **React Client**: Dispatches HTTP requests with JWT Bearer authentication headers.
2. **API Controller**: Validates DTO requests via FluentValidation, enforces `[Authorize]` role checks, returns RFC 7807 `ProblemDetails` on errors. Thin controllers do not execute SQL or contain domain calculations.
3. **Business Service**: Contains business logic, attendance rules (late arrival calculations, grace periods), leave entitlement deductions, multi-table transactions.
4. **Focused Repository / EF Core**: Executes queries with `AsNoTracking()` where appropriate and applies global soft-delete query filters (`IsDeleted == false`).
5. **SQL Server**: Persists data with referential integrity and precision constraints.

---

## Technology Stack

### Backend
- **Framework**: .NET 10 (`net10.0`)
- **Language**: C# 14
- **Web API**: ASP.NET Core Web API with Controllers
- **Data Access**: Entity Framework Core 10.0 (SQL Server provider)
- **Security**: JWT Bearer Tokens (`System.IdentityModel.Tokens.Jwt`), PBKDF2 with SHA-256 password hashing
- **Validation**: FluentValidation Auto-Validation
- **Error Handling**: Custom `GlobalExceptionMiddleware` returning RFC 7807 `ProblemDetails`
- **Documentation**: OpenAPI / Swagger UI with authenticated Bearer token execution
- **Testing**: xUnit, Moq, FluentAssertions, `Microsoft.AspNetCore.Mvc.Testing`

### Frontend
- **Framework**: React 19 + TypeScript
- **Bundler**: Vite
- **Routing**: React Router v7
- **HTTP Client**: Axios with request/response JWT interceptors
- **Icons**: Lucide React
- **Design System**: Pure Vanilla CSS with HSL variables, glassmorphism, responsive cards, and micro-animations

---

## Solution Structure

```
HRAttendance/
│
├── HRAttendance.sln
├── erd.txt
├── .gitignore
├── .env.example
├── README.md
│
├── src/
│   ├── HRAttendance.API/
│   │   ├── Controllers/             # Thin REST endpoints (Auth, Employee, Attendance, Leave, etc.)
│   │   ├── Middleware/              # Global exception middleware (RFC 7807 ProblemDetails)
│   │   ├── Extensions/              # Clean DI, Authentication, and Swagger extensions
│   │   ├── Validators/              # FluentValidation request validators
│   │   ├── Program.cs               # Pipeline and middleware orchestration
│   │   ├── appsettings.json         # Base configuration placeholders
│   │   └── appsettings.Development.json
│   │
│   ├── HRAttendance.Business/
│   │   ├── BusinessRules/           # Role definitions, attendance status enums, leave status constants
│   │   ├── Interfaces/              # Business service contracts (IAuthService, IEmployeeService, etc.)
│   │   └── Services/                # Domain implementations, calculations, transactions, hashing
│   │
│   └── HRAttendance.Data/
│       ├── Models/                  # 24 database entities matching erd.txt (Organizations, Employees, Shifts, etc.)
│       ├── Configurations/          # EF Core Fluent API configurations (decimal precisions, indexes, FKs)
│       ├── Repositories/            # Focused data access repositories (Employee, Attendance, Leave)
│       ├── Interfaces/              # Repository interfaces
│       ├── DTOs/                    # Request and response contracts (Auth, Employee, Attendance, Leave, etc.)
│       ├── Migrations/              # EF Core code-first migrations
│       └── ApplicationDbContext.cs  # DbSets, soft-delete query filters, SaveChangesAsync audit tracking
│
├── tests/
│   ├── HRAttendance.UnitTests/       # Unit tests for PasswordHasher, EmployeeService, etc.
│   └── HRAttendance.IntegrationTests/ # Integration tests with WebApplicationFactory for API endpoints
│
└── frontend/
    └── hra-attendance-web/
        ├── src/
        │   ├── api/                 # Axios client and domain API modules
        │   ├── context/             # AuthContext with JWT session and instant demo accounts
        │   ├── components/layout/   # MainLayout, Sidebar, Header
        │   ├── components/common/   # ProtectedRoute, Badges, StatCards
        │   ├── pages/               # Auth, Dashboard, Attendance, Employees, Leaves, Settings
        │   ├── routes/              # AppRoutes with role protection
        │   ├── types/               # TypeScript contracts
        │   ├── index.css            # Custom CSS design system
        │   ├── App.tsx
        │   └── main.tsx
        ├── package.json
        └── vite.config.ts
```

---

## Project Dependencies

```
HRAttendance.API
    ├── HRAttendance.Business
    └── HRAttendance.Data

HRAttendance.Business
    └── HRAttendance.Data

HRAttendance.Data
    └── EF Core / SQL Server

HRAttendance.UnitTests
    ├── HRAttendance.Business
    └── HRAttendance.Data

HRAttendance.IntegrationTests
    ├── HRAttendance.API
    ├── HRAttendance.Business
    └── HRAttendance.Data
```

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [Node.js](https://nodejs.org/) (v20+ recommended)
- [SQL Server](https://www.microsoft.com/sql-server/) (LocalDB, Express, or Azure SQL)

### 1. Database Configuration
Update the connection string in `src/HRAttendance.API/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HRAttendanceDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

### 2. JWT Configuration
Configure token parameters in `src/HRAttendance.API/appsettings.json`:
```json
"Jwt": {
  "Key": "YourSuperSecretKeyThatMustBeAtLeast32BytesLongForSecurity!",
  "Issuer": "HRAttendance",
  "Audience": "HRAttendanceApp",
  "ExpiryMinutes": 120
}
```

### 3. Database Migrations
Run EF Core migrations using the .NET CLI:
```powershell
# Add a new migration (if schema modified)
dotnet ef migrations add <MigrationName> --project src/HRAttendance.Data --startup-project src/HRAttendance.API

# Update database
dotnet ef database update --project src/HRAttendance.Data --startup-project src/HRAttendance.API
```

### 4. Running the Backend API
```powershell
# Restore and build
dotnet restore
dotnet build

# Run the API
dotnet run --project src/HRAttendance.API
```
- The API will start at: `http://localhost:5000`
- Swagger documentation UI is available at root: `http://localhost:5000/`

### 5. Running the Tests
```powershell
dotnet test
```

### 6. Running the React Frontend
```powershell
cd frontend/hra-attendance-web
npm install
npm run dev
```
- The web app will launch at: `http://localhost:5173`
- The login page includes **Quick Demo Accounts** (`Super Admin`, `HR/Admin`, `Manager`, `Employee`) allowing immediate end-to-end UI exploration without requiring database seeding.

---

## Authorization Roles

The system supports four application tiers:
1. **Super Admin**: Tenant-wide system administration, cross-organization configuration, and policy setup.
2. **HR / Admin**: Employee onboarding, department assignment, shift assignment, and leave policy management.
3. **Manager**: Direct-report supervision, attendance approvals, leave approvals, and team schedule tracking.
4. **Employee**: Self-service terminal, biometric punch recording, attendance history review, and leave application.

---

## License
MIT License.
