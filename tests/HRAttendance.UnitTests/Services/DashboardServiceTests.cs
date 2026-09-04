using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using HRAttendance.Business.Services;
using HRAttendance.Data;
using HRAttendance.Data.Models.Attendance;
using HRAttendance.Data.Models.Employee;
using HRAttendance.Data.Models.Leave;
using HRAttendance.Data.Models.Organization;
using Xunit;

namespace HRAttendance.UnitTests.Services;

public class DashboardServiceTests
{
    private ApplicationDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetEmployeeDashboardAsync_ShouldReturnEmployeeKpisAndTodayPunch()
    {
        using var context = CreateDbContext(nameof(GetEmployeeDashboardAsync_ShouldReturnEmployeeKpisAndTodayPunch));
        var logger = new Mock<ILogger<DashboardService>>();
        var service = new DashboardService(context, logger.Object);

        // Arrange
        var shift = new Shift
        {
            Id = 1,
            OrganizationId = 1,
            Name = "General Morning",
            InTime = new TimeOnly(9, 0),
            OutTime = new TimeOnly(18, 0),
            GraceMinutes = 15
        };

        var dept = new Department { Id = 1, OrganizationId = 1, Name = "Engineering" };
        var desig = new Designation { Id = 1, OrganizationId = 1, Name = "Lead Engineer" };
        var loc = new Location { Id = 1, OrganizationId = 1, Name = "HQ Campus", Country = "India", Latitude = 12.92m, Longitude = 77.68m, Radius = 500 };

        var employee = new Employee
        {
            Id = 1,
            OrganizationId = 1,
            EmployeeCode = "EMP001",
            FirstName = "John",
            LastName = "Doe",
            ProfessionalDetails = new EmployeeProfessionalDetails
            {
                OrganizationId = 1,
                EmployeeId = 1,
                Department = dept,
                Designation = desig,
                Location = loc,
                ShiftId = 1
            }
        };

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var inTime = new DateTimeOffset(today.ToDateTime(new TimeOnly(9, 5)), TimeSpan.Zero);

        var todayAttendance = new EmployeeAttendance
        {
            Id = 10,
            OrganizationId = 1,
            EmployeeId = 1,
            LocationId = 1,
            ShiftId = 1,
            AcademicYearId = 1,
            InTime = inTime,
            Status = 1
        };

        var academicYear = new AcademicYear { Id = 1, OrganizationId = 1, StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 12, 31), IsActive = true };
        var leaveType = new LeaveType { Id = 1, OrganizationId = 1, Name = "Annual Leave", IsActive = true };
        var balance = new EmployeeLeave
        {
            Id = 1,
            OrganizationId = 1,
            EmployeeId = 1,
            LeaveTypeId = 1,
            AcademicYearId = 1,
            LeaveCredited = 15,
            LeavesTaken = 2
        };

