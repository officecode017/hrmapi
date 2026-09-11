using Microsoft.EntityFrameworkCore;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Security;
using HRAttendance.Data.Models.Employee;
using HRAttendance.Data.Models.Leave;
using HRAttendance.Data.Models.Overtime;
using HRAttendance.Data.Models.Payroll;
using HRAttendance.Data.Models.Attendance;

namespace HRAttendance.Data;

public static class DbInitializer
{
    private static readonly (string Name, string Category, string Description)[] AllMasterPermissions = new[]
    {
        // Organization Masters
        ("Organization.View", "Organization", "View organization profile, settings, and hierarchy"),
        ("Organization.Manage", "Organization", "Update organization legal identity, currency, and fiscal settings"),
        ("Organization.Update", "Organization", "Update organization profile and branding logo"),
        ("Location.View", "Organization", "View campus locations and geofence coordinates"),
        ("Location.Manage", "Organization", "Create, update, and configure campus locations and geofences"),
        ("Location.Create", "Organization", "Create new campus locations"),
        ("Location.Edit", "Organization", "Edit campus locations"),
        ("Location.Delete", "Organization", "Delete or deactivate campus locations"),
        ("Location.CaptureGPS", "Organization", "Capture device GPS to automatically populate campus coordinates"),
        ("Department.View", "Organization", "View organizational departmental units"),
        ("Department.Manage", "Organization", "Create, update, and delete departments and head assignments"),
        ("Department.Create", "Organization", "Create new departmental units"),
        ("Department.Edit", "Organization", "Edit department details"),
        ("Department.Delete", "Organization", "Remove or deactivate departments"),
        ("Designation.View", "Organization", "View job designations and career bands"),
        ("Designation.Manage", "Organization", "Create, update, and delete job designations"),
        ("Designation.Create", "Organization", "Create new job designations"),
        ("Designation.Edit", "Organization", "Edit job designations"),
        ("Designation.Delete", "Organization", "Remove or deactivate job designations"),
        ("AcademicYear.View", "Organization", "View academic and fiscal operational cycles"),
        ("AcademicYear.Manage", "Organization", "Create, update, and activate academic cycles"),

        // Employee Directory
        ("Employee.View", "Employees", "View employee directory and organizational profiles"),
        ("Employee.Create", "Employees", "Onboard new employees with complete profile"),
        ("Employee.Edit", "Employees", "Edit administrative, salary, and job profile of any employee"),
        ("Employee.Update", "Employees", "Edit and update employee profile details and media"),
        ("Employee.Delete", "Employees", "Deactivate or terminate employee records"),
        ("Employee.Manage", "Employees", "Full employee directory administration and management"),
        ("Employee.ViewSelf", "Employees", "View own employee profile, documents, and compensation overview"),
        ("Employee.EditSelf", "Employees", "Self-service edit for personal contact details and emergency contacts"),

        // Attendance & WebClock
        ("Attendance.View", "Attendance", "View personal attendance logs and punches"),
        ("Attendance.ViewAll", "Attendance", "View organization-wide employee attendance and biometric punches"),
        ("Attendance.Punch", "Attendance", "Perform web clock-in and clock-out"),
        ("Attendance.ManualPunch", "Attendance", "Submit manual attendance punch adjustment requests"),
        ("Attendance.Manage", "Attendance", "Administratively edit and regularize attendance records"),
        ("Attendance.Regularize", "Attendance", "Submit and approve attendance regularization requests"),

        // Leaves & Policies
        ("Leave.Apply", "Leaves", "Submit leave applications and time-off requests"),
        ("Leave.View", "Leaves", "View own leave quota balances and application history"),
        ("Leave.ViewAll", "Leaves", "View all department and organization leave applications"),
        ("Leave.Approve", "Leaves", "Approve employee leave applications"),
        ("Leave.Reject", "Leaves", "Reject employee leave applications"),
        ("Leave.Cancel", "Leaves", "Cancel own or approved employee leave applications"),
        ("Leave.ManageTypes", "Leaves", "Configure leave categories, quotas, and accrual rules"),
        ("Leave.Configure", "Leaves", "Configure annual leave balances, policy entitlements, and carry forwards"),
        ("Leave.Adjust", "Leaves", "Manually adjust employee leave balances with audit history"),

        // Overtime Operations
        ("Overtime.View", "Overtime", "View overtime logs and compensatory multiplier policies"),
        ("Overtime.Request", "Overtime", "Request and submit overtime claims"),
        ("Overtime.Approve", "Overtime", "Approve employee overtime claims"),
        ("Overtime.Reject", "Overtime", "Reject employee overtime claims"),
        ("Overtime.Manage", "Overtime", "Configure overtime rules, daily caps, and rate multipliers"),

        // Shifts & Schedules
        ("Shift.View", "Shifts", "View shift templates and roster schedules"),
        ("Shift.Create", "Shifts", "Create custom work shifts and grace periods"),
        ("Shift.Edit", "Shifts", "Edit shift start/end times and break configurations"),
        ("Shift.Update", "Shifts", "Update shift details and roster assignments"),
        ("Shift.Delete", "Shifts", "Remove or deactivate shift templates"),
        ("Shift.Assign", "Shifts", "Assign work shifts to individual employees or entire departments"),

        // Holidays & OffDays
        ("Holiday.View", "Holidays", "View organization holidays and declared observances"),
        ("Holiday.Manage", "Holidays", "Create, edit, and delete organization and campus holidays"),
        ("OffDay.View", "Holidays", "View weekly off-day policies and weekend schedules"),
        ("OffDay.Manage", "Holidays", "Create and manage role-wise weekend policies and recurrence rules"),

        // Security & RBAC
        ("Role.View", "Roles", "View security roles and access privileges"),
        ("Role.Create", "Roles", "Create new security roles"),
        ("Role.Edit", "Roles", "Edit security role names and descriptions"),
        ("Role.Update", "Roles", "Update security role configurations"),
        ("Role.Delete", "Roles", "Remove custom security roles"),
        ("Role.Assign", "Roles", "Assign roles to employees"),
        ("Permission.View", "Roles", "View available permissions across modules"),
        ("Permission.Manage", "Roles", "Configure and assign permissions to system roles"),

        // Notifications
        ("Notification.View", "Notifications", "View personal system notifications and alerts"),
        ("Notification.Manage", "Notifications", "Broadcast organizational announcements and notifications"),

        // Payroll & Compensation Masters
        ("Payroll.View", "Payroll", "View payroll periods, payslips, and salary structures"),
        ("Payroll.Manage", "Payroll", "Manage salary components, payroll policies, and statutory rules"),
        ("Payroll.Calculate", "Payroll", "Initiate and recalculate employee payroll batches"),
        ("Payroll.Approve", "Payroll", "Approve and authorize processed payroll batches"),
        ("Payroll.Lock", "Payroll", "Lock and freeze finalized payroll periods for disbursement"),
        ("Payroll.Export", "Payroll", "Generate bank disbursement files and statutory reports")
    };

