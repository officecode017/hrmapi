using HRAttendance.Data.Models.Common;
using HRAttendance.Data.Models.Organization;

namespace HRAttendance.Data.Models.Security;

public class PermissionMaster : AuditableEntity
{
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }

    public virtual Organization.Organization Organization { get; set; } = null!;
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
