using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Data.Configurations;

public class FinancialYearConfiguration : IEntityTypeConfiguration<FinancialYear>
{
    public void Configure(EntityTypeBuilder<FinancialYear> builder)
    {
        builder.ToTable("FinancialYears");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.YearCode).HasMaxLength(20).IsRequired();

        builder.HasIndex(x => new { x.OrganizationId, x.YearCode }).IsUnique();

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PayrollPolicyConfiguration : IEntityTypeConfiguration<PayrollPolicy>
{
    public void Configure(EntityTypeBuilder<PayrollPolicy> builder)
    {
        builder.ToTable("PayrollPolicies");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OTMultiplier).HasPrecision(18, 2);
        builder.Property(x => x.StandardMonthlyWorkingHours).HasPrecision(18, 2);

        builder.HasIndex(x => x.OrganizationId).IsUnique();

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class StatutoryRuleConfiguration : IEntityTypeConfiguration<StatutoryRule>
{
    public void Configure(EntityTypeBuilder<StatutoryRule> builder)
    {
        builder.ToTable("StatutoryRules");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.OrganizationId, x.RuleType, x.Version }).IsUnique();

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SalaryComponentConfiguration : IEntityTypeConfiguration<SalaryComponent>
{
    public void Configure(EntityTypeBuilder<SalaryComponent> builder)
    {
        builder.ToTable("SalaryComponents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();

        builder.HasIndex(x => new { x.OrganizationId, x.Code }).IsUnique();

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class EmployeeSalaryStructureConfiguration : IEntityTypeConfiguration<EmployeeSalaryStructure>
{
    public void Configure(EntityTypeBuilder<EmployeeSalaryStructure> builder)
    {
        builder.ToTable("EmployeeSalaryStructures");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.MonthlyGrossSalary).HasPrecision(18, 2);
        builder.Property(x => x.AnnualCTC).HasPrecision(18, 2);
        builder.Property(x => x.RevisionReason).HasMaxLength(500);

        builder.HasIndex(x => new { x.EmployeeId, x.Version }).IsUnique();

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Employee)
            .WithMany(e => e.SalaryStructures)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class EmployeeSalaryStructureItemConfiguration : IEntityTypeConfiguration<EmployeeSalaryStructureItem>
{
    public void Configure(EntityTypeBuilder<EmployeeSalaryStructureItem> builder)
    {
        builder.ToTable("EmployeeSalaryStructureItems");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.MonthlyAmount).HasPrecision(18, 2);
        builder.Property(x => x.AnnualAmount).HasPrecision(18, 2);
        builder.Property(x => x.PercentageRate).HasPrecision(18, 2);

        builder.HasOne(x => x.Structure)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.EmployeeSalaryStructureId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Component)
            .WithMany(x => x.StructureItems)
            .HasForeignKey(x => x.SalaryComponentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PayrollPeriodConfiguration : IEntityTypeConfiguration<PayrollPeriod>
{
    public void Configure(EntityTypeBuilder<PayrollPeriod> builder)
    {
        builder.ToTable("PayrollPeriods");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Description).HasMaxLength(250);

        builder.Property(x => x.TotalGrossPay).HasPrecision(18, 2);
        builder.Property(x => x.TotalDeductions).HasPrecision(18, 2);
        builder.Property(x => x.TotalEmployerContributions).HasPrecision(18, 2);
        builder.Property(x => x.TotalNetPay).HasPrecision(18, 2);

        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => new { x.OrganizationId, x.Year, x.Month, x.RunType, x.SequenceNumber })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FinancialYear)
            .WithMany(x => x.PayrollPeriods)
            .HasForeignKey(x => x.FinancialYearId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PayrollEmployeeConfiguration : IEntityTypeConfiguration<PayrollEmployee>
{
    public void Configure(EntityTypeBuilder<PayrollEmployee> builder)
    {
        builder.ToTable("PayrollEmployees");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.WorkingDays).HasPrecision(18, 2);
        builder.Property(x => x.PresentDays).HasPrecision(18, 2);
        builder.Property(x => x.PaidLeaveDays).HasPrecision(18, 2);
        builder.Property(x => x.HalfDays).HasPrecision(18, 2);
        builder.Property(x => x.LOPDays).HasPrecision(18, 2);
        builder.Property(x => x.ApprovedOvertimeHours).HasPrecision(18, 2);

        builder.Property(x => x.BaseMonthlyGross).HasPrecision(18, 2);
        builder.Property(x => x.ProratedGross).HasPrecision(18, 2);
        builder.Property(x => x.LOPDeduction).HasPrecision(18, 2);
        builder.Property(x => x.OvertimePay).HasPrecision(18, 2);
        builder.Property(x => x.AdjustmentsTotal).HasPrecision(18, 2);
        builder.Property(x => x.ArrearsTotal).HasPrecision(18, 2);
        builder.Property(x => x.GrossEarnings).HasPrecision(18, 2);
        builder.Property(x => x.StatutoryDeductions).HasPrecision(18, 2);
        builder.Property(x => x.OtherDeductions).HasPrecision(18, 2);
        builder.Property(x => x.TotalDeductions).HasPrecision(18, 2);
        builder.Property(x => x.EmployerContributions).HasPrecision(18, 2);
        builder.Property(x => x.NetPay).HasPrecision(18, 2);

        builder.HasIndex(x => new { x.OrganizationId, x.PayrollPeriodId, x.EmployeeId }).IsUnique();

        builder.HasOne(x => x.Period)
            .WithMany(x => x.Employees)
            .HasForeignKey(x => x.PayrollPeriodId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Employee)
            .WithMany(e => e.PayrollSnapshots)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PayrollSalarySliceConfiguration : IEntityTypeConfiguration<PayrollSalarySlice>
{
    public void Configure(EntityTypeBuilder<PayrollSalarySlice> builder)
    {
        builder.ToTable("PayrollSalarySlices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SliceNotes).HasMaxLength(250);

        builder.Property(x => x.MonthlyGrossInSlice).HasPrecision(18, 2);
        builder.Property(x => x.ProratedGrossInSlice).HasPrecision(18, 2);

        builder.HasIndex(x => new { x.PayrollEmployeeId, x.SliceStartDate, x.SliceEndDate }).IsUnique();

        builder.HasOne(x => x.PayrollEmployee)
            .WithMany(x => x.Slices)
            .HasForeignKey(x => x.PayrollEmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SalaryStructure)
            .WithMany(x => x.SalarySlices)
            .HasForeignKey(x => x.SalaryStructureId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PayrollItemConfiguration : IEntityTypeConfiguration<PayrollItem>
{
    public void Configure(EntityTypeBuilder<PayrollItem> builder)
    {
        builder.ToTable("PayrollItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ComponentCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ComponentName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.CalculationFormula).HasMaxLength(500);
        builder.Property(x => x.CalculationNotes).HasMaxLength(500);

        builder.Property(x => x.CalculationBase).HasPrecision(18, 2);
        builder.Property(x => x.CalculationRate).HasPrecision(18, 2);
        builder.Property(x => x.OriginalAmount).HasPrecision(18, 2);
        builder.Property(x => x.ProratedAmount).HasPrecision(18, 2);
        builder.Property(x => x.FinalAmount).HasPrecision(18, 2);

        builder.HasOne(x => x.PayrollEmployee)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.PayrollEmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PayrollAdjustmentConfiguration : IEntityTypeConfiguration<PayrollAdjustment>
{
    public void Configure(EntityTypeBuilder<PayrollAdjustment> builder)
    {
        builder.ToTable("PayrollAdjustments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AdjustmentNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        builder.Property(x => x.RejectionReason).HasMaxLength(500);

        builder.Property(x => x.Amount).HasPrecision(18, 2);

        builder.HasIndex(x => new { x.OrganizationId, x.AdjustmentNumber }).IsUnique();

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Employee)
            .WithMany(e => e.PayrollAdjustments)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PayrollPeriod)
            .WithMany(x => x.Adjustments)
            .HasForeignKey(x => x.PayrollPeriodId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PayrollArrearConfiguration : IEntityTypeConfiguration<PayrollArrear>
{
    public void Configure(EntityTypeBuilder<PayrollArrear> builder)
    {
        builder.ToTable("PayrollArrears");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ArrearNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ComponentCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();

        builder.Property(x => x.OriginalAmount).HasPrecision(18, 2);
        builder.Property(x => x.CorrectAmount).HasPrecision(18, 2);
        builder.Property(x => x.DifferenceAmount).HasPrecision(18, 2);

        builder.HasIndex(x => new { x.OrganizationId, x.ArrearNumber }).IsUnique();

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Employee)
            .WithMany(e => e.PayrollArrears)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TargetPayrollPeriod)
            .WithMany(x => x.ArrearsTargetingPeriod)
            .HasForeignKey(x => x.TargetPayrollPeriodId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PayrollExceptionConfiguration : IEntityTypeConfiguration<PayrollException>
{
    public void Configure(EntityTypeBuilder<PayrollException> builder)
    {
        builder.ToTable("PayrollExceptions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ErrorCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ResolutionNotes).HasMaxLength(500);

        builder.HasOne(x => x.PayrollPeriod)
            .WithMany(x => x.Exceptions)
            .HasForeignKey(x => x.PayrollPeriodId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PayslipConfiguration : IEntityTypeConfiguration<Payslip>
{
    public void Configure(EntityTypeBuilder<Payslip> builder)
    {
        builder.ToTable("Payslips");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PayslipNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.StoragePath).HasMaxLength(500);
        builder.Property(x => x.DocumentHash).HasMaxLength(128);

        builder.HasIndex(x => x.PayrollEmployeeId).IsUnique();

        builder.HasOne(x => x.PayrollEmployee)
            .WithOne(x => x.Payslip)
            .HasForeignKey<Payslip>(x => x.PayrollEmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Employee)
            .WithMany(e => e.Payslips)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PayslipItemConfiguration : IEntityTypeConfiguration<PayslipItem>
{
    public void Configure(EntityTypeBuilder<PayslipItem> builder)
    {
        builder.ToTable("PayslipItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ComponentCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ComponentName).HasMaxLength(150).IsRequired();

        builder.Property(x => x.Amount).HasPrecision(18, 2);

        builder.HasOne(x => x.Payslip)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.PayslipId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PayslipAccessLogConfiguration : IEntityTypeConfiguration<PayslipAccessLog>
{
    public void Configure(EntityTypeBuilder<PayslipAccessLog> builder)
    {
        builder.ToTable("PayslipAccessLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.IpAddress).HasMaxLength(50);
        builder.Property(x => x.UserAgent).HasMaxLength(300);

        builder.HasOne(x => x.Payslip)
            .WithMany(x => x.AccessLogs)
            .HasForeignKey(x => x.PayslipId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PayrollPaymentConfiguration : IEntityTypeConfiguration<PayrollPayment>
{
    public void Configure(EntityTypeBuilder<PayrollPayment> builder)
    {
        builder.ToTable("PayrollPayments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.IdempotencyKey).HasMaxLength(100).IsRequired();
        builder.Property(x => x.TransactionReference).HasMaxLength(100);
        builder.Property(x => x.BankName).HasMaxLength(100);
        builder.Property(x => x.MaskedAccountNumber).HasMaxLength(30);
        builder.Property(x => x.IFSCCode).HasMaxLength(20);
        builder.Property(x => x.FailureReason).HasMaxLength(500);

        builder.Property(x => x.Amount).HasPrecision(18, 2);

        builder.HasIndex(x => new { x.OrganizationId, x.IdempotencyKey }).IsUnique();

        builder.HasOne(x => x.PayrollEmployee)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.PayrollEmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class BankExportBatchConfiguration : IEntityTypeConfiguration<BankExportBatch>
{
    public void Configure(EntityTypeBuilder<BankExportBatch> builder)
    {
        builder.ToTable("BankExportBatches");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BatchNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Format).HasMaxLength(30).IsRequired();
        builder.Property(x => x.StoragePath).HasMaxLength(500).IsRequired();

        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PayrollPeriod)
            .WithMany(x => x.BankExports)
            .HasForeignKey(x => x.PayrollPeriodId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
