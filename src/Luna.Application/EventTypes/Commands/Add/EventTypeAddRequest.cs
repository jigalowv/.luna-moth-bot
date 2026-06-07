using ErrorOr;
using MediatR;

namespace Luna.Application.EventTypes.Commands.Add;

public record EventTypeAddRequest(
    ulong ExecutorDiscordId,
    string Title
) : IRequest<ErrorOr<bool>>;