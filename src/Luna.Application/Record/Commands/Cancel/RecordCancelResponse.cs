namespace Luna.Application.Record.Commands.Cancel;

public record RecordCancelResponse(
    IReadOnlyDictionary<ulong, TimeSpan> UserDeafenDurations,
    IReadOnlyDictionary<ulong, TimeSpan> UserNotDeafenDurations
);