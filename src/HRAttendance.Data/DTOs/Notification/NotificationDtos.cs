namespace HRAttendance.Data.DTOs.Notification;

public class NotificationDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int EmployeeId { get; set; }
    public string NotificationHeader { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int NotificationType { get; set; }
    public DateTimeOffset Date { get; set; }
    public bool ReadStatus { get; set; }
    public string? Url { get; set; }
}
