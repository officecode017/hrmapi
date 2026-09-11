using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using HRAttendance.Data;
using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Payroll;
using HRAttendance.Data.Models.Employee;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Payroll;
using Xunit;

namespace HRAttendance.IntegrationTests.Controllers;

public class PayrollPaymentsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public PayrollPaymentsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(int orgId, int paymentId)> SeedTestPaymentAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var org = new Organization
        {
            Id = 20,
            Name = "Payment Test Org"
        };
        if (!context.Organizations.Any(o => o.Id == 20))
            context.Organizations.Add(org);

        var emp = new Employee
        {
            Id = 20,
            OrganizationId = 20,
            EmployeeCode = "EMP020",
            FirstName = "Charlie",
            LastName = "Brown"
        };
        if (!context.Employees.Any(e => e.Id == 20))
            context.Employees.Add(emp);

        var fy = new FinancialYear
        {
            Id = 20,
            OrganizationId = 20,
            YearCode = "2026-27",
            StartDate = new DateOnly(2026, 4, 1),
            EndDate = new DateOnly(2027, 3, 31),
            IsActive = true
        };
        if (!context.FinancialYears.Any(f => f.Id == 20))
            context.FinancialYears.Add(fy);

        var period = new PayrollPeriod
        {
            Id = 20,
            OrganizationId = 20,
            FinancialYearId = 20,
            Month = 9,
            Year = 2026,
            Status = PayrollPeriodStatus.Approved
        };
        if (!context.PayrollPeriods.Any(p => p.Id == 20))
            context.PayrollPeriods.Add(period);

        var pe = new PayrollEmployee
        {
            Id = 20,
            OrganizationId = 20,
            PayrollPeriodId = 20,
            EmployeeId = 20,
            NetPay = 42000m,
            GrossEarnings = 50000m,
            TotalDeductions = 8000m,
            Employee = emp
        };
        if (!context.PayrollEmployees.Any(p => p.Id == 20))
            context.PayrollEmployees.Add(pe);

        var payment = new PayrollPayment
        {
            Id = 20,
            OrganizationId = 20,
            PayrollEmployeeId = 20,
            PaymentAttemptNumber = 1,
            IdempotencyKey = "IDEMP-20-1",
            Amount = 42000m,
            Status = PaymentStatus.Processing,
            Method = PaymentMethod.BankTransfer,
            BankName = "State Bank",
            MaskedAccountNumber = "XXXX-XXXX-1234",
            IFSCCode = "SBIN0001234",
            PayrollEmployee = pe
        };
        if (!context.PayrollPayments.Any(p => p.Id == 20))
            context.PayrollPayments.Add(payment);

        await context.SaveChangesAsync();
        return (20, 20);
    }

    [Fact]
    public async Task GetPaymentById_WhenExists_ReturnsOkWithDetails()
    {
        var (orgId, paymentId) = await SeedTestPaymentAsync();

        var token = _factory.CreateAdminToken(userId: 1, organizationId: orgId);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/payroll/payments/{paymentId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<ApiResponseDto<PayrollPaymentDto>>();
        dto.Should().NotBeNull();
        dto!.Success.Should().BeTrue();
        dto.Data.Should().NotBeNull();
        dto.Data!.Amount.Should().Be(42000m);
        dto.Data.Status.Should().Be(PaymentStatus.Processing);
        dto.Data.EmployeeName.Should().Contain("Charlie");
    }

    [Fact]
    public async Task RecordPaymentResult_WhenSuccessful_UpdatesPayment()
    {
        var (orgId, paymentId) = await SeedTestPaymentAsync();

        var token = _factory.CreateAdminToken(userId: 1, organizationId: orgId);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new RecordPaymentResultRequestDto
        {
            IsSuccess = true,
            TransactionReference = "UTR-2026-999888"
        };

        var response = await client.PostAsJsonAsync($"/api/payroll/payments/{paymentId}/record-result", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify status transitioned to Paid
        var getResponse = await client.GetAsync($"/api/payroll/payments/{paymentId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await getResponse.Content.ReadFromJsonAsync<ApiResponseDto<PayrollPaymentDto>>();
        dto!.Data!.Status.Should().Be(PaymentStatus.Paid);
        dto.Data.TransactionReference.Should().Be("UTR-2026-999888");
    }
}
