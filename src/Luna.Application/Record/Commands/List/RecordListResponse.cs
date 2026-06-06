namespace Luna.Application.Record.Commands.List;

public record RecordListResponse(
    IReadOnlyCollection<RecordListItem> Items
);

public record RecordListItem(
    DateTime StartAt,
    ulong ChannelId,
    ulong ExecutorDiscordId 
);