using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HRAttendance.Data.Models.Document;

namespace HRAttendance.Data.Configurations;

public class EmployeeFolderConfiguration : IEntityTypeConfiguration<EmployeeFolder>
{
    public void Configure(EntityTypeBuilder<EmployeeFolder> builder)
    {
        builder.ToTable("EmployeeFolders");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Path).IsRequired().HasMaxLength(500);

        builder.HasIndex(x => new { x.OrganizationId, x.EmployeeId, x.ParentFolderId, x.Name });

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.EmployeeFolders)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ParentFolder)
            .WithMany(x => x.SubFolders)
            .HasForeignKey(x => x.ParentFolderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class EmployeeDocumentConfiguration : IEntityTypeConfiguration<EmployeeDocument>
{
    public void Configure(EntityTypeBuilder<EmployeeDocument> builder)
    {
        builder.ToTable("EmployeeDocuments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.OriginalFileName).IsRequired().HasMaxLength(260);
        builder.Property(x => x.FileExtension).IsRequired().HasMaxLength(20);
        builder.Property(x => x.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.BlobPath).IsRequired().HasMaxLength(1000);

        builder.HasIndex(x => new { x.OrganizationId, x.EmployeeId, x.FolderId });
        builder.HasIndex(x => new { x.SourceType, x.RelatedEntityId });

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.EmployeeDocuments)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Folder)
            .WithMany(x => x.Documents)
            .HasForeignKey(x => x.FolderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
