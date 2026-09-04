using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using HRAttendance.Business.BusinessRules;
using HRAttendance.Business.Services;
using HRAttendance.Data;
using HRAttendance.Data.DTOs.Leave;
using HRAttendance.Data.Repositories;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Employee;
using HRAttendance.Data.Models.Leave;
using Xunit;

namespace HRAttendance.UnitTests.Services;

public class LeaveServiceTests
{
    private ApplicationDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task ApplyLeaveAsync_WhenOverlappingLeaveExists_ShouldFail()
    {
        using var context = CreateDbContext(nameof(ApplyLeaveAsync_WhenOverlappingLeaveExists_ShouldFail));
        var employee = new Employee { Id = 1, OrganizationId = 1, EmployeeCode = "EMP001", FirstName = "John", LastName = "Doe" };
        var academicYear = new AcademicYear { Id = 1, OrganizationId = 1, StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 12, 31), IsActive = true };
        var leaveType = new LeaveType { Id = 1, OrganizationId = 1, Name = "Casual Leave", IsActive = true };

        var existingApp = new LeaveApplication
        {
            Id = 1,
            OrganizationId = 1,
            EmployeeId = 1,
            LeaveTypeId = 1,
            LeaveFrom = new DateTimeOffset(2026, 6, 10, 0, 0, 0, TimeSpan.Zero),
            LeaveTo = new DateTimeOffset(2026, 6, 12, 0, 0, 0, TimeSpan.Zero),
            NoOfLeave = 3,
            Status = LeaveStatus.Approved
        };

        await context.Employees.AddAsync(employee);
        await context.AcademicYears.AddAsync(academicYear);
        await context.LeaveTypes.AddAsync(leaveType);
        await context.LeaveApplications.AddAsync(existingApp);
        await context.SaveChangesAsync();

        var repo = new LeaveRepository(context);
        var logger = new Mock<ILogger<LeaveService>>();
        var service = new LeaveService(repo, context, logger.Object);

        // Act: try to apply with overlapping dates (June 11 - June 15)
        var result = await service.ApplyLeaveAsync(new CreateLeaveApplicationDto
        {
            EmployeeId = 1,
            LeaveTypeId = 1,
            LeaveFrom = new DateTimeOffset(2026, 6, 11, 0, 0, 0, TimeSpan.Zero),
            LeaveTo = new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero),
            NoOfLeave = 5
        });

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("overlapping");
    }

    [Fact]
    public async Task CancelLeaveAsync_WhenApproved_ShouldRefundBalanceAndMarkCancelled()
    {
        using var context = CreateDbContext(nameof(CancelLeaveAsync_WhenApproved_ShouldRefundBalanceAndMarkCancelled));
        var employee = new Employee { Id = 2, OrganizationId = 1, EmployeeCode = "EMP002", FirstName = "Jane", LastName = "Doe" };
        var academicYear = new AcademicYear { Id = 1, OrganizationId = 1, StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 12, 31), IsActive = true };
        var leaveType = new LeaveType { Id = 1, OrganizationId = 1, Name = "Annual Leave", IsActive = true };

        var balance = new EmployeeLeave
        {
            Id = 1,
            OrganizationId = 1,
            EmployeeId = 2,
            LeaveTypeId = 1,
            AcademicYearId = 1,
            LeaveCredited = 15,
            LeavesTaken = 3
        };

        var app = new LeaveApplication
        {
            Id = 10,
            OrganizationId = 1,
            EmployeeId = 2,
            LeaveTypeId = 1,
            LeaveFrom = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero),
            LeaveTo = new DateTimeOffset(2026, 7, 3, 0, 0, 0, TimeSpan.Zero),
            NoOfLeave = 3,
            Status = LeaveStatus.Approved
        };

        await context.Employees.AddAsync(employee);
        await context.AcademicYears.AddAsync(academicYear);
        await context.LeaveTypes.AddAsync(leaveType);
        await context.EmployeeLeaves.AddAsync(balance);
        await context.LeaveApplications.AddAsync(app);
        await context.SaveChangesAsync();

        var repo = new LeaveRepository(context);
        var logger = new Mock<ILogger<LeaveService>>();
        var service = new LeaveService(repo, context, logger.Object);

        // Act
        var result = await service.CancelLeaveAsync(10, 2);

        // Assert
        result.Success.Should().BeTrue();
        var updatedApp = await context.LeaveApplications.FirstOrDefaultAsync(a => a.Id == 10);
        updatedApp!.Status.Should().Be("Cancelled");

        var updatedBalance = await context.EmployeeLeaves.FirstOrDefaultAsync(b => b.EmployeeId == 2);
        updatedBalance!.LeavesTaken.Should().Be(0); // 3 - 3 = 0
    }
}
