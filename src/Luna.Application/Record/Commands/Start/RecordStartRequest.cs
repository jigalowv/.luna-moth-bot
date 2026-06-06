using ErrorOr;
using MediatR;

namespace Luna.Application.Record.Commands.Start;

public record RecordStartRequest(
    ulong ExecutorDiscordId,
    IReadOnlyDictionary<ulong, bool> IdAndIsDeafenPairs,
    ulong ChannelId
) : IRequest<ErrorOr<bool>>;