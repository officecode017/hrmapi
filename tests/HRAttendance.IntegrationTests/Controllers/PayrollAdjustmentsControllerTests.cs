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

public class PayrollAdjustmentsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public PayrollAdjustmentsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(int orgId, int periodId, int employeeId)> SeedTestDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var org = new Organization
        {
            Id = 10,
            Name = "Adjustment Test Org"
        };
        if (!context.Organizations.Any(o => o.Id == 10))
            context.Organizations.Add(org);

        var fy = new FinancialYear
        {
            Id = 10,
            OrganizationId = 10,
            YearCode = "2026-27",
            StartDate = new DateOnly(2026, 4, 1),
            EndDate = new DateOnly(2027, 3, 31),
            IsActive = true
        };
        if (!context.FinancialYears.Any(f => f.Id == 10))
            context.FinancialYears.Add(fy);

        var period = new PayrollPeriod
        {
            Id = 10,
            OrganizationId = 10,
            FinancialYearId = 10,
            Month = 8,
            Year = 2026,
            StartDate = new DateOnly(2026, 8, 1),
            EndDate = new DateOnly(2026, 8, 31),
            Status = PayrollPeriodStatus.Draft
        };
        if (!context.PayrollPeriods.Any(p => p.Id == 10))
            context.PayrollPeriods.Add(period);

        var emp = new Employee
        {
            Id = 10,
            OrganizationId = 10,
            EmployeeCode = "EMP010",
            FirstName = "Bob",
            LastName = "Marley"
        };
        if (!context.Employees.Any(e => e.Id == 10))
            context.Employees.Add(emp);

        await context.SaveChangesAsync();
        return (10, 10, 10);
    }

    [Fact]
    public async Task CreateAndApproveAdjustment_Workflow_Success()
    {
        var (orgId, periodId, employeeId) = await SeedTestDataAsync();

        var token = _factory.CreateAdminToken(userId: 1, organizationId: orgId);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreatePayrollAdjustmentDto
        {
            EmployeeId = employeeId,
            PayrollPeriodId = periodId,
            Type = AdjustmentType.Bonus,
            Direction = AdjustmentDirection.Earning,
            Amount = 7500m,
            Reason = "Project delivery milestone bonus"
        };

        // 1. Create Adjustment
        var createResponse = await client.PostAsJsonAsync("/api/payroll/adjustments", request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var createdDto = await createResponse.Content.ReadFromJsonAsync<ApiResponseDto<PayrollAdjustmentDto>>();
        createdDto.Should().NotBeNull();
        createdDto!.Success.Should().BeTrue();
        createdDto.Data.Should().NotBeNull();
        createdDto.Data!.Amount.Should().Be(7500m);
        createdDto.Data.Status.Should().Be(AdjustmentStatus.Pending);

        var adjId = createdDto.Data.Id;

        // 2. Approve Adjustment
        var approveResponse = await client.PostAsync($"/api/payroll/adjustments/{adjId}/approve", null);
        approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Verify Status
        var getResponse = await client.GetAsync($"/api/payroll/adjustments/{adjId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getDto = await getResponse.Content.ReadFromJsonAsync<ApiResponseDto<PayrollAdjustmentDto>>();
        getDto.Should().NotBeNull();
        getDto!.Data!.Status.Should().Be(AdjustmentStatus.Approved);
    }
}
