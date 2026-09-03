using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HRAttendance.Business.BusinessRules;
using HRAttendance.Business.Interfaces;
using HRAttendance.Data;
using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Leave;
using HRAttendance.Data.Interfaces;
using HRAttendance.Data.Models.Leave;

namespace HRAttendance.Business.Services;

public class LeaveService : ILeaveService
{
    private readonly ILeaveRepository _leaveRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LeaveService> _logger;

    public LeaveService(
        ILeaveRepository leaveRepository,
        ApplicationDbContext context,
        ILogger<LeaveService> logger)
    {
        _leaveRepository = leaveRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponseDto<LeaveApplicationDto>> ApplyLeaveAsync(CreateLeaveApplicationDto request, CancellationToken cancellationToken = default)
    {
        if (request.LeaveTo < request.LeaveFrom)
        {
            return ApiResponseDto<LeaveApplicationDto>.Fail("Leave 'To' date cannot be earlier than 'From' date.");
        }

        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);
        if (employee == null)
        {
            return ApiResponseDto<LeaveApplicationDto>.Fail("Employee not found.");
        }

        var leaveType = await _context.LeaveTypes.FirstOrDefaultAsync(lt => lt.Id == request.LeaveTypeId, cancellationToken);
        if (leaveType == null || !leaveType.IsActive)
        {
            return ApiResponseDto<LeaveApplicationDto>.Fail("Invalid or inactive leave type.");
        }

        var academicYear = await _context.AcademicYears
            .FirstOrDefaultAsync(a => a.OrganizationId == employee.OrganizationId && a.IsActive, cancellationToken);

        if (academicYear == null)
        {
            return ApiResponseDto<LeaveApplicationDto>.Fail("Active academic year not configured.");
        }

        // Check leave balance
        var balance = await _leaveRepository.GetEmployeeLeaveBalanceAsync(request.EmployeeId, request.LeaveTypeId, academicYear.Id, cancellationToken);
        if (balance != null)
        {
            var available = (balance.LeaveCredited + balance.LeaveBroughtForward) - balance.LeavesTaken;
            if (available < request.NoOfLeave)
            {
                return ApiResponseDto<LeaveApplicationDto>.Fail($"Insufficient leave balance. Available: {available}, Requested: {request.NoOfLeave}");
            }
        }

        var now = DateTimeOffset.UtcNow;
        var application = new LeaveApplication
        {
            OrganizationId = employee.OrganizationId,
            EmployeeId = request.EmployeeId,
            LeaveTypeId = request.LeaveTypeId,
            NoOfLeave = request.NoOfLeave,
            LeaveFrom = request.LeaveFrom,
            LeaveTo = request.LeaveTo,
            LeaveApplicationDate = now,
            Status = LeaveStatus.Pending,
            Reason = request.Reason
        };

        await _leaveRepository.AddApplicationAsync(application, cancellationToken);

        // Generate SubLeaveApplications for each day in range
        var subApplications = new List<SubLeaveApplication>();
        var currentDate = DateOnly.FromDateTime(request.LeaveFrom.DateTime);
        var endDate = DateOnly.FromDateTime(request.LeaveTo.DateTime);

        while (currentDate <= endDate)
        {
            subApplications.Add(new SubLeaveApplication
            {
                OrganizationId = employee.OrganizationId,
                LeaveApplicationId = application.Id,
                EmployeeId = request.EmployeeId,
                LeaveDate = currentDate,
                Status = LeaveStatus.Pending,
                IsHalfDay = request.IsHalfDay
            });

            currentDate = currentDate.AddDays(1);
        }

        if (subApplications.Any())
        {
            await _leaveRepository.AddSubApplicationsAsync(subApplications, cancellationToken);
        }

        _logger.LogInformation("Employee {EmployeeId} applied for {Days} days of leave (AppId: {AppId})", request.EmployeeId, request.NoOfLeave, application.Id);

        var dto = new LeaveApplicationDto
        {
            Id = application.Id,
            EmployeeId = application.EmployeeId,
            EmployeeName = $"{employee.FirstName} {employee.LastName}".Trim(),
            LeaveTypeId = application.LeaveTypeId,
            LeaveTypeName = leaveType.Name,
            NoOfLeave = application.NoOfLeave,
            LeaveFrom = application.LeaveFrom,
            LeaveTo = application.LeaveTo,
            LeaveApplicationDate = application.LeaveApplicationDate,
            Status = application.Status,
            Reason = application.Reason
        };

        return ApiResponseDto<LeaveApplicationDto>.Ok(dto, "Leave application submitted successfully.");
    }

