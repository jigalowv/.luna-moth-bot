using ErrorOr;
using MediatR;

namespace Luna.Application.EventTypes.Commands.Remove;

public record EventTypeRemoveRequest(
    ulong ExecutorDiscordId,
    string Title
) : IRequest<ErrorOr<bool>>;