    private static readonly HashSet<string> ManagerPermissionNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Organization.View", "Location.View", "Department.View", "Designation.View", "AcademicYear.View",
        "Employee.View", "Employee.Update", "Employee.Edit", "Employee.ViewSelf", "Employee.EditSelf",
        "Attendance.View", "Attendance.ViewAll", "Attendance.Punch", "Attendance.ManualPunch", "Attendance.Regularize",
        "Leave.Apply", "Leave.View", "Leave.ViewAll", "Leave.Approve", "Leave.Reject", "Leave.Cancel",
        "Overtime.View", "Overtime.Request", "Overtime.Approve", "Overtime.Reject",
        "Shift.View", "Shift.Assign",
        "Holiday.View", "OffDay.View",
        "Notification.View", "Payroll.View"
    };

    private static readonly HashSet<string> EmployeePermissionNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Employee.ViewSelf", "Employee.EditSelf",
        "Attendance.View", "Attendance.Punch", "Attendance.ManualPunch",
        "Leave.Apply", "Leave.View",
        "Overtime.View", "Overtime.Request",
        "Shift.View",
        "Holiday.View", "OffDay.View",
        "Notification.View", "Payroll.View"
    };

    public static async Task SeedAsync(ApplicationDbContext context, Func<string, string> passwordHasher)
    {
        // Ensure database exists and latest migrations applied
        await context.Database.MigrateAsync();

        // 1. Initial Organization and core entities (only if database is brand new)
        if (!await context.Organizations.AnyAsync())
        {
            await SeedInitialOrganizationAndEntitiesAsync(context, passwordHasher);
        }

        // 2. Master Permissions & Role-Permission Mappings (idempotent: always feeds missing permissions)
        await SeedPermissionsAndRolesAsync(context);

        // 3. Payroll Masters, Financial Years, Salary Components & Structures (idempotent)
        await SeedPayrollMastersAsync(context);

        // 4. Heal any legacy or uninitialized PayrollPeriod records
        await HealPayrollPeriodsAsync(context);
    }

    public static async Task SeedPermissionsAndRolesAsync(ApplicationDbContext context)
    {
        var organizations = await context.Organizations.ToListAsync();
        if (!organizations.Any()) return;

        foreach (var org in organizations)
        {
            // 1. Ensure all master permissions exist
            var existingPerms = await context.PermissionMasters
                .Where(p => p.OrganizationId == org.Id)
                .ToListAsync();

            var existingPermNames = existingPerms.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var newPerms = new List<PermissionMaster>();
            foreach (var pDef in AllMasterPermissions)
            {
                if (!existingPermNames.Contains(pDef.Name))
                {
                    var newPerm = new PermissionMaster
                    {
                        OrganizationId = org.Id,
                        Name = pDef.Name,
                        Category = pDef.Category,
                        Description = pDef.Description
                    };
                    newPerms.Add(newPerm);
                    context.PermissionMasters.Add(newPerm);
                }
            }

            if (newPerms.Any())
            {
                await context.SaveChangesAsync();
                existingPerms.AddRange(newPerms);
            }

            var permMap = existingPerms.ToDictionary(p => p.Name, p => p.Id, StringComparer.OrdinalIgnoreCase);

            // 2. Ensure default system roles exist
            var roles = await context.Roles
                .Where(r => r.OrganizationId == org.Id)
                .ToListAsync();

            var roleSuperAdmin = roles.FirstOrDefault(r => r.Name.Equals("Super Admin", StringComparison.OrdinalIgnoreCase))
                ?? CreateRole(context, org.Id, "Super Admin", "Full system administration");
            var roleAdmin = roles.FirstOrDefault(r => r.Name.Equals("HR/Admin", StringComparison.OrdinalIgnoreCase))
                ?? CreateRole(context, org.Id, "HR/Admin", "HR operations and management");
            var roleManager = roles.FirstOrDefault(r => r.Name.Equals("Manager", StringComparison.OrdinalIgnoreCase))
                ?? CreateRole(context, org.Id, "Manager", "Team supervision and approvals");
            var roleEmployee = roles.FirstOrDefault(r => r.Name.Equals("Employee", StringComparison.OrdinalIgnoreCase))
                ?? CreateRole(context, org.Id, "Employee", "Self-service terminal access");

            await context.SaveChangesAsync();

            // 3. Map permissions to roles
            // Super Admin & HR/Admin get ALL permissions
            await EnsureRolePermissionsAsync(context, org.Id, roleSuperAdmin.Id, permMap.Values);
            await EnsureRolePermissionsAsync(context, org.Id, roleAdmin.Id, permMap.Values);

            // Manager permissions
            var managerPermIds = permMap
                .Where(kvp => ManagerPermissionNames.Contains(kvp.Key))
                .Select(kvp => kvp.Value);
            await EnsureRolePermissionsAsync(context, org.Id, roleManager.Id, managerPermIds);

            // Employee self-service permissions
            var employeePermIds = permMap
                .Where(kvp => EmployeePermissionNames.Contains(kvp.Key))
                .Select(kvp => kvp.Value);
            await EnsureRolePermissionsAsync(context, org.Id, roleEmployee.Id, employeePermIds);
        }

        await context.SaveChangesAsync();
    }

    private static Role CreateRole(ApplicationDbContext context, int orgId, string name, string description)
    {
        var role = new Role
        {
            OrganizationId = orgId,
            Name = name,
            Description = description,
            IsActive = true
        };
        context.Roles.Add(role);
        return role;
    }

    private static async Task EnsureRolePermissionsAsync(
        ApplicationDbContext context,
        int organizationId,
        int roleId,
        IEnumerable<int> targetPermissionIds)
    {
        var existingRolePerms = await context.RolePermissions
            .IgnoreQueryFilters()
            .Where(rp => rp.OrganizationId == organizationId && rp.RoleId == roleId)
            .ToListAsync();

        var existingPermIds = existingRolePerms.ToDictionary(rp => rp.PermissionId);

        foreach (var permId in targetPermissionIds)
        {
            if (!existingPermIds.TryGetValue(permId, out var existingRp))
            {
                context.RolePermissions.Add(new RolePermission
                {
                    OrganizationId = organizationId,
                    RoleId = roleId,
                    PermissionId = permId,
                    IsDeleted = false
                });
            }
            else if (existingRp.IsDeleted)
            {
                existingRp.IsDeleted = false;
            }
        }
    }

    private static async Task SeedInitialOrganizationAndEntitiesAsync(ApplicationDbContext context, Func<string, string> passwordHasher)
    {
        var org = new Organization
        {
            Name = "Acme Global Enterprises",
            Phone = "+1 (555) 019-2834",
            Email = "contact@acmeglobal.com",
            Website = "https://acmeglobal.com",
            Country = "United States",
            State = "California",
            City = "San Francisco",
            PostalCode = "94105",
            Industry = "Information Technology"
        };
        await context.Organizations.AddAsync(org);
        await context.SaveChangesAsync();

        // Academic Year
        var academicYear = new AcademicYear
        {
            OrganizationId = org.Id,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 12, 31),
            IsActive = true
        };
        await context.AcademicYears.AddAsync(academicYear);

        // Location
        var location = new Location
        {
            OrganizationId = org.Id,
            Name = "HQ Main Campus",
            Country = "United States",
            EmailAlias = "hq@acmeglobal.com",
            Latitude = 37.774929m,
            Longitude = -122.419418m,
            Radius = 500,
            TimeZoneValue = "Pacific Standard Time"
        };
        await context.Locations.AddAsync(location);
        await context.SaveChangesAsync();

        // Shift
        var shift = new Shift
        {
            OrganizationId = org.Id,
            LocationId = location.Id,
            Name = "General Morning Shift",
            InTime = new TimeOnly(9, 0),
            OutTime = new TimeOnly(18, 0),
            GraceMinutes = 15,
            BreakMinutes = 60,
            BreakStartTime = new TimeOnly(13, 0),
            BreakEndTime = new TimeOnly(14, 0)
        };
        await context.Shifts.AddAsync(shift);

        // Departments
        var deptEngineering = new Department { OrganizationId = org.Id, Name = "Engineering" };
        var deptHr = new Department { OrganizationId = org.Id, Name = "Human Resources" };
        var deptFinance = new Department { OrganizationId = org.Id, Name = "Finance" };
        await context.Departments.AddRangeAsync(deptEngineering, deptHr, deptFinance);

        // Designations
        var desigArchitect = new Designation { OrganizationId = org.Id, Name = "Lead Architect" };
        var desigHrManager = new Designation { OrganizationId = org.Id, Name = "HR Manager" };
        var desigEngineer = new Designation { OrganizationId = org.Id, Name = "Senior Software Engineer" };
        await context.Designations.AddRangeAsync(desigArchitect, desigHrManager, desigEngineer);

        // Security Roles
        var roleSuperAdmin = new Role { OrganizationId = org.Id, Name = "Super Admin", Description = "Full system administration", IsActive = true };
        var roleAdmin = new Role { OrganizationId = org.Id, Name = "HR/Admin", Description = "HR operations and management", IsActive = true };
        var roleManager = new Role { OrganizationId = org.Id, Name = "Manager", Description = "Team supervision and approvals", IsActive = true };
        var roleEmployee = new Role { OrganizationId = org.Id, Name = "Employee", Description = "Self-service terminal access", IsActive = true };
        await context.Roles.AddRangeAsync(roleSuperAdmin, roleAdmin, roleManager, roleEmployee);

        // Leave Types & Settings
        var paidLeave = new LeaveType
        {
            OrganizationId = org.Id,
            Name = "Annual / Paid Leave",
            Description = "Standard annual vacation entitlement",
            IsActive = true,
            LeaveSetting = new LeaveSetting
            {
                OrganizationId = org.Id,
                IsPaid = true,
                CanTakeHalfDay = true,
                Leaves = 18.00m,
                CarryForwardLeaveCount = 5.00m
            }
        };

        var sickLeave = new LeaveType
        {
            OrganizationId = org.Id,
            Name = "Sick Leave",
            Description = "Medical and recovery leave",
            IsActive = true,
            LeaveSetting = new LeaveSetting
            {
                OrganizationId = org.Id,
                IsPaid = true,
                CanTakeHalfDay = true,
                Leaves = 12.00m
            }
        };

        var casualLeave = new LeaveType
        {
            OrganizationId = org.Id,
            Name = "Casual Leave",
            Description = "Short notice urgent personal leave",
            IsActive = true,
            LeaveSetting = new LeaveSetting
            {
                OrganizationId = org.Id,
                IsPaid = true,
                CanTakeHalfDay = true,
                Leaves = 8.00m
            }
        };

        await context.LeaveTypes.AddRangeAsync(paidLeave, sickLeave, casualLeave);

        // Overtime Setting
        var otSetting = new OTSetting
        {
            OrganizationId = org.Id,
            Name = "Standard Daily Overtime",
            IsOverTimeEnabled = true,
            OTStartAfterMinutes = 30,
            Multiplier = 1.50m,
            MaxOTHoursPerDay = 4.00m,
            IsActive = true
        };
        await context.OTSettings.AddAsync(otSetting);

        await context.SaveChangesAsync();

        // Default Super Admin Employee
        var adminEmp = new Employee
        {
            OrganizationId = org.Id,
            EmployeeCode = "ADMIN001",
            FirstName = "System",
            LastName = "Administrator",
            IsActive = true,
            PasswordHash = passwordHasher("Password@123"),
            ContactDetails = new EmployeeContactDetails
            {
                OrganizationId = org.Id,
                WorkEmail = "admin@hrm.local",
                Mobile = "+1 (555) 010-0001",
                City = "San Francisco",
                Country = "United States"
            },
            ProfessionalDetails = new EmployeeProfessionalDetails
            {
                OrganizationId = org.Id,
                DepartmentId = deptHr.Id,
                DesignationId = desigHrManager.Id,
                LocationId = location.Id,
                ShiftId = shift.Id,
                DateOfJoining = new DateOnly(2025, 1, 1)
            }
        };
        adminEmp.EmployeeRoles.Add(new EmployeeRole { OrganizationId = org.Id, RoleId = roleSuperAdmin.Id });
        adminEmp.EmployeeRoles.Add(new EmployeeRole { OrganizationId = org.Id, RoleId = roleAdmin.Id });

        // Demo Regular Employee
        var regularEmp = new Employee
        {
            OrganizationId = org.Id,
            EmployeeCode = "EMP001",
            FirstName = "Sarah",
            LastName = "Jenkins",
            IsActive = true,
            PasswordHash = passwordHasher("Password@123"),
            ContactDetails = new EmployeeContactDetails
            {
                OrganizationId = org.Id,
                WorkEmail = "employee@hrm.local",
                Mobile = "+1 (555) 010-0002",
                City = "San Francisco",
                Country = "United States"
            },
            ProfessionalDetails = new EmployeeProfessionalDetails
            {
                OrganizationId = org.Id,
                DepartmentId = deptEngineering.Id,
                DesignationId = desigArchitect.Id,
                LocationId = location.Id,
                ShiftId = shift.Id,
                DateOfJoining = new DateOnly(2025, 3, 1)
            }
        };
        regularEmp.EmployeeRoles.Add(new EmployeeRole { OrganizationId = org.Id, RoleId = roleEmployee.Id });

        await context.Employees.AddRangeAsync(adminEmp, regularEmp);
        await context.SaveChangesAsync();

        // Seed Employee Leave Entitlements
        await context.EmployeeLeaves.AddRangeAsync(
            new EmployeeLeave
            {
                OrganizationId = org.Id,
                EmployeeId = regularEmp.Id,
                LeaveTypeId = paidLeave.Id,
                AcademicYearId = academicYear.Id,
                LeaveCredited = 18.00m,
                LeaveBroughtForward = 2.00m,
                LeavesTaken = 4.00m
            },
            new EmployeeLeave
            {
                OrganizationId = org.Id,
                EmployeeId = regularEmp.Id,
                LeaveTypeId = sickLeave.Id,
                AcademicYearId = academicYear.Id,
                LeaveCredited = 12.00m,
                LeaveBroughtForward = 0.00m,
                LeavesTaken = 1.00m
            },
            new EmployeeLeave
            {
                OrganizationId = org.Id,
                EmployeeId = regularEmp.Id,
                LeaveTypeId = casualLeave.Id,
                AcademicYearId = academicYear.Id,
                LeaveCredited = 8.00m,
                LeaveBroughtForward = 0.00m,
                LeavesTaken = 2.00m
            }
        );

        await context.SaveChangesAsync();
    }

    public static async Task SeedPayrollMastersAsync(ApplicationDbContext context)
    {
        var organizations = await context.Organizations.ToListAsync();
        if (!organizations.Any()) return;

        foreach (var org in organizations)
        {
            // 1. Ensure Financial Year exists
            var activeFY = await context.FinancialYears
                .FirstOrDefaultAsync(fy => fy.OrganizationId == org.Id && fy.YearCode == "FY 2026-27");

            if (activeFY == null)
            {
                activeFY = new FinancialYear
                {
                    OrganizationId = org.Id,
                    YearCode = "FY 2026-27",
                    StartDate = new DateOnly(2026, 4, 1),
                    EndDate = new DateOnly(2027, 3, 31),
                    IsActive = true
                };
                context.FinancialYears.Add(activeFY);
                await context.SaveChangesAsync();
            }

            // 2. Ensure standard Salary Components exist
            var standardComponents = new (string Code, string Name, ComponentType Type, ComponentCalculationType CalcType, int Order, bool IsTaxable, bool IsStatutory)[]
            {
                ("BASIC", "Basic Salary", ComponentType.Earning, ComponentCalculationType.PercentageOfCTC, 1, true, false),
                ("HRA", "House Rent Allowance", ComponentType.Earning, ComponentCalculationType.PercentageOfBasic, 2, true, false),
                ("SA", "Special Allowance", ComponentType.Earning, ComponentCalculationType.FixedAmount, 3, true, false),
                ("CA", "Conveyance Allowance", ComponentType.Earning, ComponentCalculationType.FixedAmount, 4, true, false),
                ("MA", "Medical Allowance", ComponentType.Earning, ComponentCalculationType.FixedAmount, 5, true, false),
                ("PF_EE", "Provident Fund (Employee)", ComponentType.Deduction, ComponentCalculationType.PercentageOfBasic, 6, false, true),
                ("PF_ER", "Provident Fund (Employer)", ComponentType.EmployerContribution, ComponentCalculationType.PercentageOfBasic, 7, false, true),
                ("PT", "Professional Tax", ComponentType.Deduction, ComponentCalculationType.FixedAmount, 8, false, true),
                ("TDS", "Income Tax / TDS", ComponentType.Deduction, ComponentCalculationType.ManualAmount, 9, false, true),
                ("ESI_EE", "ESI (Employee)", ComponentType.Deduction, ComponentCalculationType.PercentageOfGross, 10, false, true),
                ("ESI_ER", "ESI (Employer)", ComponentType.EmployerContribution, ComponentCalculationType.PercentageOfGross, 11, false, true)
            };

            var existingComponents = await context.SalaryComponents
                .Where(c => c.OrganizationId == org.Id)
                .ToListAsync();

            var compMap = existingComponents.ToDictionary(c => c.Code, StringComparer.OrdinalIgnoreCase);
            var newComponentsAdded = false;

            foreach (var sc in standardComponents)
            {
                if (!compMap.TryGetValue(sc.Code, out var comp))
                {
                    comp = new SalaryComponent
                    {
                        OrganizationId = org.Id,
                        Code = sc.Code,
                        Name = sc.Name,
                        Type = sc.Type,
                        CalculationType = sc.CalcType,
                        CalculationOrder = sc.Order,
                        IsTaxable = sc.IsTaxable,
                        IsStatutory = sc.IsStatutory,
                        IsActive = true
                    };
                    context.SalaryComponents.Add(comp);
                    compMap[sc.Code] = comp;
                    newComponentsAdded = true;
                }
            }

            if (newComponentsAdded)
            {
                await context.SaveChangesAsync();
            }

            // 3. Ensure Statutory Rules exist
            if (!await context.StatutoryRules.AnyAsync(r => r.OrganizationId == org.Id && r.RuleType == StatutoryRuleType.ProvidentFund))
            {
                context.StatutoryRules.Add(new StatutoryRule
                {
                    OrganizationId = org.Id,
                    RuleType = StatutoryRuleType.ProvidentFund,
                    Version = 1,
                    EffectiveFrom = new DateOnly(2026, 4, 1),
                    ConfigurationJson = "{\"EmployeeRate\":0.12,\"EmployerEPSRate\":0.0833,\"EmployerEPFRate\":0.0367,\"WageCeiling\":15000,\"EnforceWageCeiling\":true}",
                    IsActive = true
                });
            }

            if (!await context.StatutoryRules.AnyAsync(r => r.OrganizationId == org.Id && r.RuleType == StatutoryRuleType.ESI))
            {
                context.StatutoryRules.Add(new StatutoryRule
                {
                    OrganizationId = org.Id,
                    RuleType = StatutoryRuleType.ESI,
                    Version = 1,
                    EffectiveFrom = new DateOnly(2026, 4, 1),
                    ConfigurationJson = "{\"EmployeeRate\":0.0075,\"EmployerRate\":0.0325,\"GrossWageLimit\":21000}",
                    IsActive = true
                });
            }

            if (!await context.StatutoryRules.AnyAsync(r => r.OrganizationId == org.Id && r.RuleType == StatutoryRuleType.ProfessionalTax))
            {
                context.StatutoryRules.Add(new StatutoryRule
                {
                    OrganizationId = org.Id,
                    RuleType = StatutoryRuleType.ProfessionalTax,
                    Version = 1,
                    EffectiveFrom = new DateOnly(2026, 4, 1),
                    ConfigurationJson = "{\"StateCode\":\"MH\",\"Brackets\":[{\"MinGross\":0,\"MaxGross\":7500,\"MonthlyTax\":0},{\"MinGross\":7501,\"MaxGross\":10000,\"MonthlyTax\":175},{\"MinGross\":10001,\"MaxGross\":999999999,\"MonthlyTax\":200,\"FebruaryTax\":300}]}",
                    IsActive = true
                });
            }

            // 4. Ensure Payroll Policy exists
            if (!await context.PayrollPolicies.AnyAsync(p => p.OrganizationId == org.Id))
            {
                context.PayrollPolicies.Add(new PayrollPolicy
                {
                    OrganizationId = org.Id,
                    ProrationBasis = SalaryProrationBasis.ActualCalendarDays,
                    FixedProrationDays = 30,
                    LOPBasis = LOPCalculationBasis.CalendarDays,
                    FixedLOPDays = 30,
                    OTBasis = OvertimeBasis.BasicSalary,
                    OTMultiplier = 1.5m,
                    StandardMonthlyWorkingHours = 160m,
                    RoundingRule = RoundingRule.TwoDecimals,
                    ConsiderHolidaysInLOP = false,
                    ConsiderWeekendsInLOP = false
                });
            }

            await context.SaveChangesAsync();

            // 5. Ensure Employees have valid salary structures
            var employees = await context.Employees
                .Include(e => e.ContactDetails)
                .Include(e => e.ProfessionalDetails)
                .Include(e => e.SalaryStructures.Where(s => s.IsActive))
                .Where(e => e.OrganizationId == org.Id && e.IsActive)
                .ToListAsync();

            var basicComp = compMap["BASIC"];
            var hraComp = compMap["HRA"];
            var saComp = compMap["SA"];

            foreach (var emp in employees)
            {
                // Ensure employee has state set for PT
                if (emp.ContactDetails != null && string.IsNullOrWhiteSpace(emp.ContactDetails.State))
                {
                    emp.ContactDetails.State = "MH";
                }

                if (!emp.SalaryStructures.Any())
                {
                    // Assign default salary structure
                    decimal monthlyGross = emp.EmployeeCode == "ADMIN001" ? 80000m : 50000m;
                    decimal annualCtc = monthlyGross * 12m;
                    decimal basicAmt = monthlyGross * 0.50m;
                    decimal hraAmt = monthlyGross * 0.20m;
                    decimal saAmt = monthlyGross - basicAmt - hraAmt;

                    var structure = new EmployeeSalaryStructure
                    {
                        OrganizationId = org.Id,
                        EmployeeId = emp.Id,
                        Version = 1,
                        EffectiveFrom = new DateOnly(2026, 1, 1),
                        MonthlyGrossSalary = monthlyGross,
                        AnnualCTC = annualCtc,
                        RevisionReason = "Initial Compensation Package",
                        IsActive = true,
                        Items = new List<EmployeeSalaryStructureItem>
                        {
                            new()
                            {
                                SalaryComponentId = basicComp.Id,
                                MonthlyAmount = basicAmt,
                                AnnualAmount = basicAmt * 12m,
                                PercentageRate = 50.0m
                            },
                            new()
                            {
                                SalaryComponentId = hraComp.Id,
                                MonthlyAmount = hraAmt,
                                AnnualAmount = hraAmt * 12m,
                                PercentageRate = 20.0m
                            },
                            new()
                            {
                                SalaryComponentId = saComp.Id,
                                MonthlyAmount = saAmt,
                                AnnualAmount = saAmt * 12m,
                                PercentageRate = 30.0m
                            }
                        }
                    };

                    context.EmployeeSalaryStructures.Add(structure);
                }
            }

            await context.SaveChangesAsync();

            // 6. Seed sample attendance for current active employees (September 2026)
            var academicYear = await context.AcademicYears.FirstOrDefaultAsync(ay => ay.OrganizationId == org.Id && ay.IsActive);
            if (academicYear != null)
            {
                var sampleMonth = 9;
                var sampleYear = 2026;
                var startPeriod = new DateTimeOffset(new DateTime(sampleYear, sampleMonth, 1, 0, 0, 0, DateTimeKind.Utc));
                var endPeriod = new DateTimeOffset(new DateTime(sampleYear, sampleMonth, 30, 23, 59, 59, DateTimeKind.Utc));

                foreach (var emp in employees)
                {
                    var existingPunches = await context.EmployeeAttendances
                        .AnyAsync(a => a.EmployeeId == emp.Id && a.InTime >= startPeriod && a.InTime <= endPeriod);

                    if (!existingPunches && emp.ProfessionalDetails != null && emp.ProfessionalDetails.LocationId.HasValue && emp.ProfessionalDetails.ShiftId.HasValue)
                    {
                        var attendanceList = new List<EmployeeAttendance>();
                        for (int day = 1; day <= 25; day++)
                        {
                            var punchDate = new DateTime(sampleYear, sampleMonth, day);
                            if (punchDate.DayOfWeek == DayOfWeek.Saturday || punchDate.DayOfWeek == DayOfWeek.Sunday)
                            {
                                continue;
                            }

                            attendanceList.Add(new EmployeeAttendance
                            {
                                OrganizationId = org.Id,
                                EmployeeId = emp.Id,
                                LocationId = emp.ProfessionalDetails.LocationId.Value,
                                ShiftId = emp.ProfessionalDetails.ShiftId.Value,
                                AcademicYearId = academicYear.Id,
                                InTime = new DateTimeOffset(punchDate.AddHours(9), TimeSpan.Zero),
                                OutTime = new DateTimeOffset(punchDate.AddHours(18), TimeSpan.Zero),
                                DayTotal = 9.0m,
                                Status = 1, // Present
                                PunchCount = 2
                            });
                        }

                        if (attendanceList.Any())
                        {
                            await context.EmployeeAttendances.AddRangeAsync(attendanceList);
                        }
                    }
                }
                await context.SaveChangesAsync();
            }
        }
    }

    private static async Task HealPayrollPeriodsAsync(ApplicationDbContext context)
    {
        var periods = await context.PayrollPeriods.ToListAsync();
        bool modified = false;
        foreach (var period in periods)
        {
            if (period.StartDate.Year <= 1)
            {
                period.StartDate = new DateOnly(period.Year, period.Month, 1);
                modified = true;
            }
            if (period.EndDate.Year <= 1)
            {
                period.EndDate = new DateOnly(period.Year, period.Month, DateTime.DaysInMonth(period.Year, period.Month));
                modified = true;
            }
            if (period.Status == PayrollPeriodStatus.Calculating)
            {
                period.Status = PayrollPeriodStatus.Draft;
                modified = true;
            }
        }
        if (modified)
        {
            await context.SaveChangesAsync();
        }
    }
}
