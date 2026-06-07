namespace Luna.Domain.Entities;

public sealed class EventType
{
    private EventType() { }

    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }

    public ICollection<Event> Events { get; set; } = [];

    public static EventType Create(string title)
    {
        return new EventType
        {
            Title = title
        };
    }
}