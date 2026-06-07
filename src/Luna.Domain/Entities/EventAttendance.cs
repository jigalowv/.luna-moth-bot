namespace Luna.Domain.Entities;

public class EventAttendance
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public bool IsDeafened { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    public EventMember Member { get; set; } = null!;
}