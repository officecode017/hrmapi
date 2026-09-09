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
[Route("api/payroll/bank-exports")]
[Authorize]
public class BankExportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IBankExportService _bankExportService;

    public BankExportsController(
        ApplicationDbContext context,
        IBankExportService bankExportService)
    {
        _context = context;
        _bankExportService = bankExportService;
    }

    /// <summary>
    /// Module 10: Generate bank disbursement export file (CSV_NEFT, HDFC, ICICI).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<BankExportBatchDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateBankExport([FromBody] GenerateBankExportDto request, CancellationToken cancellationToken)
    {
        var empId = User.TryGetEmployeeId() ?? 0;

        try
        {
            var result = await _bankExportService.GenerateExportBatchAsync(
                request.PayrollPeriodId,
                request.Format,
                empId,
                cancellationToken);

            var batch = await _context.BankExportBatches
                .Include(b => b.PayrollPeriod)
                .FirstOrDefaultAsync(b => b.Id == result.BatchId, cancellationToken);

            var dto = new BankExportBatchDto
            {
                Id = result.BatchId,
                OrganizationId = batch?.OrganizationId ?? 0,
                PayrollPeriodId = request.PayrollPeriodId,
                PeriodDescription = batch?.PayrollPeriod?.Description ?? string.Empty,
                BatchNumber = result.BatchNumber,
                Format = result.Format,
                TotalRecords = result.TotalRecords,
                TotalAmount = result.TotalAmount,
                StoragePath = result.FileName,
                ExportedAt = batch?.ExportedAt ?? DateTimeOffset.UtcNow,
                ExportedBy = empId
            };

            return CreatedAtAction(nameof(GetBankExportById), new { id = result.BatchId }, ApiResponseDto<BankExportBatchDto>.Ok(dto, "Bank export generated successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<BankExportBatchDto>.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Module 10: Get export batch metadata and totals.
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(ApiResponseDto<BankExportBatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBankExportById(int id, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();

        var batch = await _context.BankExportBatches
            .Include(b => b.PayrollPeriod)
            .FirstOrDefaultAsync(b => b.Id == id && b.OrganizationId == orgId, cancellationToken);

        if (batch == null)
            return NotFound(ApiResponseDto<BankExportBatchDto>.Fail("Bank export batch not found."));

        var dto = new BankExportBatchDto
        {
            Id = batch.Id,
            OrganizationId = batch.OrganizationId,
            PayrollPeriodId = batch.PayrollPeriodId,
            PeriodDescription = batch.PayrollPeriod?.Description ?? string.Empty,
            BatchNumber = batch.BatchNumber,
            Format = batch.Format,
            TotalRecords = batch.TotalRecords,
            TotalAmount = batch.TotalAmount,
            StoragePath = batch.StoragePath,
            ExportedAt = batch.ExportedAt,
            ExportedBy = batch.ExportedBy
        };

        return Ok(ApiResponseDto<BankExportBatchDto>.Ok(dto));
    }

    /// <summary>
    /// Module 10: Download generated bank transfer file.
    /// </summary>
    [HttpGet("{id:int}/download")]
    [Authorize(Roles = $"{ApplicationRoles.SuperAdmin},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadBankExport(int id, CancellationToken cancellationToken)
    {
        var orgId = User.GetOrganizationIdOrDefault();
        var empId = User.TryGetEmployeeId() ?? 0;

        var batch = await _context.BankExportBatches
            .FirstOrDefaultAsync(b => b.Id == id && b.OrganizationId == orgId, cancellationToken);

        if (batch == null)
            return NotFound(ApiResponseDto<string>.Fail("Bank export batch not found."));

        // Generate the stream for download
        var result = await _bankExportService.GenerateExportBatchAsync(
            batch.PayrollPeriodId,
            batch.Format,
            empId,
            cancellationToken);

        return File(result.FileBytes, result.ContentType, result.FileName);
    }
}
