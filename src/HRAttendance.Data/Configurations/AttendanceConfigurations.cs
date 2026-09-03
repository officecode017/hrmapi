using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HRAttendance.Data.Models.Attendance;

namespace HRAttendance.Data.Configurations;

public class EmployeeAttendanceConfiguration : IEntityTypeConfiguration<EmployeeAttendance>
{
    public void Configure(EntityTypeBuilder<EmployeeAttendance> builder)
    {
        builder.ToTable("EmployeeAttendances");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DayTotal).HasPrecision(18, 2);
        builder.Property(x => x.InTimeLatitude).HasPrecision(10, 7);
        builder.Property(x => x.InTimeLongitude).HasPrecision(10, 7);
        builder.Property(x => x.OutTimeLatitude).HasPrecision(10, 7);
        builder.Property(x => x.OutTimeLongitude).HasPrecision(10, 7);
        builder.Property(x => x.Remark).HasMaxLength(500);

        builder.HasOne(x => x.Organization)
            .WithMany(x => x.EmployeeAttendances)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.EmployeeAttendances)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Location)
            .WithMany(x => x.EmployeeAttendances)
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Shift)
            .WithMany(x => x.EmployeeAttendances)
            .HasForeignKey(x => x.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AcademicYear)
            .WithMany(x => x.EmployeeAttendances)
            .HasForeignKey(x => x.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
