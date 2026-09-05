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

        var leaveDate = DateOnly.FromDateTime(request.LeaveFrom.DateTime);
        var academicYear = await _context.AcademicYears
            .Where(a => a.OrganizationId == employee.OrganizationId && a.StartDate <= leaveDate && a.EndDate >= leaveDate)
            .OrderByDescending(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? await _context.AcademicYears
                .Where(a => a.OrganizationId == employee.OrganizationId && a.IsActive)
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync(cancellationToken);

        if (academicYear == null)
        {
            return ApiResponseDto<LeaveApplicationDto>.Fail("Active academic year not configured.");
        }

        // 1. BUSINESS RULE: Prevent Overlapping Leave Applications
        var hasOverlap = await _context.LeaveApplications
            .AnyAsync(l => l.EmployeeId == request.EmployeeId &&
                           l.Status != LeaveStatus.Rejected &&
                           l.Status != "Cancelled" &&
                           l.LeaveFrom <= request.LeaveTo &&
                           l.LeaveTo >= request.LeaveFrom, cancellationToken);

        if (hasOverlap)
        {
            return ApiResponseDto<LeaveApplicationDto>.Fail("You already have an active or pending leave application overlapping with these dates.");
        }

        // 2. Check leave balance
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
        var applications = await _leaveRepository.GetByEmployeeIdAsync(employeeId, cancellationToken);

        var dtos = applications.Select(a => new LeaveApplicationDto
        {
            Id = a.Id,
            EmployeeId = a.EmployeeId,
            EmployeeName = a.Employee != null ? $"{a.Employee.FirstName} {a.Employee.LastName}".Trim() : string.Empty,
            LeaveTypeId = a.LeaveTypeId,
            LeaveTypeName = a.LeaveType?.Name ?? string.Empty,
            NoOfLeave = a.NoOfLeave,
            LeaveFrom = a.LeaveFrom,
            LeaveTo = a.LeaveTo,
            LeaveApplicationDate = a.LeaveApplicationDate,
            Status = a.Status,
            Reason = a.Reason,
            ApproverName = a.Approver != null ? $"{a.Approver.FirstName} {a.Approver.LastName}".Trim() : null
        }).ToList();

        return ApiResponseDto<List<LeaveApplicationDto>>.Ok(dtos);
    }

    public async Task<ApiResponseDto<List<LeaveApplicationDto>>> GetPendingLeavesAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        var applications = await _leaveRepository.GetPendingByOrganizationAsync(organizationId, cancellationToken);

        var dtos = applications.Select(a => new LeaveApplicationDto
        {
            Id = a.Id,
            EmployeeId = a.EmployeeId,
            EmployeeName = a.Employee != null ? $"{a.Employee.FirstName} {a.Employee.LastName}".Trim() : string.Empty,
            LeaveTypeId = a.LeaveTypeId,
            LeaveTypeName = a.LeaveType?.Name ?? string.Empty,
            NoOfLeave = a.NoOfLeave,
            LeaveFrom = a.LeaveFrom,
            LeaveTo = a.LeaveTo,
            LeaveApplicationDate = a.LeaveApplicationDate,
            Status = a.Status,
            Reason = a.Reason,
            ApproverName = a.Approver != null ? $"{a.Approver.FirstName} {a.Approver.LastName}".Trim() : null
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
            var leaveDate = DateOnly.FromDateTime(application.LeaveFrom.DateTime);
            var academicYear = await _context.AcademicYears
                .Where(a => a.OrganizationId == application.OrganizationId && a.StartDate <= leaveDate && a.EndDate >= leaveDate)
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync(cancellationToken)
                ?? await _context.AcademicYears
                    .Where(a => a.OrganizationId == application.OrganizationId && a.IsActive)
                    .OrderByDescending(a => a.Id)
                    .FirstOrDefaultAsync(cancellationToken);

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

    public async Task<ApiResponseDto<bool>> CancelLeaveAsync(int leaveApplicationId, int employeeId, bool isAdmin = false, CancellationToken cancellationToken = default)
    {
        var application = await _leaveRepository.GetByIdAsync(leaveApplicationId, cancellationToken);
        if (application == null)
        {
            return ApiResponseDto<bool>.Fail("Leave application not found.");
        }

        if (!isAdmin && application.EmployeeId != employeeId)
        {
            return ApiResponseDto<bool>.Fail("Unauthorized: You can only cancel your own leave applications.");
        }

        if (application.Status == "Cancelled")
        {
            return ApiResponseDto<bool>.Fail("Leave application is already cancelled.");
        }

        // If previously approved, refund deducted balance
        if (application.Status == LeaveStatus.Approved)
        {
            var leaveDate = DateOnly.FromDateTime(application.LeaveFrom.DateTime);
            var academicYear = await _context.AcademicYears
                .Where(a => a.OrganizationId == application.OrganizationId && a.StartDate <= leaveDate && a.EndDate >= leaveDate)
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync(cancellationToken)
                ?? await _context.AcademicYears
                    .Where(a => a.OrganizationId == application.OrganizationId && a.IsActive)
                    .OrderByDescending(a => a.Id)
                    .FirstOrDefaultAsync(cancellationToken);

            if (academicYear != null)
            {
                var balance = await _leaveRepository.GetEmployeeLeaveBalanceAsync(application.EmployeeId, application.LeaveTypeId, academicYear.Id, cancellationToken);
                if (balance != null)
                {
                    balance.LeavesTaken = Math.Max(0, balance.LeavesTaken - application.NoOfLeave);
                    await _leaveRepository.UpdateLeaveBalanceAsync(balance, cancellationToken);
                }
            }
        }

        application.Status = "Cancelled";
        foreach (var sub in application.SubLeaveApplications)
        {
            sub.Status = "Cancelled";
        }

        await _leaveRepository.UpdateApplicationAsync(application, cancellationToken);
        _logger.LogInformation("Leave application {AppId} cancelled by employee {EmployeeId}", leaveApplicationId, employeeId);

        return ApiResponseDto<bool>.Ok(true, "Leave application cancelled successfully.");
    }

    public async Task<ApiResponseDto<List<LeaveBalanceDto>>> GetLeaveBalancesAsync(int employeeId, int academicYearId, CancellationToken cancellationToken = default)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);
        var orgId = employee?.OrganizationId ?? 1;

        if (academicYearId <= 0)
        {
            var activeYear = await _context.AcademicYears
                .AsNoTracking()
                .Where(a => a.OrganizationId == orgId && a.IsActive)
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync(cancellationToken);
            academicYearId = activeYear?.Id ?? 1;
        }

        var balances = await _context.EmployeeLeaves
            .AsNoTracking()
            .Where(el => el.EmployeeId == employeeId && el.AcademicYearId == academicYearId)
            .Include(el => el.LeaveType)
            .ToListAsync(cancellationToken);

        var balanceMap = balances.ToDictionary(b => b.LeaveTypeId);

        // Also fetch all active leave types for this organization
        var leaveTypes = await _context.LeaveTypes
            .AsNoTracking()
            .Include(lt => lt.LeaveSetting)
            .Where(lt => lt.OrganizationId == orgId && lt.IsActive && !lt.IsDeleted)
            .ToListAsync(cancellationToken);

        var dtos = new List<LeaveBalanceDto>();
        foreach (var lt in leaveTypes)
        {
            if (balanceMap.TryGetValue(lt.Id, out var b))
            {
                dtos.Add(new LeaveBalanceDto
                {
                    LeaveTypeId = b.LeaveTypeId,
                    LeaveTypeName = b.LeaveType?.Name ?? lt.Name,
                    Credited = b.LeaveCredited,
                    BroughtForward = b.LeaveBroughtForward,
                    Taken = b.LeavesTaken
                });
            }
            else
            {
                var standardQuota = lt.LeaveSetting?.Leaves ?? 12;
                dtos.Add(new LeaveBalanceDto
                {
                    LeaveTypeId = lt.Id,
                    LeaveTypeName = lt.Name,
                    Credited = standardQuota,
                    BroughtForward = 0,
                    Taken = 0
                });
            }
        }

        return ApiResponseDto<List<LeaveBalanceDto>>.Ok(dtos);
    }


    public async Task<ApiResponseDto<bool>> AdjustLeaveBalanceAsync(AdjustLeaveBalanceDto request, CancellationToken cancellationToken = default)
    {
        var balance = await _context.EmployeeLeaves
            .FirstOrDefaultAsync(b => b.EmployeeId == request.EmployeeId &&
                                      b.LeaveTypeId == request.LeaveTypeId &&
                                      b.AcademicYearId == request.AcademicYearId, cancellationToken);

        if (balance == null)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);
            if (employee == null) return ApiResponseDto<bool>.Fail("Employee not found.");

            balance = new EmployeeLeave
            {
                OrganizationId = employee.OrganizationId,
                EmployeeId = request.EmployeeId,
                LeaveTypeId = request.LeaveTypeId,
                AcademicYearId = request.AcademicYearId,
                LeaveCredited = request.LeaveCredited,
                LeaveBroughtForward = request.LeaveBroughtForward,
                LeavesTaken = request.LeavesTaken
            };
            await _context.EmployeeLeaves.AddAsync(balance, cancellationToken);
        }
        else
        {
            balance.LeaveCredited = request.LeaveCredited;
            balance.LeaveBroughtForward = request.LeaveBroughtForward;
            balance.LeavesTaken = request.LeavesTaken;
            _context.EmployeeLeaves.Update(balance);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Leave balance adjusted for employee {EmployeeId}, type {TypeId}: Credited={Credited}, Taken={Taken}", request.EmployeeId, request.LeaveTypeId, request.LeaveCredited, request.LeavesTaken);

        return ApiResponseDto<bool>.Ok(true, "Employee leave balance adjusted successfully.");
    }

    public async Task<ApiResponseDto<List<EmployeeLeaveBalanceDetailDto>>> GetAllLeaveBalancesAsync(
        int organizationId,
        int? academicYearId,
        int? departmentId,
        int? leaveTypeId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = _context.EmployeeLeaves
            .AsNoTracking()
            .Include(el => el.Employee)
                .ThenInclude(e => e.ProfessionalDetails)
                    .ThenInclude(p => p!.Department)
            .Include(el => el.Employee)
                .ThenInclude(e => e.ProfessionalDetails)
                    .ThenInclude(p => p!.Designation)
            .Include(el => el.LeaveType)
            .Include(el => el.AcademicYear)
            .Where(el => el.OrganizationId == organizationId);

        if (academicYearId.HasValue && academicYearId.Value > 0)
        {
            query = query.Where(el => el.AcademicYearId == academicYearId.Value);
        }

        if (departmentId.HasValue && departmentId.Value > 0)
        {
            query = query.Where(el => el.Employee.ProfessionalDetails != null && el.Employee.ProfessionalDetails.DepartmentId == departmentId.Value);
        }

        if (leaveTypeId.HasValue && leaveTypeId.Value > 0)
        {
            query = query.Where(el => el.LeaveTypeId == leaveTypeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(el =>
                (el.Employee.FirstName != null && el.Employee.FirstName.ToLower().Contains(searchLower)) ||
                (el.Employee.LastName != null && el.Employee.LastName.ToLower().Contains(searchLower)) ||
                (el.Employee.EmployeeCode != null && el.Employee.EmployeeCode.ToLower().Contains(searchLower)));
        }

        var list = await query
            .OrderBy(el => el.Employee.FirstName)
            .ThenBy(el => el.LeaveType.Name)
            .Select(el => new EmployeeLeaveBalanceDetailDto
            {
                Id = el.Id,
                EmployeeId = el.EmployeeId,
                EmployeeName = $"{el.Employee.FirstName} {el.Employee.LastName}".Trim(),
                EmployeeCode = el.Employee.EmployeeCode,
                DepartmentName = el.Employee.ProfessionalDetails != null && el.Employee.ProfessionalDetails.Department != null ? el.Employee.ProfessionalDetails.Department.Name : null,
                DesignationName = el.Employee.ProfessionalDetails != null && el.Employee.ProfessionalDetails.Designation != null ? el.Employee.ProfessionalDetails.Designation.Name : null,
                ProfilePictureUrl = el.Employee.PhotoPath,
                LeaveTypeId = el.LeaveTypeId,
                LeaveTypeName = el.LeaveType.Name,
                AcademicYearId = el.AcademicYearId,
                AcademicYearStartDate = el.AcademicYear.StartDate,
                AcademicYearEndDate = el.AcademicYear.EndDate,
                LeaveCredited = el.LeaveCredited,
                LeaveBroughtForward = el.LeaveBroughtForward,
                LeavesTaken = el.LeavesTaken
            })
            .ToListAsync(cancellationToken);

        return ApiResponseDto<List<EmployeeLeaveBalanceDetailDto>>.Ok(list);
    }

    public async Task<ApiResponseDto<int>> BulkAllocateLeaveBalancesAsync(
        BulkAllocateLeaveBalanceDto request,
        CancellationToken cancellationToken = default)
    {
        var academicYear = await _context.AcademicYears.FirstOrDefaultAsync(ay => ay.Id == request.AcademicYearId, cancellationToken);
        if (academicYear == null)
        {
            return ApiResponseDto<int>.Fail("Selected academic year not found.");
        }

        var employeesQuery = _context.Employees
            .Include(e => e.ProfessionalDetails)
            .Where(e => e.OrganizationId == request.OrganizationId && e.IsActive && !e.IsDeleted);

        if (request.DepartmentId.HasValue && request.DepartmentId.Value > 0)
        {
            employeesQuery = employeesQuery.Where(e => e.ProfessionalDetails != null && e.ProfessionalDetails.DepartmentId == request.DepartmentId.Value);
        }


        var employees = await employeesQuery.ToListAsync(cancellationToken);
        if (!employees.Any())
        {
            return ApiResponseDto<int>.Fail("No active employees found matching the criteria.");
        }

        var leaveTypesQuery = _context.LeaveTypes
            .Include(lt => lt.LeaveSetting)
            .Where(lt => lt.OrganizationId == request.OrganizationId && lt.IsActive && !lt.IsDeleted);

        if (request.LeaveTypeId.HasValue && request.LeaveTypeId.Value > 0)
        {
            leaveTypesQuery = leaveTypesQuery.Where(lt => lt.Id == request.LeaveTypeId.Value);
        }

        var leaveTypes = await leaveTypesQuery.ToListAsync(cancellationToken);
        if (!leaveTypes.Any())
        {
            return ApiResponseDto<int>.Fail("No active leave types found to allocate.");
        }

        var empIds = employees.Select(e => e.Id).ToList();
        var ltIds = leaveTypes.Select(lt => lt.Id).ToList();

        var existingBalances = await _context.EmployeeLeaves
            .Where(el => el.AcademicYearId == request.AcademicYearId && empIds.Contains(el.EmployeeId) && ltIds.Contains(el.LeaveTypeId))
            .ToListAsync(cancellationToken);

        var existingLookup = existingBalances.ToDictionary(el => (el.EmployeeId, el.LeaveTypeId));

        int count = 0;
        foreach (var emp in employees)
        {
            foreach (var lt in leaveTypes)
            {
                var standardQuota = lt.LeaveSetting?.Leaves ?? 12;

                if (existingLookup.TryGetValue((emp.Id, lt.Id), out var existing))
                {
                    if (request.OverwriteExisting)
                    {
                        existing.LeaveCredited = standardQuota;
                        _context.EmployeeLeaves.Update(existing);
                        count++;
                    }
                }
                else
                {
                    var newBalance = new EmployeeLeave
                    {
                        OrganizationId = request.OrganizationId,
                        EmployeeId = emp.Id,
                        LeaveTypeId = lt.Id,
                        AcademicYearId = request.AcademicYearId,
                        LeaveCredited = standardQuota,
                        LeaveBroughtForward = 0,
                        LeavesTaken = 0
                    };
                    await _context.EmployeeLeaves.AddAsync(newBalance, cancellationToken);
                    count++;
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Bulk allocated leave balances for {Count} entries in academic year {YearId}", count, request.AcademicYearId);

        return ApiResponseDto<int>.Ok(count, $"Successfully allocated/updated leave balances for {count} records.");
    }
}

