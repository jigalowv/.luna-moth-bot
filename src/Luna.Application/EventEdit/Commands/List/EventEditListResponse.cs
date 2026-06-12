namespace Luna.Application.EventEdit.Commands.List;

public record EventEditListResponse(
    IReadOnlyCollection<EventEditListResponseItem> Items
);

public record EventEditListResponseItem(
    int EventId,
    string EventTypeTitle,
    ulong CreatorDiscordId,
    DateTime StartAt
);