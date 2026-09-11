using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRAttendance.API.Extensions;
using HRAttendance.Business.BusinessRules;
using HRAttendance.Business.Interfaces.Payroll;
using HRAttendance.Data;
using HRAttendance.Data.DTOs.Common;
using HRAttendance.Data.DTOs.Payroll;

namespace HRAttendance.API.Controllers.Payroll;

[ApiController]
[Route("api/payroll/payments")]
[Authorize]
public class PayrollPaymentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPayrollPaymentService _paymentService;

    public PayrollPaymentsController(
        ApplicationDbContext context,
        IPayrollPaymentService paymentService)
    {
        _context = context;
        _paymentService = paymentService;
    }

    /// <summary>
    /// Module 9: Get payment attempt details and failure diagnostics.
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<PayrollPaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentById(int id, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var p = await _context.PayrollPayments
            .Include(pay => pay.PayrollEmployee)
                .ThenInclude(pe => pe.Employee)
            .FirstOrDefaultAsync(pay => pay.Id == id && pay.OrganizationId == orgId, cancellationToken);

        if (p == null)
            return NotFound(ApiResponseDto<PayrollPaymentDto>.Fail("Payment record not found."));

        var dto = PayrollPaymentDto.FromEntity(p);

        return Ok(ApiResponseDto<PayrollPaymentDto>.Ok(dto));
    }

    /// <summary>
    /// Module 9: Manually confirm payment with UTR/transaction reference.
    /// </summary>
    [HttpPost("{id:int}/manual-confirm")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<PayrollPaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ManualConfirmPayment(int id, [FromBody] ManualConfirmPaymentRequestDto request, CancellationToken cancellationToken)
    {
        var empId = User.TryGetEmployeeId() ?? 0;

        if (string.IsNullOrWhiteSpace(request.TransactionReference))
        {
            return BadRequest(ApiResponseDto<PayrollPaymentDto>.Fail("Transaction reference / UTR is required for manual payment confirmation."));
        }

        try
        {
            var command = new HRAttendance.Business.Interfaces.Payroll.ManualConfirmPaymentDto(
                id,
                request.TransactionReference.Trim(),
                request.PaymentDate ?? DateTimeOffset.UtcNow,
                request.Method,
                request.Remarks?.Trim()
            );

            var updated = await _paymentService.ConfirmPaymentManuallyAsync(command, empId, cancellationToken);
            return await GetPaymentById(updated.Id, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<PayrollPaymentDto>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Module 9: Elevated manual override with mandatory reason.
    /// </summary>
    [HttpPost("{id:int}/manual-override")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<PayrollPaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ManualOverridePayment(int id, [FromBody] ManualOverridePaymentRequestDto request, CancellationToken cancellationToken)
    {
        var empId = User.TryGetEmployeeId() ?? 0;

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest(ApiResponseDto<PayrollPaymentDto>.Fail("Override reason is mandatory for manual payment override."));
        }

        try
        {
            var command = new HRAttendance.Business.Interfaces.Payroll.ManualOverridePaymentDto(
                id,
                request.Reason.Trim(),
                string.IsNullOrWhiteSpace(request.TransactionReference) ? $"OVERRIDE-{Guid.NewGuid():N}" : request.TransactionReference.Trim(),
                request.PaymentDate ?? DateTimeOffset.UtcNow,
                request.Method,
                request.Remarks?.Trim()
            );

            var updated = await _paymentService.OverridePaymentAsync(command, empId, cancellationToken);
            return await GetPaymentById(updated.Id, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<PayrollPaymentDto>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Module 9: Record bank transaction UTR or failure message.
    /// </summary>
    [HttpPost("{id:int}/record-result")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecordPaymentResult(int id, [FromBody] RecordPaymentResultRequestDto request, CancellationToken cancellationToken)
    {
        var empId = User.TryGetEmployeeId() ?? 0;

        try
        {
            var command = new HRAttendance.Business.Interfaces.Payroll.RecordPaymentResultDto(
                id,
                request.IsSuccess,
                request.TransactionReference,
                request.FailureReason);

            await _paymentService.RecordPaymentResultAsync(
                command,
                empId,
                cancellationToken);

            return Ok(ApiResponseDto<bool>.Ok(true, "Payment result recorded successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<bool>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Module 9: Spawn retry payment attempt (Idempotent, increments attempt #).
    /// </summary>
    [HttpPost("{id:int}/retry")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<PayrollPaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RetryPayment(int id, [FromBody] RetryPaymentDto request, CancellationToken cancellationToken)
    {
        var empId = User.TryGetEmployeeId() ?? 0;

        try
        {
            var newPayment = await _paymentService.RetryPaymentAsync(
                id,
                request.IdempotencyKey,
                empId,
                cancellationToken);

            return await GetPaymentById(newPayment.Id, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<PayrollPaymentDto>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Module 9: Mark paid transaction as reversed.
    /// </summary>
    [HttpPost("{id:int}/reverse")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReversePayment(int id, [FromBody] string reason, CancellationToken cancellationToken)
    {
        var empId = User.TryGetEmployeeId() ?? 0;

        try
        {
            await _paymentService.ReversePaymentAsync(id, reason, empId, cancellationToken);
            return Ok(ApiResponseDto<bool>.Ok(true, "Payment reversed successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<bool>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Module 9: Cancel pending payment.
    /// </summary>
    [HttpPost("{id:int}/cancel")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelPayment(int id, [FromBody] string reason, CancellationToken cancellationToken)
    {
        var empId = User.TryGetEmployeeId() ?? 0;

        try
        {
            await _paymentService.ReversePaymentAsync(id, reason, empId, cancellationToken);
            return Ok(ApiResponseDto<bool>.Ok(true, "Payment cancelled."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<bool>.Fail(ex.Message));
        }
    }
}
