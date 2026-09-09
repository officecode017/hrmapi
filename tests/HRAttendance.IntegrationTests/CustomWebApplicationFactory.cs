using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HRAttendance.Business.Services;
using HRAttendance.Data;
using HRAttendance.Data.Models.Employee;

namespace HRAttendance.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public string DatabaseName { get; } = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptors = services.Where(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
                  || d.ServiceType == typeof(DbContextOptions)
                  || d.ServiceType == typeof(ApplicationDbContext)).ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            var inMemoryProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(DatabaseName);
                options.UseInternalServiceProvider(inMemoryProvider);
            });
        });
    }

    public string CreateAdminToken(int userId = 1, int organizationId = 1)
    {
        using var scope = Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<HRAttendance.Business.Interfaces.ITokenService>();

        var employee = new Employee
        {
            Id = userId,
            OrganizationId = organizationId,
            EmployeeCode = "ADM001",
            FirstName = "Admin",
            LastName = "User"
        };
        return tokenService.GenerateToken(employee, new List<string> { HRAttendance.Business.BusinessRules.ApplicationRoles.SuperAdmin, HRAttendance.Business.BusinessRules.ApplicationRoles.Admin }, out _);
    }
}
