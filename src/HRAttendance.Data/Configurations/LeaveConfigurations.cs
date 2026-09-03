using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HRAttendance.Data.Models.Leave;

namespace HRAttendance.Data.Configurations;

public class LeaveTypeConfiguration : IEntityTypeConfiguration<LeaveType>
{
    public void Configure(EntityTypeBuilder<LeaveType> builder)
    {
        builder.ToTable("LeaveTypes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);

        builder.HasOne(x => x.Organization)
            .WithMany(x => x.LeaveTypes)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LeaveSetting)
            .WithOne(x => x.LeaveType)
            .HasForeignKey<LeaveSetting>(x => x.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class LeaveSettingConfiguration : IEntityTypeConfiguration<LeaveSetting>
{
    public void Configure(EntityTypeBuilder<LeaveSetting> builder)
    {
        builder.ToTable("LeaveSettings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.InitialValueDuringProbation).HasPrecision(18, 2);
        builder.Property(x => x.CarryForwardLeaveCount).HasPrecision(18, 2);
        builder.Property(x => x.MaximumAccrualYearlyLeave).HasPrecision(18, 2);
        builder.Property(x => x.Leaves).HasPrecision(18, 2);
        builder.Property(x => x.MaxLeaveMonthly).HasPrecision(18, 2);

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class LeaveApplicationConfiguration : IEntityTypeConfiguration<LeaveApplication>
{
    public void Configure(EntityTypeBuilder<LeaveApplication> builder)
    {
        builder.ToTable("LeaveApplications");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.NoOfLeave).HasPrecision(18, 2);
        builder.Property(x => x.Status).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Reason).HasMaxLength(500);

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.LeaveApplications)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LeaveType)
            .WithMany(x => x.LeaveApplications)
            .HasForeignKey(x => x.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Approver)
            .WithMany()
            .HasForeignKey(x => x.ApprovedBy)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SubLeaveApplicationConfiguration : IEntityTypeConfiguration<SubLeaveApplication>
{
    public void Configure(EntityTypeBuilder<SubLeaveApplication> builder)
    {
        builder.ToTable("SubLeaveApplications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).IsRequired().HasMaxLength(50);

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LeaveApplication)
            .WithMany(x => x.SubLeaveApplications)
            .HasForeignKey(x => x.LeaveApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.SubLeaveApplications)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Approver)
            .WithMany()
            .HasForeignKey(x => x.ApprovedBy)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class EmployeeLeaveConfiguration : IEntityTypeConfiguration<EmployeeLeave>
{
    public void Configure(EntityTypeBuilder<EmployeeLeave> builder)
    {
        builder.ToTable("EmployeeLeaves");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LeaveCredited).HasPrecision(18, 2);
        builder.Property(x => x.LeaveBroughtForward).HasPrecision(18, 2);
        builder.Property(x => x.LeavesTaken).HasPrecision(18, 2);

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.EmployeeLeaves)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LeaveType)
            .WithMany(x => x.EmployeeLeaves)
            .HasForeignKey(x => x.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AcademicYear)
            .WithMany(x => x.EmployeeLeaves)
            .HasForeignKey(x => x.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
