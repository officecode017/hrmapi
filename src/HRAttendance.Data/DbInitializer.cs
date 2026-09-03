using Microsoft.EntityFrameworkCore;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Security;
using HRAttendance.Data.Models.Employee;
using HRAttendance.Data.Models.Leave;
using HRAttendance.Data.Models.Overtime;

namespace HRAttendance.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context, Func<string, string> passwordHasher)
    {
        // Ensure database exists
        await context.Database.MigrateAsync();

        // 1. Organization
        if (await context.Organizations.AnyAsync()) return; // Already seeded

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

        // 2. Academic Year
        var academicYear = new AcademicYear
        {
            OrganizationId = org.Id,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 12, 31),
            IsActive = true
        };
        await context.AcademicYears.AddAsync(academicYear);

        // 3. Location
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

        // 4. Shift
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

        // 5. Departments
        var deptEngineering = new Department { OrganizationId = org.Id, Name = "Engineering" };
        var deptHr = new Department { OrganizationId = org.Id, Name = "Human Resources" };
        var deptFinance = new Department { OrganizationId = org.Id, Name = "Finance" };
        await context.Departments.AddRangeAsync(deptEngineering, deptHr, deptFinance);

        // 6. Designations
        var desigArchitect = new Designation { OrganizationId = org.Id, Name = "Lead Architect" };
        var desigHrManager = new Designation { OrganizationId = org.Id, Name = "HR Manager" };
        var desigEngineer = new Designation { OrganizationId = org.Id, Name = "Senior Software Engineer" };
        await context.Designations.AddRangeAsync(desigArchitect, desigHrManager, desigEngineer);

        // 7. Security Roles
        var roleSuperAdmin = new Role { OrganizationId = org.Id, Name = "Super Admin", Description = "Full system administration", IsActive = true };
        var roleAdmin = new Role { OrganizationId = org.Id, Name = "HR/Admin", Description = "HR operations and management", IsActive = true };
        var roleManager = new Role { OrganizationId = org.Id, Name = "Manager", Description = "Team supervision and approvals", IsActive = true };
        var roleEmployee = new Role { OrganizationId = org.Id, Name = "Employee", Description = "Self-service terminal access", IsActive = true };
        await context.Roles.AddRangeAsync(roleSuperAdmin, roleAdmin, roleManager, roleEmployee);

        // 8. Leave Types & Settings
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

        // 9. Overtime Setting
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

        // 10. Default Super Admin Employee
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

        // 11. Demo Regular Employee
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

        // 12. Seed Employee Leave Entitlements
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
}
