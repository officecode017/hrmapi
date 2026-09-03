using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HRAttendance.Data.Models.Overtime;

namespace HRAttendance.Data.Configurations;

public class OTSettingConfiguration : IEntityTypeConfiguration<OTSetting>
{
    public void Configure(EntityTypeBuilder<OTSetting> builder)
    {
        builder.ToTable("OTSettings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);

        builder.Property(x => x.Multiplier).HasPrecision(5, 2);
        builder.Property(x => x.MaxOTHoursPerDay).HasPrecision(5, 2);
        builder.Property(x => x.MaxOTHoursPerWeek).HasPrecision(5, 2);
        builder.Property(x => x.MaxOTHoursPerMonth).HasPrecision(5, 2);

        builder.HasOne(x => x.Organization)
            .WithMany(x => x.OTSettings)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class OTEntryConfiguration : IEntityTypeConfiguration<OTEntry>
{
    public void Configure(EntityTypeBuilder<OTEntry> builder)
    {
        builder.ToTable("OTEntries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OTHours).HasPrecision(5, 2);
        builder.Property(x => x.MultiplierApplied).HasPrecision(5, 2);
        builder.Property(x => x.HourlyRate).HasPrecision(18, 2);
        builder.Property(x => x.OTAmount).HasPrecision(18, 2);

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.OTEntries)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.OTSetting)
            .WithMany(x => x.OTEntries)
            .HasForeignKey(x => x.OTSettingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
