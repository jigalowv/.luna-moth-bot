namespace Luna.Domain.Entities;

public class EventEdit
{
    public int EventId { get; set; }
    public int TempEventId { get; set; }
    public string EndCode { get; set; } = null!;

    public Event Event { get; set; } = null!;
    public Event TempEvent { get; set; } = null!;
    public ICollection<EventEditExecutor> Executors { get; set; } = [];
}