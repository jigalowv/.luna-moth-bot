using System.ComponentModel.DataAnnotations;

namespace Luna.Domain.Entities;

public class Event
{
    public int Id { get; set; }
    public int TypeId { get; set; }
    public int CreatorId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    [MaxLength(100)]
    public string? Description { get; set; }

    public EventType Type { get; set; } = null!;
    public Executor Creator { get; set; } = null!;
    public ICollection<EventMember> Members { get; set; } = [];
}