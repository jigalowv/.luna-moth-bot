namespace Luna.Application.Record.Commands.Users;

public record RecordUsersResponse(
    ulong ChannelId,
    IReadOnlyDictionary<ulong, TimeSpan> UserDeafenDurations,
    IReadOnlyDictionary<ulong, TimeSpan> UserNotDeafenDurations
);