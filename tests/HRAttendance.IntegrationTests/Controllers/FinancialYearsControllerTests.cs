using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using HRAttendance.Data;
using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Payroll;
using HRAttendance.Data.Models.Organization;
using Xunit;

namespace HRAttendance.IntegrationTests.Controllers;

public class FinancialYearsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public FinancialYearsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task SeedOrganizationAsync(int orgId = 40)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (!context.Organizations.Any(o => o.Id == orgId))
        {
            context.Organizations.Add(new Organization
            {
                Id = orgId,
                Name = "Fiscal Year Test Org"
            });
            await context.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task CreateAndListFinancialYears_Success()
    {
        await SeedOrganizationAsync(40);

        var token = _factory.CreateAdminToken(userId: 1, organizationId: 40);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateFinancialYearDto
        {
            YearCode = "2027-28",
            StartDate = new DateOnly(2027, 4, 1),
            EndDate = new DateOnly(2028, 3, 31),
            IsActive = true
        };

        // 1. Create FY
        var createResponse = await client.PostAsJsonAsync("/api/payroll/financial-years", request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponseDto<FinancialYearDto>>();
        created.Should().NotBeNull();
        created!.Success.Should().BeTrue();
        created.Data.Should().NotBeNull();
        created.Data!.YearCode.Should().Be("2027-28");

        // 2. List FYs
        var listResponse = await client.GetAsync("/api/payroll/financial-years");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var listDto = await listResponse.Content.ReadFromJsonAsync<ApiResponseDto<List<FinancialYearDto>>>();
        listDto.Should().NotBeNull();
        listDto!.Data.Should().Contain(f => f.YearCode == "2027-28");
    }
}
