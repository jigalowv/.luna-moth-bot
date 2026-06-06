namespace Luna.Domain.Entities;

public class EventType
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public ICollection<Event> Events { get; set; } = [];
}