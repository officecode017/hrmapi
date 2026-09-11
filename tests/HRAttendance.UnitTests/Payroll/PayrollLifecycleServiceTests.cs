using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using HRAttendance.Business.Interfaces.Payroll;
using HRAttendance.Business.Services.Payroll;
using HRAttendance.Data;
using HRAttendance.Data.Models.Attendance;
using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Employee;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Payroll;
using Xunit;

namespace HRAttendance.UnitTests.Payroll;

public class PayrollLifecycleServiceTests
{
    private ApplicationDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task SubmitForReviewAsync_WhenCriticalExceptionsExist_ShouldThrowException()
    {
        using var context = CreateDbContext(nameof(SubmitForReviewAsync_WhenCriticalExceptionsExist_ShouldThrowException));
        var period = new PayrollPeriod
        {
            Id = 1,
            OrganizationId = 1,
            FinancialYearId = 1,
            Month = 9,
            Year = 2026,
            Status = PayrollPeriodStatus.Calculated
        };
        context.PayrollPeriods.Add(period);
        await context.SaveChangesAsync();

        var mockValidation = new Mock<IPayrollValidationService>();
        mockValidation.Setup(v => v.ValidatePeriodAsync(period.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationSummary(1, 1, 0, 0, false, new List<PayrollException>()));

        var mockProjection = new Mock<IPayslipProjectionService>();

        var service = new PayrollLifecycleService(context, mockValidation.Object, mockProjection.Object);

        // Act & Assert
        var act = () => service.SubmitForReviewAsync(period.Id, 99);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*critical exceptions exist*");
    }

    [Fact]
    public async Task ApproveAsync_WhenValid_ShouldSetStatusToApproved()
    {
        using var context = CreateDbContext(nameof(ApproveAsync_WhenValid_ShouldSetStatusToApproved));
        var period = new PayrollPeriod
        {
            Id = 2,
            OrganizationId = 1,
            FinancialYearId = 1,
            Month = 9,
            Year = 2026,
            Status = PayrollPeriodStatus.UnderReview
        };
        context.PayrollPeriods.Add(period);
        await context.SaveChangesAsync();

        var mockValidation = new Mock<IPayrollValidationService>();
        mockValidation.Setup(v => v.ValidatePeriodAsync(period.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationSummary(0, 0, 0, 0, true, new List<PayrollException>()));

        var mockProjection = new Mock<IPayslipProjectionService>();

        var service = new PayrollLifecycleService(context, mockValidation.Object, mockProjection.Object);

        // Act
        var result = await service.ApproveAsync(period.Id, 99);

        // Assert
        result.Status.Should().Be(PayrollPeriodStatus.Approved);
        result.ApprovedBy.Should().Be(99);
        result.ApprovedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task LockAsync_ShouldLockPeriod_TriggerProjection_AndStampAttendances()
    {
        using var context = CreateDbContext(nameof(LockAsync_ShouldLockPeriod_TriggerProjection_AndStampAttendances));
        var period = new PayrollPeriod
        {
            Id = 3,
            OrganizationId = 1,
            FinancialYearId = 1,
            Month = 9,
            Year = 2026,
            StartDate = new DateOnly(2026, 9, 1),
            EndDate = new DateOnly(2026, 9, 30),
            Status = PayrollPeriodStatus.Approved
        };

        var att = new EmployeeAttendance
        {
            Id = 101,
            OrganizationId = 1,
            EmployeeId = 5,
            InTime = new DateTimeOffset(2026, 9, 15, 9, 0, 0, TimeSpan.Zero),
            IsProcessedForPayroll = false
        };

        var adj = new PayrollAdjustment
        {
            Id = 201,
            AdjustmentNumber = "ADJ-001",
            OrganizationId = 1,
            EmployeeId = 5,
            PayrollPeriodId = period.Id,
            Amount = 500,
            Reason = "Bonus",
            Status = AdjustmentStatus.Approved
        };

        context.PayrollPeriods.Add(period);
        context.EmployeeAttendances.Add(att);
        context.PayrollAdjustments.Add(adj);
        await context.SaveChangesAsync();

        var mockValidation = new Mock<IPayrollValidationService>();
        var mockProjection = new Mock<IPayslipProjectionService>();
        mockProjection.Setup(p => p.ProjectPayslipsForPeriodAsync(period.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Payslip>());

        var service = new PayrollLifecycleService(context, mockValidation.Object, mockProjection.Object);

        // Act
        var result = await service.LockAsync(period.Id, 88);

        // Assert
        result.Status.Should().Be(PayrollPeriodStatus.Locked);
        result.LockedBy.Should().Be(88);
        result.LockedAt.Should().NotBeNull();

        mockProjection.Verify(p => p.ProjectPayslipsForPeriodAsync(period.Id, It.IsAny<CancellationToken>()), Times.Once);

        var updatedAtt = await context.EmployeeAttendances.FindAsync(att.Id);
        updatedAtt!.IsProcessedForPayroll.Should().BeTrue();

        var updatedAdj = await context.PayrollAdjustments.FindAsync(adj.Id);
        updatedAdj!.Status.Should().Be(AdjustmentStatus.Applied);
    }

    [Fact]
    public async Task ReopenAsync_WhenLocked_ShouldUnmarkAttendanceAndRevertAdjustments()
    {
        using var context = CreateDbContext(nameof(ReopenAsync_WhenLocked_ShouldUnmarkAttendanceAndRevertAdjustments));
        var period = new PayrollPeriod
        {
            Id = 4,
            OrganizationId = 1,
            FinancialYearId = 1,
            Month = 9,
            Year = 2026,
            StartDate = new DateOnly(2026, 9, 1),
            EndDate = new DateOnly(2026, 9, 30),
            Status = PayrollPeriodStatus.Locked,
            LockedBy = 88,
            LockedAt = DateTimeOffset.UtcNow
        };

        var att = new EmployeeAttendance
        {
            Id = 102,
            OrganizationId = 1,
            EmployeeId = 5,
            InTime = new DateTimeOffset(2026, 9, 15, 9, 0, 0, TimeSpan.Zero),
            IsProcessedForPayroll = true
        };

        var adj = new PayrollAdjustment
        {
            Id = 202,
            AdjustmentNumber = "ADJ-002",
            OrganizationId = 1,
            EmployeeId = 5,
            PayrollPeriodId = period.Id,
            Amount = 500,
            Reason = "Bonus",
            Status = AdjustmentStatus.Applied
        };

        context.PayrollPeriods.Add(period);
        context.EmployeeAttendances.Add(att);
        context.PayrollAdjustments.Add(adj);
        await context.SaveChangesAsync();

        var mockValidation = new Mock<IPayrollValidationService>();
        var mockProjection = new Mock<IPayslipProjectionService>();

        var service = new PayrollLifecycleService(context, mockValidation.Object, mockProjection.Object);

        // Act
        var result = await service.ReopenAsync(period.Id, 88);

        // Assert
        result.Status.Should().Be(PayrollPeriodStatus.Draft);
        result.LockedBy.Should().BeNull();

        var updatedAtt = await context.EmployeeAttendances.FindAsync(att.Id);
        updatedAtt!.IsProcessedForPayroll.Should().BeFalse();

        var updatedAdj = await context.PayrollAdjustments.FindAsync(adj.Id);
        updatedAdj!.Status.Should().Be(AdjustmentStatus.Approved);
    }
}
