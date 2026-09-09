using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using HRAttendance.Data;
using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Payroll;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Payroll;
using Xunit;

namespace HRAttendance.IntegrationTests.Controllers;

public class PayrollPeriodsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PayrollPeriodsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task SeedTestDataAsync(int orgId = 1, int fyId = 1)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (!context.Organizations.Any(o => o.Id == orgId))
        {
            context.Organizations.Add(new Organization
            {
                Id = orgId,
                Name = "Integration Test Org"
            });
        }

        if (!context.FinancialYears.Any(f => f.Id == fyId))
        {
            context.FinancialYears.Add(new FinancialYear
            {
                Id = fyId,
                OrganizationId = orgId,
                YearCode = "2026-27",
                StartDate = new DateOnly(2026, 4, 1),
                EndDate = new DateOnly(2027, 3, 31),
                IsActive = true
            });
        }

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetPeriods_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = null;

        var response = await client.GetAsync("/api/payroll/periods");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePeriod_WithValidData_ReturnsCreatedAndListsPeriods()
    {
        await SeedTestDataAsync(1, 1);

        var token = _factory.CreateAdminToken(userId: 1, organizationId: 1);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreatePayrollPeriodDto
        {
            FinancialYearId = 1,
            Month = 11,
            Year = 2026,
            RunType = PayrollRunType.Regular,
            SequenceNumber = 1,
            Description = "November 2026 Regular Payroll",
            StartDate = new DateOnly(2026, 11, 1),
            EndDate = new DateOnly(2026, 11, 30)
        };

        // Act 1: POST /api/payroll/periods
        var postResponse = await client.PostAsJsonAsync("/api/payroll/periods", request);
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await postResponse.Content.ReadFromJsonAsync<ApiResponseDto<PayrollPeriodDto>>();
        created.Should().NotBeNull();
        created!.Success.Should().BeTrue();
        created.Data.Should().NotBeNull();
        created.Data!.Month.Should().Be(11);
        created.Data.Year.Should().Be(2026);
        created.Data.Status.Should().Be(PayrollPeriodStatus.Draft);

        var periodId = created.Data.Id;

        // Act 2: GET /api/payroll/periods/{id}
        var getResponse = await client.GetAsync($"/api/payroll/periods/{periodId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var periodDto = await getResponse.Content.ReadFromJsonAsync<ApiResponseDto<PayrollPeriodDto>>();
        periodDto.Should().NotBeNull();
        periodDto!.Data!.Id.Should().Be(periodId);

        // Act 3: GET /api/payroll/periods/{id}/summary
        var summaryResponse = await client.GetAsync($"/api/payroll/periods/{periodId}/summary");
        summaryResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var summaryDto = await summaryResponse.Content.ReadFromJsonAsync<ApiResponseDto<PayrollPeriodSummaryDto>>();
        summaryDto.Should().NotBeNull();
        summaryDto!.Data!.PeriodId.Should().Be(periodId);
        summaryDto.Data.Status.Should().Be(PayrollPeriodStatus.Draft);
    }
}
