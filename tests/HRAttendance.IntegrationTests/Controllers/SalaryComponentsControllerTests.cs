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

public class SalaryComponentsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public SalaryComponentsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task SeedOrganizationAsync(int orgId = 30)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (!context.Organizations.Any(o => o.Id == orgId))
        {
            context.Organizations.Add(new Organization
            {
                Id = orgId,
                Name = "Component Test Org"
            });
            await context.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task CreateAndGetComponent_Success()
    {
        await SeedOrganizationAsync(30);

        var token = _factory.CreateAdminToken(userId: 1, organizationId: 30);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateSalaryComponentDto
        {
            Code = "TRAVEL_ALLOWANCE",
            Name = "Travel Allowance",
            Type = ComponentType.Earning,
            CalculationType = ComponentCalculationType.FixedAmount,
            CalculationOrder = 5,
            IsTaxable = true,
            IsStatutory = false
        };

        // 1. Create component
        var createResponse = await client.PostAsJsonAsync("/api/payroll/salary-components", request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponseDto<SalaryComponentDto>>();
        created.Should().NotBeNull();
        created!.Success.Should().BeTrue();
        created.Data.Should().NotBeNull();
        created.Data!.Code.Should().Be("TRAVEL_ALLOWANCE");

        var compId = created.Data.Id;

        // 2. Get component by ID
        var getResponse = await client.GetAsync($"/api/payroll/salary-components/{compId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getDto = await getResponse.Content.ReadFromJsonAsync<ApiResponseDto<SalaryComponentDto>>();
        getDto.Should().NotBeNull();
        getDto!.Data!.Name.Should().Be("Travel Allowance");

        // 3. Duplicate code returns 400
        var duplicateResponse = await client.PostAsJsonAsync("/api/payroll/salary-components", request);
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
