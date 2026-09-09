using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRAttendance.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEnterprisePayrollSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayslipItems_SalaryComponents_SalaryComponentId",
                table: "PayslipItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Payslips_Organizations_OrganizationId",
                table: "Payslips");

            migrationBuilder.DropForeignKey(
                name: "FK_Payslips_PayrollPeriods_PayrollPeriodId",
                table: "Payslips");

            migrationBuilder.DropTable(
                name: "PayslipSubPeriods");

            migrationBuilder.DropIndex(
                name: "IX_Payslips_OrganizationId",
                table: "Payslips");

            migrationBuilder.DropIndex(
                name: "IX_Payslips_PayrollPeriodId_EmployeeId",
                table: "Payslips");

            migrationBuilder.DropIndex(
                name: "IX_PayslipItems_SalaryComponentId",
                table: "PayslipItems");

            migrationBuilder.DropIndex(
                name: "IX_PayrollPeriods_OrganizationId_Month_Year",
                table: "PayrollPeriods");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeSalaryStructures_EmployeeId_EffectiveFrom",
                table: "EmployeeSalaryStructures");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeSalaryStructures_OrganizationId_EmployeeId_Version",
                table: "EmployeeSalaryStructures");

            migrationBuilder.DropColumn(
                name: "AffectsNetPay",
                table: "SalaryComponents");

            migrationBuilder.DropColumn(
                name: "DefaultValue",
                table: "SalaryComponents");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "SalaryComponents");

            migrationBuilder.DropColumn(
                name: "IsPartOfCTC",
                table: "SalaryComponents");

            migrationBuilder.DropColumn(
                name: "IsProrated",
                table: "SalaryComponents");

            migrationBuilder.DropColumn(
                name: "GrossEarnings",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "HasMidMonthRevision",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "LossOfPayDays",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "LossOfPayDeduction",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "NetSalary",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "OffDaysAndHolidays",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "OvertimeHours",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "OvertimePay",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "PaidLeaveDays",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "PaymentReference",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "PresentDays",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "Reimbursements",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "TotalDeductions",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "TotalWorkingDays",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "CalculationNotes",
                table: "PayslipItems");

            migrationBuilder.DropColumn(
                name: "OriginalMonthlyAmount",
                table: "PayslipItems");

            migrationBuilder.DropColumn(
                name: "SalaryComponentId",
                table: "PayslipItems");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "PayrollPeriods");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "EmployeeSalaryStructures");

            migrationBuilder.DropColumn(
                name: "CalculationBase",
                table: "EmployeeSalaryStructureItems");

            migrationBuilder.RenameColumn(
                name: "CalculationBase",
                table: "SalaryComponents",
                newName: "CalculationType");

            migrationBuilder.RenameColumn(
                name: "TotalCalendarDays",
                table: "Payslips",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Remarks",
                table: "Payslips",
                newName: "StoragePath");

            migrationBuilder.RenameColumn(
                name: "PayrollPeriodId",
                table: "Payslips",
                newName: "PayrollEmployeeId");

            migrationBuilder.RenameColumn(
                name: "ProratedAmount",
                table: "PayslipItems",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "TotalEmployees",
                table: "PayrollPeriods",
                newName: "TotalExceptionsCount");

            migrationBuilder.RenameColumn(
                name: "ProcessedAt",
                table: "PayrollPeriods",
                newName: "LockedAt");

            migrationBuilder.RenameColumn(
                name: "IsLocked",
                table: "EmployeeSalaryStructures",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "EmployeeSalaryStructureItems",
                newName: "PercentageRate");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SalaryComponents",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "CalculationOrder",
                table: "SalaryComponents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DocumentHash",
                table: "Payslips",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "GeneratedAt",
                table: "Payslips",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "PayslipNumber",
                table: "Payslips",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ComponentName",
                table: "PayslipItems",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "PayslipItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CalculatedAt",
                table: "PayrollPeriods",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PayrollPeriods",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FinancialYearId",
                table: "PayrollPeriods",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LockedBy",
                table: "PayrollPeriods",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PolicySnapshotJson",
                table: "PayrollPeriods",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PayrollPeriods",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<int>(
                name: "RunType",
                table: "PayrollPeriods",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SequenceNumber",
                table: "PayrollPeriods",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalEmployeesProcessed",
                table: "PayrollPeriods",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalEmployerContributions",
                table: "PayrollPeriods",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "RevisionReason",
                table: "EmployeeSalaryStructures",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "BankExportBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    PayrollPeriodId = table.Column<int>(type: "int", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Format = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TotalRecords = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ExportedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExportedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankExportBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankExportBatches_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BankExportBatches_PayrollPeriods_PayrollPeriodId",
                        column: x => x.PayrollPeriodId,
                        principalTable: "PayrollPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinancialYears",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    YearCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialYears", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialYears_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayrollAdjustments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdjustmentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    PayrollPeriodId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RejectedBy = table.Column<int>(type: "int", nullable: true),
                    RejectedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollAdjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollAdjustments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollAdjustments_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollAdjustments_PayrollPeriods_PayrollPeriodId",
                        column: x => x.PayrollPeriodId,
                        principalTable: "PayrollPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayrollArrears",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArrearNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    SourcePayrollPeriodId = table.Column<int>(type: "int", nullable: false),
                    TargetPayrollPeriodId = table.Column<int>(type: "int", nullable: false),
                    ComponentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OriginalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CorrectAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DifferenceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollArrears", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollArrears_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollArrears_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollArrears_PayrollPeriods_TargetPayrollPeriodId",
                        column: x => x.TargetPayrollPeriodId,
                        principalTable: "PayrollPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayrollEmployees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayrollPeriodId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    CalculationVersion = table.Column<int>(type: "int", nullable: false),
                    EligibleEmploymentDays = table.Column<int>(type: "int", nullable: false),
                    CalendarDaysInMonth = table.Column<int>(type: "int", nullable: false),
                    WorkingDays = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PresentDays = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidLeaveDays = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HalfDays = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LOPDays = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ApprovedOvertimeHours = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BaseMonthlyGross = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProratedGross = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LOPDeduction = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OvertimePay = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AdjustmentsTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ArrearsTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GrossEarnings = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StatutoryDeductions = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OtherDeductions = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EmployerContributions = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NetPay = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CalculatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollEmployees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollEmployees_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollEmployees_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollEmployees_PayrollPeriods_PayrollPeriodId",
                        column: x => x.PayrollPeriodId,
                        principalTable: "PayrollPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayrollExceptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayrollPeriodId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    ErrorCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    ResolvedBy = table.Column<int>(type: "int", nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollExceptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollExceptions_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollExceptions_PayrollPeriods_PayrollPeriodId",
                        column: x => x.PayrollPeriodId,
                        principalTable: "PayrollPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayrollPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    ProrationBasis = table.Column<int>(type: "int", nullable: false),
                    FixedProrationDays = table.Column<int>(type: "int", nullable: false),
                    LOPBasis = table.Column<int>(type: "int", nullable: false),
                    FixedLOPDays = table.Column<int>(type: "int", nullable: false),
                    OTBasis = table.Column<int>(type: "int", nullable: false),
                    OTMultiplier = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StandardMonthlyWorkingHours = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RoundingRule = table.Column<int>(type: "int", nullable: false),
                    ConsiderHolidaysInLOP = table.Column<bool>(type: "bit", nullable: false),
                    ConsiderWeekendsInLOP = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollPolicies_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayslipAccessLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayslipId = table.Column<int>(type: "int", nullable: false),
                    AccessedBy = table.Column<int>(type: "int", nullable: false),
                    AccessedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayslipAccessLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayslipAccessLogs_Payslips_PayslipId",
                        column: x => x.PayslipId,
                        principalTable: "Payslips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatutoryRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    RuleType = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    ConfigurationJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatutoryRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatutoryRules_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayrollItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayrollEmployeeId = table.Column<int>(type: "int", nullable: false),
                    ComponentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ComponentName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ComponentType = table.Column<int>(type: "int", nullable: false),
                    CalculationOrder = table.Column<int>(type: "int", nullable: false),
                    CalculationType = table.Column<int>(type: "int", nullable: false),
                    CalculationBase = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CalculationRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OriginalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProratedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FinalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CalculationFormula = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CalculationNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollItems_PayrollEmployees_PayrollEmployeeId",
                        column: x => x.PayrollEmployeeId,
                        principalTable: "PayrollEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayrollPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayrollEmployeeId = table.Column<int>(type: "int", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    PaymentAttemptNumber = table.Column<int>(type: "int", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Method = table.Column<int>(type: "int", nullable: false),
                    TransactionReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MaskedAccountNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    IFSCCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    InitiatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PaidAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ReversedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollPayments_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollPayments_PayrollEmployees_PayrollEmployeeId",
                        column: x => x.PayrollEmployeeId,
                        principalTable: "PayrollEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayrollSalarySlices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayrollEmployeeId = table.Column<int>(type: "int", nullable: false),
                    SalaryStructureId = table.Column<int>(type: "int", nullable: false),
                    SalaryStructureVersion = table.Column<int>(type: "int", nullable: false),
                    SliceStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SliceEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalCalendarDaysInSlice = table.Column<int>(type: "int", nullable: false),
                    EligibleDaysInSlice = table.Column<int>(type: "int", nullable: false),
                    MonthlyGrossInSlice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProratedGrossInSlice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SliceNotes = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollSalarySlices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollSalarySlices_EmployeeSalaryStructures_SalaryStructureId",
                        column: x => x.SalaryStructureId,
                        principalTable: "EmployeeSalaryStructures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollSalarySlices_PayrollEmployees_PayrollEmployeeId",
                        column: x => x.PayrollEmployeeId,
                        principalTable: "PayrollEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payslips_PayrollEmployeeId",
                table: "Payslips",
                column: "PayrollEmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriods_FinancialYearId",
                table: "PayrollPeriods",
                column: "FinancialYearId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriods_OrganizationId_Year_Month_RunType_SequenceNumber",
                table: "PayrollPeriods",
                columns: new[] { "OrganizationId", "Year", "Month", "RunType", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalaryStructures_EmployeeId_Version",
                table: "EmployeeSalaryStructures",
                columns: new[] { "EmployeeId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalaryStructures_OrganizationId",
                table: "EmployeeSalaryStructures",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_BankExportBatches_OrganizationId",
                table: "BankExportBatches",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_BankExportBatches_PayrollPeriodId",
                table: "BankExportBatches",
                column: "PayrollPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialYears_OrganizationId_YearCode",
                table: "FinancialYears",
                columns: new[] { "OrganizationId", "YearCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollAdjustments_EmployeeId",
                table: "PayrollAdjustments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollAdjustments_OrganizationId_AdjustmentNumber",
                table: "PayrollAdjustments",
                columns: new[] { "OrganizationId", "AdjustmentNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollAdjustments_PayrollPeriodId",
                table: "PayrollAdjustments",
                column: "PayrollPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollArrears_EmployeeId",
                table: "PayrollArrears",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollArrears_OrganizationId_ArrearNumber",
                table: "PayrollArrears",
                columns: new[] { "OrganizationId", "ArrearNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollArrears_TargetPayrollPeriodId",
                table: "PayrollArrears",
                column: "TargetPayrollPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollEmployees_EmployeeId",
                table: "PayrollEmployees",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollEmployees_OrganizationId_PayrollPeriodId_EmployeeId",
                table: "PayrollEmployees",
                columns: new[] { "OrganizationId", "PayrollPeriodId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollEmployees_PayrollPeriodId",
                table: "PayrollEmployees",
                column: "PayrollPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollExceptions_EmployeeId",
                table: "PayrollExceptions",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollExceptions_PayrollPeriodId",
                table: "PayrollExceptions",
                column: "PayrollPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollItems_PayrollEmployeeId",
                table: "PayrollItems",
                column: "PayrollEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPayments_OrganizationId_IdempotencyKey",
                table: "PayrollPayments",
                columns: new[] { "OrganizationId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPayments_PayrollEmployeeId",
                table: "PayrollPayments",
                column: "PayrollEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPolicies_OrganizationId",
                table: "PayrollPolicies",
                column: "OrganizationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollSalarySlices_PayrollEmployeeId_SliceStartDate_SliceEndDate",
                table: "PayrollSalarySlices",
                columns: new[] { "PayrollEmployeeId", "SliceStartDate", "SliceEndDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollSalarySlices_SalaryStructureId",
                table: "PayrollSalarySlices",
                column: "SalaryStructureId");

            migrationBuilder.CreateIndex(
                name: "IX_PayslipAccessLogs_PayslipId",
                table: "PayslipAccessLogs",
                column: "PayslipId");

            migrationBuilder.CreateIndex(
                name: "IX_StatutoryRules_OrganizationId_RuleType_Version",
                table: "StatutoryRules",
                columns: new[] { "OrganizationId", "RuleType", "Version" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PayrollPeriods_FinancialYears_FinancialYearId",
                table: "PayrollPeriods",
                column: "FinancialYearId",
                principalTable: "FinancialYears",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payslips_PayrollEmployees_PayrollEmployeeId",
                table: "Payslips",
                column: "PayrollEmployeeId",
                principalTable: "PayrollEmployees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayrollPeriods_FinancialYears_FinancialYearId",
                table: "PayrollPeriods");

            migrationBuilder.DropForeignKey(
                name: "FK_Payslips_PayrollEmployees_PayrollEmployeeId",
                table: "Payslips");

            migrationBuilder.DropTable(
                name: "BankExportBatches");

            migrationBuilder.DropTable(
                name: "FinancialYears");

            migrationBuilder.DropTable(
                name: "PayrollAdjustments");

            migrationBuilder.DropTable(
                name: "PayrollArrears");

            migrationBuilder.DropTable(
                name: "PayrollExceptions");

            migrationBuilder.DropTable(
                name: "PayrollItems");

            migrationBuilder.DropTable(
                name: "PayrollPayments");

            migrationBuilder.DropTable(
                name: "PayrollPolicies");

            migrationBuilder.DropTable(
                name: "PayrollSalarySlices");

            migrationBuilder.DropTable(
                name: "PayslipAccessLogs");

            migrationBuilder.DropTable(
                name: "StatutoryRules");

            migrationBuilder.DropTable(
                name: "PayrollEmployees");

            migrationBuilder.DropIndex(
                name: "IX_Payslips_PayrollEmployeeId",
                table: "Payslips");

            migrationBuilder.DropIndex(
                name: "IX_PayrollPeriods_FinancialYearId",
                table: "PayrollPeriods");

            migrationBuilder.DropIndex(
                name: "IX_PayrollPeriods_OrganizationId_Year_Month_RunType_SequenceNumber",
                table: "PayrollPeriods");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeSalaryStructures_EmployeeId_Version",
                table: "EmployeeSalaryStructures");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeSalaryStructures_OrganizationId",
                table: "EmployeeSalaryStructures");

            migrationBuilder.DropColumn(
                name: "CalculationOrder",
                table: "SalaryComponents");

            migrationBuilder.DropColumn(
                name: "DocumentHash",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "GeneratedAt",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "PayslipNumber",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "PayslipItems");

            migrationBuilder.DropColumn(
                name: "CalculatedAt",
                table: "PayrollPeriods");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "PayrollPeriods");

            migrationBuilder.DropColumn(
                name: "FinancialYearId",
                table: "PayrollPeriods");

            migrationBuilder.DropColumn(
                name: "LockedBy",
                table: "PayrollPeriods");

            migrationBuilder.DropColumn(
                name: "PolicySnapshotJson",
                table: "PayrollPeriods");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PayrollPeriods");

            migrationBuilder.DropColumn(
                name: "RunType",
                table: "PayrollPeriods");

            migrationBuilder.DropColumn(
                name: "SequenceNumber",
                table: "PayrollPeriods");

            migrationBuilder.DropColumn(
                name: "TotalEmployeesProcessed",
                table: "PayrollPeriods");

            migrationBuilder.DropColumn(
                name: "TotalEmployerContributions",
                table: "PayrollPeriods");

            migrationBuilder.RenameColumn(
                name: "CalculationType",
                table: "SalaryComponents",
                newName: "CalculationBase");

            migrationBuilder.RenameColumn(
                name: "StoragePath",
                table: "Payslips",
                newName: "Remarks");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Payslips",
                newName: "TotalCalendarDays");

            migrationBuilder.RenameColumn(
                name: "PayrollEmployeeId",
                table: "Payslips",
                newName: "PayrollPeriodId");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "PayslipItems",
                newName: "ProratedAmount");

            migrationBuilder.RenameColumn(
                name: "TotalExceptionsCount",
                table: "PayrollPeriods",
                newName: "TotalEmployees");

            migrationBuilder.RenameColumn(
                name: "LockedAt",
                table: "PayrollPeriods",
                newName: "ProcessedAt");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "EmployeeSalaryStructures",
                newName: "IsLocked");

            migrationBuilder.RenameColumn(
                name: "PercentageRate",
                table: "EmployeeSalaryStructureItems",
                newName: "Value");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SalaryComponents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AddColumn<bool>(
                name: "AffectsNetPay",
                table: "SalaryComponents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "DefaultValue",
                table: "SalaryComponents",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SalaryComponents",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPartOfCTC",
                table: "SalaryComponents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsProrated",
                table: "SalaryComponents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossEarnings",
                table: "Payslips",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "HasMidMonthRevision",
                table: "Payslips",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Payslips",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LossOfPayDays",
                table: "Payslips",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LossOfPayDeduction",
                table: "Payslips",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetSalary",
                table: "Payslips",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OffDaysAndHolidays",
                table: "Payslips",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Payslips",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "OvertimeHours",
                table: "Payslips",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OvertimePay",
                table: "Payslips",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PaidAt",
                table: "Payslips",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidLeaveDays",
                table: "Payslips",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Payslips",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentReference",
                table: "Payslips",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PresentDays",
                table: "Payslips",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Reimbursements",
                table: "Payslips",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDeductions",
                table: "Payslips",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalWorkingDays",
                table: "Payslips",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "ComponentName",
                table: "PayslipItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AddColumn<string>(
                name: "CalculationNotes",
                table: "PayslipItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalMonthlyAmount",
                table: "PayslipItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SalaryComponentId",
                table: "PayslipItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "PayrollPeriods",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RevisionReason",
                table: "EmployeeSalaryStructures",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "EmployeeSalaryStructures",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CalculationBase",
                table: "EmployeeSalaryStructureItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PayslipSubPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeSalaryStructureId = table.Column<int>(type: "int", nullable: false),
                    PayslipId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    DaysInSlice = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LossOfPayDaysInSlice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    PayableDaysInSlice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProratedDeductions = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProratedGross = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProratedNet = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StructureVersion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayslipSubPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayslipSubPeriods_EmployeeSalaryStructures_EmployeeSalaryStructureId",
                        column: x => x.EmployeeSalaryStructureId,
                        principalTable: "EmployeeSalaryStructures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayslipSubPeriods_Payslips_PayslipId",
                        column: x => x.PayslipId,
                        principalTable: "Payslips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payslips_OrganizationId",
                table: "Payslips",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Payslips_PayrollPeriodId_EmployeeId",
                table: "Payslips",
                columns: new[] { "PayrollPeriodId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayslipItems_SalaryComponentId",
                table: "PayslipItems",
                column: "SalaryComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriods_OrganizationId_Month_Year",
                table: "PayrollPeriods",
                columns: new[] { "OrganizationId", "Month", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalaryStructures_EmployeeId_EffectiveFrom",
                table: "EmployeeSalaryStructures",
                columns: new[] { "EmployeeId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalaryStructures_OrganizationId_EmployeeId_Version",
                table: "EmployeeSalaryStructures",
                columns: new[] { "OrganizationId", "EmployeeId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayslipSubPeriods_EmployeeSalaryStructureId",
                table: "PayslipSubPeriods",
                column: "EmployeeSalaryStructureId");

            migrationBuilder.CreateIndex(
                name: "IX_PayslipSubPeriods_PayslipId",
                table: "PayslipSubPeriods",
                column: "PayslipId");

            migrationBuilder.AddForeignKey(
                name: "FK_PayslipItems_SalaryComponents_SalaryComponentId",
                table: "PayslipItems",
                column: "SalaryComponentId",
                principalTable: "SalaryComponents",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Payslips_Organizations_OrganizationId",
                table: "Payslips",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payslips_PayrollPeriods_PayrollPeriodId",
                table: "Payslips",
                column: "PayrollPeriodId",
                principalTable: "PayrollPeriods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
