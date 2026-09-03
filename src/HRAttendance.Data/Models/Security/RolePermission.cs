using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;

namespace HRAttendance.Data.Models.Security;

public class RolePermission : AuditableEntity
{
    public int OrganizationId { get; set; }
    public int RoleId { get; set; }
    public int PermissionId { get; set; }

    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
    public virtual PermissionMaster Permission { get; set; } = null!;
}
