using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Security;
using HRAttendance.Data.Models.Employee;
using HRAttendance.Data.Models.Attendance;
using HRAttendance.Data.Models.Leave;
using HRAttendance.Data.Models.Overtime;
using HRAttendance.Data.Models.Notification;
using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Organization Setup
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<OffDay> OffDays => Set<OffDay>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Designation> Designations => Set<Designation>();
    public DbSet<Shift> Shifts => Set<Shift>();

    // Security
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<EmployeeRole> EmployeeRoles => Set<EmployeeRole>();
    public DbSet<PermissionMaster> PermissionMasters => Set<PermissionMaster>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    // Employee
    public DbSet<Models.Employee.Employee> Employees => Set<Models.Employee.Employee>();
    public DbSet<EmployeeContactDetails> EmployeeContactDetails => Set<EmployeeContactDetails>();
    public DbSet<EmployeeProfessionalDetails> EmployeeProfessionalDetails => Set<EmployeeProfessionalDetails>();

    // Attendance
    public DbSet<EmployeeAttendance> EmployeeAttendances => Set<EmployeeAttendance>();
    public DbSet<AttendanceRegularizationRequest> AttendanceRegularizationRequests
        => Set<AttendanceRegularizationRequest>();

    // Leave
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveSetting> LeaveSettings => Set<LeaveSetting>();
    public DbSet<LeaveApplication> LeaveApplications => Set<LeaveApplication>();
    public DbSet<SubLeaveApplication> SubLeaveApplications => Set<SubLeaveApplication>();
    public DbSet<EmployeeLeave> EmployeeLeaves => Set<EmployeeLeave>();

    // Overtime
    public DbSet<OTSetting> OTSettings => Set<OTSetting>();
    public DbSet<OTEntry> OTEntries => Set<OTEntry>();

    // Notifications
    public DbSet<Models.Notification.Notification> Notifications => Set<Models.Notification.Notification>();

    // Payroll
    public DbSet<FinancialYear> FinancialYears => Set<FinancialYear>();
    public DbSet<PayrollPolicy> PayrollPolicies => Set<PayrollPolicy>();
    public DbSet<StatutoryRule> StatutoryRules => Set<StatutoryRule>();
    public DbSet<SalaryComponent> SalaryComponents => Set<SalaryComponent>();
    public DbSet<EmployeeSalaryStructure> EmployeeSalaryStructures => Set<EmployeeSalaryStructure>();
    public DbSet<EmployeeSalaryStructureItem> EmployeeSalaryStructureItems => Set<EmployeeSalaryStructureItem>();
    public DbSet<PayrollPeriod> PayrollPeriods => Set<PayrollPeriod>();
    public DbSet<PayrollEmployee> PayrollEmployees => Set<PayrollEmployee>();
    public DbSet<PayrollSalarySlice> PayrollSalarySlices => Set<PayrollSalarySlice>();
    public DbSet<PayrollItem> PayrollItems => Set<PayrollItem>();
    public DbSet<PayrollAdjustment> PayrollAdjustments => Set<PayrollAdjustment>();
    public DbSet<PayrollArrear> PayrollArrears => Set<PayrollArrear>();
    public DbSet<PayrollException> PayrollExceptions => Set<PayrollException>();
    public DbSet<Payslip> Payslips => Set<Payslip>();
    public DbSet<PayslipItem> PayslipItems => Set<PayslipItem>();
    public DbSet<PayslipAccessLog> PayslipAccessLogs => Set<PayslipAccessLog>();
    public DbSet<PayrollPayment> PayrollPayments => Set<PayrollPayment>();
    public DbSet<BankExportBatch> BankExportBatches => Set<BankExportBatch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filter for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "p");
                var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                var falseConstant = Expression.Constant(false);
                var compareExpression = Expression.Equal(property, falseConstant);
                var lambda = Expression.Lambda(compareExpression, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt ??= now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedAt = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<ISoftDelete>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
