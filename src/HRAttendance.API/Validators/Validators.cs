using FluentValidation;
using HRAttendance.Data.DTOs.Auth;
using HRAttendance.Data.DTOs.Employee;
using HRAttendance.Data.DTOs.Attendance;
using HRAttendance.Data.DTOs.Leave;
using HRAttendance.Data.DTOs.Shift;
using HRAttendance.Data.DTOs.Holiday;
using HRAttendance.Data.DTOs.Department;
using HRAttendance.Data.DTOs.Designation;
using HRAttendance.Data.DTOs.Location;
using HRAttendance.Data.DTOs.AcademicYear;
using HRAttendance.Data.DTOs.OffDay;
using HRAttendance.Data.DTOs.Overtime;

namespace HRAttendance.API.Validators;

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0).WithMessage("OrganizationId must be greater than 0.");
        RuleFor(x => x.EmployeeCodeOrEmail).NotEmpty().WithMessage("Employee code or email is required.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
    }
}

public class CreateEmployeeDtoValidator : AbstractValidator<CreateEmployeeDto>
{
    public CreateEmployeeDtoValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0);
        RuleFor(x => x.EmployeeCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.WorkEmail).NotEmpty().EmailAddress().MaximumLength(150);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}

public class CheckInRequestDtoValidator : AbstractValidator<CheckInRequestDto>
{
    public CheckInRequestDtoValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0);
        RuleFor(x => x.LocationId).GreaterThan(0);
    }
}

public class CreateLeaveApplicationDtoValidator : AbstractValidator<CreateLeaveApplicationDto>
{
    public CreateLeaveApplicationDtoValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0);
        RuleFor(x => x.LeaveTypeId).GreaterThan(0);
        RuleFor(x => x.NoOfLeave).GreaterThan(0);
        RuleFor(x => x.LeaveFrom).NotEmpty();
        RuleFor(x => x.LeaveTo).GreaterThanOrEqualTo(x => x.LeaveFrom);
    }
}

public class CreateShiftDtoValidator : AbstractValidator<CreateShiftDto>
{
    public CreateShiftDtoValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0);
        RuleFor(x => x.LocationId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateHolidayDtoValidator : AbstractValidator<CreateHolidayDto>
{
    public CreateHolidayDtoValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0);
        RuleFor(x => x.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateDepartmentDtoValidator : AbstractValidator<CreateDepartmentDto>
{
    public CreateDepartmentDtoValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateDesignationDtoValidator : AbstractValidator<CreateDesignationDto>
{
    public CreateDesignationDtoValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateLocationDtoValidator : AbstractValidator<CreateLocationDto>
{
    public CreateLocationDtoValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateAcademicYearDtoValidator : AbstractValidator<CreateAcademicYearDto>
{
    public CreateAcademicYearDtoValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate).WithMessage("EndDate must be after StartDate.");
    }
}

public class CreateOffDayDtoValidator : AbstractValidator<CreateOffDayDto>
{
    public CreateOffDayDtoValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0);
        RuleFor(x => x.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.LocationId).GreaterThan(0);
        RuleFor(x => x.OffDayName).NotEmpty().MaximumLength(50);
    }
}

public class CreateLeaveTypeDtoValidator : AbstractValidator<CreateLeaveTypeDto>
{
    public CreateLeaveTypeDtoValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Leaves).GreaterThanOrEqualTo(0);
    }
}

public class CreateOTSettingDtoValidator : AbstractValidator<CreateOTSettingDto>
{
    public CreateOTSettingDtoValidator()
    {
        RuleFor(x => x.OrganizationId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Multiplier).GreaterThan(0);
    }
}
