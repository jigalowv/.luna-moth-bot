namespace Luna.Domain.Entities;

public class EventEdit
{
    public int EventId { get; set; }
    public int? NewTypeId { get; set; }
    public string EndCode { get; set; } = null!;

    public EventType? NewEventType { get; set; }
    public Event Event { get; set; } = null!;
    public ICollection<EventEditExecutor> Executors { get; set; } = [];
    public ICollection<EventMemberEdit> MembersEdits { get; set; } = [];
}