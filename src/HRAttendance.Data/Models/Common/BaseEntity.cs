namespace HRAttendance.Data.Models.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }
}

public interface IAuditableEntity
{
    int? CreatedBy { get; set; }
    DateTime? CreatedAt { get; set; }
    int? ModifiedBy { get; set; }
    DateTime? ModifiedAt { get; set; }
}

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
}

public abstract class AuditableEntity : BaseEntity, IAuditableEntity, ISoftDelete
{
    public int? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
}
