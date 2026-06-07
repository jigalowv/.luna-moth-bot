using Luna.Domain.Enums;

namespace Luna.Domain.Entities;

public class EventMember
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int EventId { get; set; }
    public bool IsActive { get; set; }
    public MemberRole Role { get; set; }

    public User User { get; set; } = null!;
    public Event Event { get; set; } = null!;
    public ICollection<EventAttendance> Attendances { get; set; } = [];
}