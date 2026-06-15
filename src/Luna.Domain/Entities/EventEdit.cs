using System.ComponentModel.DataAnnotations;

namespace Luna.Domain.Entities;

public class EventEdit
{
    public int EventId { get; set; }
    public int? NewTypeId { get; set; }

    [MaxLength(100)]
    public string? NewDescription { get; set; }

    [MaxLength(6)]
    public string EndCode { get; set; } = null!;
    public EventType? NewEventType { get; set; }
    public Event Event { get; set; } = null!;
    public ICollection<EventEditExecutor> EventEditExecutors { get; set; } = [];
    public ICollection<EventMemberEdit> MembersEdits { get; set; } = [];
}