    public async Task<ApiResponseDto<List<LeaveApplicationDto>>> GetEmployeeLeavesAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var list = await _leaveRepository.GetByEmployeeIdAsync(employeeId, cancellationToken);

        var dtos = list.Select(l => new LeaveApplicationDto
        {
            Id = l.Id,
            EmployeeId = l.EmployeeId,
            EmployeeName = l.Employee != null ? $"{l.Employee.FirstName} {l.Employee.LastName}".Trim() : string.Empty,
            LeaveTypeId = l.LeaveTypeId,
            LeaveTypeName = l.LeaveType.Name,
            NoOfLeave = l.NoOfLeave,
            LeaveFrom = l.LeaveFrom,
            LeaveTo = l.LeaveTo,
            LeaveApplicationDate = l.LeaveApplicationDate,
            Status = l.Status,
            Reason = l.Reason
        }).ToList();

        return ApiResponseDto<List<LeaveApplicationDto>>.Ok(dtos);
    }

    public async Task<ApiResponseDto<List<LeaveApplicationDto>>> GetPendingLeavesAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        var list = await _leaveRepository.GetPendingByOrganizationAsync(organizationId, cancellationToken);

        var dtos = list.Select(l => new LeaveApplicationDto
        {
            Id = l.Id,
            EmployeeId = l.EmployeeId,
            EmployeeName = l.Employee != null ? $"{l.Employee.FirstName} {l.Employee.LastName}".Trim() : string.Empty,
            LeaveTypeId = l.LeaveTypeId,
            LeaveTypeName = l.LeaveType.Name,
            NoOfLeave = l.NoOfLeave,
            LeaveFrom = l.LeaveFrom,
            LeaveTo = l.LeaveTo,
            LeaveApplicationDate = l.LeaveApplicationDate,
            Status = l.Status,
            Reason = l.Reason
        }).ToList();

        return ApiResponseDto<List<LeaveApplicationDto>>.Ok(dtos);
    }

    public async Task<ApiResponseDto<bool>> ProcessLeaveApprovalAsync(int leaveApplicationId, int approverId, LeaveApprovalDto request, CancellationToken cancellationToken = default)
    {
        var application = await _leaveRepository.GetByIdAsync(leaveApplicationId, cancellationToken);
        if (application == null)
        {
            return ApiResponseDto<bool>.Fail("Leave application not found.");
        }

        if (application.Status != LeaveStatus.Pending)
        {
            return ApiResponseDto<bool>.Fail($"Leave application is already {application.Status}.");
        }

        var newStatus = request.IsApproved ? LeaveStatus.Approved : LeaveStatus.Rejected;
        application.Status = newStatus;
        application.ApprovedBy = approverId;

        foreach (var sub in application.SubLeaveApplications)
        {
            sub.Status = newStatus;
            sub.ApprovedBy = approverId;
        }

        // If approved, deduct from EmployeeLeaves balance
        if (request.IsApproved)
        {
            var academicYear = await _context.AcademicYears
                .FirstOrDefaultAsync(a => a.OrganizationId == application.OrganizationId && a.IsActive, cancellationToken);

            if (academicYear != null)
            {
                var balance = await _leaveRepository.GetEmployeeLeaveBalanceAsync(application.EmployeeId, application.LeaveTypeId, academicYear.Id, cancellationToken);
                if (balance != null)
                {
                    balance.LeavesTaken += application.NoOfLeave;
                    await _leaveRepository.UpdateLeaveBalanceAsync(balance, cancellationToken);
                }
            }
        }

        await _leaveRepository.UpdateApplicationAsync(application, cancellationToken);
        _logger.LogInformation("Leave application {AppId} was {Status} by approver {ApproverId}", leaveApplicationId, newStatus, approverId);

        return ApiResponseDto<bool>.Ok(true, $"Leave application {newStatus.ToLower()} successfully.");
    }

    public async Task<ApiResponseDto<List<LeaveBalanceDto>>> GetLeaveBalancesAsync(int employeeId, int academicYearId, CancellationToken cancellationToken = default)
    {
        var balances = await _leaveRepository.GetEmployeeLeaveBalancesAsync(employeeId, academicYearId, cancellationToken);

        var dtos = balances.Select(b => new LeaveBalanceDto
        {
            LeaveTypeId = b.LeaveTypeId,
            LeaveTypeName = b.LeaveType.Name,
            Credited = b.LeaveCredited,
            BroughtForward = b.LeaveBroughtForward,
            Taken = b.LeavesTaken
        }).ToList();

        return ApiResponseDto<List<LeaveBalanceDto>>.Ok(dtos);
    }
}
