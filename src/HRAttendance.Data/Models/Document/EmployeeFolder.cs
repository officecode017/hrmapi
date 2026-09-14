using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;
using HRAttendance.Data.Models.Employee;

namespace HRAttendance.Data.Models.Document;

public class EmployeeFolder : AuditableEntity
{
    public int OrganizationId { get; set; }
    public int EmployeeId { get; set; }
    public int? ParentFolderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public bool IsSystemFolder { get; set; }
    public FolderType FolderType { get; set; } = FolderType.General;

    // Navigation
    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual Models.Employee.Employee Employee { get; set; } = null!;
    public virtual EmployeeFolder? ParentFolder { get; set; }
    public virtual ICollection<EmployeeFolder> SubFolders { get; set; } = new List<EmployeeFolder>();
    public virtual ICollection<EmployeeDocument> Documents { get; set; } = new List<EmployeeDocument>();
}
