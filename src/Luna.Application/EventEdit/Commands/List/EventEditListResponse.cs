namespace Luna.Application.EventEdit.Commands.List;

public record EventEditListResponse(
    IReadOnlyCollection<EventEditListResponseItem> Items
);

public record EventEditListResponseItem(
    int EventId,
    int TempEventId,
    string EventTypeTitle,
    ulong CreatorDiscordId,
    DateTime StartAt
);