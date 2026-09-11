using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using HRAttendance.Business.Interfaces.Payroll;
using HRAttendance.Business.Services.Payroll;
using HRAttendance.Data;
using HRAttendance.Data.Models.Employee;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Payroll;
using Xunit;

namespace HRAttendance.UnitTests.Payroll;

public class PayrollPaymentServiceTests
{
    private ApplicationDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task InitiateDisbursementAsync_WhenCalledWithSameIdempotencyKey_ShouldReturnExistingBatchWithoutCreatingDuplicates()
    {
        using var context = CreateDbContext(nameof(InitiateDisbursementAsync_WhenCalledWithSameIdempotencyKey_ShouldReturnExistingBatchWithoutCreatingDuplicates));
        var org = new Organization { Id = 1, Name = "Acme Corp" };
        var emp = new Employee { Id = 10, OrganizationId = 1, EmployeeCode = "EMP010", FirstName = "Alice", LastName = "Smith" };
        var period = new PayrollPeriod
        {
            Id = 1,
            OrganizationId = 1,
            FinancialYearId = 1,
            Month = 9,
            Year = 2026,
            Status = PayrollPeriodStatus.Locked
        };

        var pe = new PayrollEmployee
        {
            Id = 100,
            PayrollPeriodId = period.Id,
            EmployeeId = emp.Id,
            OrganizationId = 1,
            NetPay = 75000m,
            Period = period,
            Employee = emp
        };

        context.Organizations.Add(org);
        context.Employees.Add(emp);
        context.PayrollPeriods.Add(period);
        context.PayrollEmployees.Add(pe);
        await context.SaveChangesAsync();

        var service = new PayrollPaymentService(context);
        const string idempotencyKey = "DISB-202609-BATCH-01";

        // Act 1: Initial disbursement
        var firstResult = await service.InitiateDisbursementAsync(period.Id, idempotencyKey, 99);

        // Assert 1
        firstResult.TotalPaymentsInitiated.Should().Be(1);
        firstResult.TotalAmountDisbursed.Should().Be(75000m);

        var totalInDb = await context.PayrollPayments.CountAsync();
        totalInDb.Should().Be(1);

        // Act 2: Re-send with same idempotency key
        var secondResult = await service.InitiateDisbursementAsync(period.Id, idempotencyKey, 99);

        // Assert 2
        secondResult.TotalPaymentsInitiated.Should().Be(1);
        var totalInDbAfter = await context.PayrollPayments.CountAsync();
        totalInDbAfter.Should().Be(1); // No duplicates!
    }

    [Fact]
    public async Task RecordPaymentResultAsync_WhenPaymentSucceeds_ShouldMarkPaidAndCompletePeriodIfAllSettled()
    {
        using var context = CreateDbContext(nameof(RecordPaymentResultAsync_WhenPaymentSucceeds_ShouldMarkPaidAndCompletePeriodIfAllSettled));
        var org = new Organization { Id = 1, Name = "Acme Corp" };
        var emp = new Employee { Id = 11, OrganizationId = 1, EmployeeCode = "EMP011", FirstName = "Bob", LastName = "Dylan" };
        var period = new PayrollPeriod
        {
            Id = 2,
            OrganizationId = 1,
            FinancialYearId = 1,
            Month = 9,
            Year = 2026,
            Status = PayrollPeriodStatus.PaymentProcessing
        };

        var pe = new PayrollEmployee
        {
            Id = 200,
            PayrollPeriodId = period.Id,
            EmployeeId = emp.Id,
            OrganizationId = 1,
            NetPay = 60000m,
            Period = period,
            Employee = emp
        };

        var payment = new PayrollPayment
        {
            Id = 50,
            PayrollEmployeeId = pe.Id,
            OrganizationId = 1,
            Amount = 60000m,
            Status = PaymentStatus.Processing,
            IdempotencyKey = "KEY-001",
            PayrollEmployee = pe
        };

        context.Organizations.Add(org);
        context.Employees.Add(emp);
        context.PayrollPeriods.Add(period);
        context.PayrollEmployees.Add(pe);
        context.PayrollPayments.Add(payment);
        await context.SaveChangesAsync();

        var service = new PayrollPaymentService(context);

        // Act
        var result = await service.RecordPaymentResultAsync(new RecordPaymentResultDto(
            PaymentId: payment.Id,
            Success: true,
            TransactionReference: "TXN-HDFC-99998888",
            FailureReason: null
        ), 99);

        // Assert
        result.Status.Should().Be(PaymentStatus.Paid);
        result.TransactionReference.Should().Be("TXN-HDFC-99998888");
        result.PaidAt.Should().NotBeNull();

        var updatedPeriod = await context.PayrollPeriods.FindAsync(period.Id);
        updatedPeriod!.Status.Should().Be(PayrollPeriodStatus.Paid);
    }
}