        await context.Shifts.AddAsync(shift);
        await context.Departments.AddAsync(dept);
        await context.Designations.AddAsync(desig);
        await context.Locations.AddAsync(loc);
        await context.Employees.AddAsync(employee);
        await context.EmployeeAttendances.AddAsync(todayAttendance);
        await context.AcademicYears.AddAsync(academicYear);
        await context.LeaveTypes.AddAsync(leaveType);
        await context.EmployeeLeaves.AddAsync(balance);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetEmployeeDashboardAsync(1);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.FullName.Should().Be("John Doe");
        result.Data.DepartmentName.Should().Be("Engineering");
        result.Data.DesignationName.Should().Be("Lead Engineer");
        result.Data.Shift.Should().NotBeNull();
        result.Data.Shift!.Name.Should().Be("General Morning");
        result.Data.TodayAttendance.Status.Should().Be("CheckedIn");
        result.Data.TodayAttendance.IsLate.Should().BeFalse(); // 9:05 is within 9:15 grace
        result.Data.LeaveBalances.Should().HaveCount(1);
        result.Data.LeaveBalances[0].Available.Should().Be(13); // 15 - 2
    }

    [Fact]
    public async Task GetEmployeeCalendarAsync_ShouldMergeHolidaysLeavesAndPunchesCorrectly()
    {
        using var context = CreateDbContext(nameof(GetEmployeeCalendarAsync_ShouldMergeHolidaysLeavesAndPunchesCorrectly));
        var logger = new Mock<ILogger<DashboardService>>();
        var service = new DashboardService(context, logger.Object);

        // Arrange
        var employee = new Employee
        {
            Id = 2,
            OrganizationId = 1,
            EmployeeCode = "EMP002",
            FirstName = "Alice",
            LastName = "Smith",
            ProfessionalDetails = new EmployeeProfessionalDetails
            {
                OrganizationId = 1,
                EmployeeId = 2,
                LocationId = 1
            }
        };

        // Punch on Sept 1, 2026
        var punchTime = new DateTimeOffset(new DateTime(2026, 9, 1, 9, 0, 0), TimeSpan.Zero);
        var attendance = new EmployeeAttendance
        {
            Id = 20,
            OrganizationId = 1,
            EmployeeId = 2,
            LocationId = 1,
            ShiftId = 1,
            AcademicYearId = 1,
            InTime = punchTime,
            OutTime = punchTime.AddHours(9),
            Status = 1
        };

        // Public Holiday on Sept 15, 2026
        var holiday = new Holiday
        {
            Id = 1,
            OrganizationId = 1,
            Name = "Engineers Day",
            Date = new DateOnly(2026, 9, 15),
            IsOptional = false
        };

        // Leave application on Sept 21, 2026
        var leaveType = new LeaveType { Id = 1, OrganizationId = 1, Name = "Casual Leave", IsActive = true };
        var leaveApp = new LeaveApplication
        {
            Id = 5,
            OrganizationId = 1,
            EmployeeId = 2,
            LeaveTypeId = 1,
            LeaveFrom = new DateTimeOffset(new DateTime(2026, 9, 21), TimeSpan.Zero),
            LeaveTo = new DateTimeOffset(new DateTime(2026, 9, 21), TimeSpan.Zero),
            NoOfLeave = 1,
            Status = "Approved"
        };
        var subLeave = new SubLeaveApplication
        {
            Id = 50,
            OrganizationId = 1,
            EmployeeId = 2,
            LeaveApplicationId = 5,
            LeaveDate = new DateOnly(2026, 9, 21),
            Status = "Approved",
            IsHalfDay = false,
            LeaveApplication = leaveApp
        };

        // Off-Day Rule: Sunday Off for all employees
        var offDay = new OffDay
        {
            Id = 1,
            OrganizationId = 1,
            OffDayName = "Sunday",
            Week1 = true,
            Week2 = true,
            Week3 = true,
            Week4 = true,
            Week5 = true,
            Week6 = true
        };

        await context.Employees.AddAsync(employee);
        await context.EmployeeAttendances.AddAsync(attendance);
        await context.Holidays.AddAsync(holiday);
        await context.LeaveTypes.AddAsync(leaveType);
        await context.LeaveApplications.AddAsync(leaveApp);
        await context.SubLeaveApplications.AddAsync(subLeave);
        await context.OffDays.AddAsync(offDay);
        await context.SaveChangesAsync();

        // Act - query calendar for September 2026
        var result = await service.GetEmployeeCalendarAsync(2, 2026, 9);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Days.Should().HaveCount(30);

        // Day 1 (Sept 1) -> Present
        var day1 = result.Data.Days.First(d => d.DayOfMonth == 1);
        day1.Status.Should().Be("Present");
        day1.WorkDurationHours.Should().Be(9);

        // Day 6 (Sept 6 is Sunday) -> WeeklyOff
        var day6 = result.Data.Days.First(d => d.DayOfMonth == 6);
        day6.Status.Should().Be("WeeklyOff");

        // Day 15 (Sept 15) -> Holiday
        var day15 = result.Data.Days.First(d => d.DayOfMonth == 15);
        day15.Status.Should().Be("Holiday");
        day15.HolidayName.Should().Be("Engineers Day");

        // Day 21 (Sept 21) -> OnLeave
        var day21 = result.Data.Days.First(d => d.DayOfMonth == 21);
        day21.Status.Should().Be("OnLeave");
        day21.LeaveTypeName.Should().Be("Casual Leave");
    }
}
