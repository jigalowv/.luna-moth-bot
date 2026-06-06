using ErrorOr;
using MediatR;

namespace Luna.Application.Record.Commands.Move;

public record RecordMoveRequest(
    ulong ExecutorDiscordId,
    IReadOnlyDictionary<ulong, bool> IdAndIsDeafenPairs,
    ulong OldChannelId,
    ulong NewChannelId
) : IRequest<ErrorOr<bool>>;