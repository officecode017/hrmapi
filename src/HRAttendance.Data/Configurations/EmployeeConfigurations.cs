using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HRAttendance.Data.Models.Employee;

namespace HRAttendance.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EmployeeCode).IsRequired().HasMaxLength(50);
        builder.Property(x => x.FirstName).HasMaxLength(100);
        builder.Property(x => x.LastName).HasMaxLength(100);
        builder.Property(x => x.PasswordHash).HasMaxLength(255);
        builder.Property(x => x.PhotoPath).HasMaxLength(500);
        builder.Property(x => x.BackgroundImagePath).HasMaxLength(500);

        builder.HasIndex(x => new { x.OrganizationId, x.EmployeeCode }).IsUnique();

        builder.HasOne(x => x.Organization)
            .WithMany(x => x.Employees)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ContactDetails)
            .WithOne(x => x.Employee)
            .HasForeignKey<EmployeeContactDetails>(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ProfessionalDetails)
            .WithOne(x => x.Employee)
            .HasForeignKey<EmployeeProfessionalDetails>(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


public class EmployeeContactDetailsConfiguration : IEntityTypeConfiguration<EmployeeContactDetails>
{
    public void Configure(EntityTypeBuilder<EmployeeContactDetails> builder)
    {
        builder.ToTable("EmployeeContactDetails");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.EmployeeId).IsUnique();
        builder.Property(x => x.WorkEmail).HasMaxLength(150);
        builder.Property(x => x.Mobile).HasMaxLength(30);
        builder.Property(x => x.HomeLatitude).HasPrecision(10, 7);
        builder.Property(x => x.HomeLongitude).HasPrecision(10, 7);

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class EmployeeProfessionalDetailsConfiguration : IEntityTypeConfiguration<EmployeeProfessionalDetails>
{
    public void Configure(EntityTypeBuilder<EmployeeProfessionalDetails> builder)
    {
        builder.ToTable("EmployeeProfessionalDetails");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.EmployeeId).IsUnique();

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Department)
            .WithMany(x => x.EmployeeProfessionalDetails)
            .HasForeignKey(x => x.DepartmentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Designation)
            .WithMany(x => x.EmployeeProfessionalDetails)
            .HasForeignKey(x => x.DesignationId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Location)
            .WithMany(x => x.EmployeeProfessionalDetails)
            .HasForeignKey(x => x.LocationId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Shift)
            .WithMany(x => x.EmployeeProfessionalDetails)
            .HasForeignKey(x => x.ShiftId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Manager)
            .WithMany(x => x.DirectReports)
            .HasForeignKey(x => x.ReportingTo)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
