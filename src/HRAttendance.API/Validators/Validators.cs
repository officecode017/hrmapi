using FluentValidation;
using HRAttendance.Data.DTOs.Auth;
using HRAttendance.Data.DTOs.Employee;
using HRAttendance.Data.DTOs.Attendance;
using HRAttendance.Data.DTOs.Leave;

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
