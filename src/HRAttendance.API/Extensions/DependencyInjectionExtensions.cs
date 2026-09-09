using Microsoft.EntityFrameworkCore;
using HRAttendance.Business.Interfaces;
using HRAttendance.Business.Services;
using HRAttendance.Data;
using HRAttendance.Data.Interfaces;
using HRAttendance.Data.Repositories;

namespace HRAttendance.API.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=(localdb)\\mssqllocaldb;Database=HRAttendanceDb;Trusted_Connection=True;MultipleActiveResultSets=true";

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Repositories
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<ILeaveRepository, LeaveRepository>();

        return services;
    }

    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // Security / Auth services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();

        // Domain / Business services
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<ILeaveService, LeaveService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<IHolidayService, HolidayService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IOvertimeService, OvertimeService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IDesignationService, DesignationService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IAcademicYearService, AcademicYearService>();
        services.AddScoped<IOffDayService, OffDayService>();
        services.AddScoped<ILeaveTypeService, LeaveTypeService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // Payroll Domain Services
        services.AddScoped<HRAttendance.Business.Interfaces.Payroll.IEmploymentPeriodResolver, HRAttendance.Business.Services.Payroll.EmploymentPeriodResolver>();
        services.AddScoped<HRAttendance.Business.Interfaces.Payroll.IMidMonthProrationService, HRAttendance.Business.Services.Payroll.MidMonthProrationService>();
        services.AddScoped<HRAttendance.Business.Interfaces.Payroll.ILOPCalculationService, HRAttendance.Business.Services.Payroll.LOPCalculationService>();
        services.AddScoped<HRAttendance.Business.Interfaces.Payroll.IOvertimeCalculationService, HRAttendance.Business.Services.Payroll.OvertimeCalculationService>();
        services.AddScoped<HRAttendance.Business.Interfaces.Payroll.Statutory.IStatutoryRuleProvider, HRAttendance.Business.Services.Payroll.Statutory.StatutoryRuleProvider>();
        services.AddScoped<HRAttendance.Business.Interfaces.Payroll.Statutory.IPFCalculator, HRAttendance.Business.Services.Payroll.Statutory.PFCalculator>();
        services.AddScoped<HRAttendance.Business.Interfaces.Payroll.Statutory.IESICalculator, HRAttendance.Business.Services.Payroll.Statutory.ESICalculator>();
        services.AddScoped<HRAttendance.Business.Interfaces.Payroll.Statutory.IProfessionalTaxCalculator, HRAttendance.Business.Services.Payroll.Statutory.ProfessionalTaxCalculator>();
        services.AddScoped<HRAttendance.Business.Interfaces.Payroll.Statutory.ITDSCalculator, HRAttendance.Business.Services.Payroll.Statutory.TDSCalculator>();
        services.AddScoped<HRAttendance.Business.Interfaces.Payroll.Statutory.IStatutoryCalculationService, HRAttendance.Business.Services.Payroll.Statutory.StatutoryCalculationService>();
        services.AddScoped<HRAttendance.Business.Interfaces.Payroll.IPayrollCalculationEngine, HRAttendance.Business.Services.Payroll.PayrollCalculationEngine>();
        services.AddScoped<HRAttendance.Business.Interfaces.Payroll.IPayrollValidationService, HRAttendance.Business.Services.Payroll.PayrollValidationService>();
        services.AddScoped<HRAttendance.Business.Interfaces.Payroll.IPayslipProjectionService, HRAttendance.Business.Services.Payroll.PayslipProjectionService>();
        services.AddScoped<HRAttendance.Business.Interfaces.Payroll.IPayrollLifecycleService, HRAttendance.Business.Services.Payroll.PayrollLifecycleService>();
        services.AddScoped<HRAttendance.Business.Interfaces.Payroll.IPayrollAdjustmentService, HRAttendance.Business.Services.Payroll.PayrollAdjustmentService>();
        services.AddScoped<HRAttendance.Business.Interfaces.Payroll.IPayrollArrearService, HRAttendance.Business.Services.Payroll.PayrollArrearService>();
        services.AddScoped<HRAttendance.Business.Interfaces.Payroll.IPayrollPaymentService, HRAttendance.Business.Services.Payroll.PayrollPaymentService>();
        services.AddScoped<HRAttendance.Business.Interfaces.Payroll.IPayslipPdfGenerator, HRAttendance.Business.Services.Payroll.QuestPdfGenerator>();
        services.AddScoped<HRAttendance.Business.Interfaces.Payroll.IBankExportService, HRAttendance.Business.Services.Payroll.BankExportService>();

        return services;
    }
}
