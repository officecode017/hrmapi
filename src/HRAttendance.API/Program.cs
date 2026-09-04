using FluentValidation;
using FluentValidation.AspNetCore;
using HRAttendance.API.Extensions;
using HRAttendance.API.Middleware;
using HRAttendance.API.Validators;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Infrastructure, Business, Auth and Swagger extensions
builder.Services
    .AddDataServices(builder.Configuration)
    .AddBusinessServices()
    .AddAuthenticationServices(builder.Configuration)
    .AddSwaggerServices();

// 2. Controllers and FluentValidation
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestDtoValidator>();

// 3. CORS configuration for frontend web app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// 4. Global Exception Middleware (ProblemDetails)
app.UseMiddleware<GlobalExceptionMiddleware>();

// 5. OpenAPI / Swagger Documentation (Available in Development & Production)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HRAttendance API v1");
    c.RoutePrefix = string.Empty; // Serve Swagger UI at root "/"
});

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 6. Automatic Database Seeding
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<HRAttendance.Data.ApplicationDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<HRAttendance.Business.Interfaces.IPasswordHasher>();
    await HRAttendance.Data.DbInitializer.SeedAsync(context, passwordHasher.HashPassword);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning(ex, "Database seeding skipped or encountered an error. Ensure SQL Server is accessible.");
}

app.Run();

// Needed for WebApplicationFactory in integration tests
public partial class Program { }
