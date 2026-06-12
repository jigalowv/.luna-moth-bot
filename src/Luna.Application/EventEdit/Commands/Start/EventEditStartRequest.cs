using ErrorOr;
using MediatR;

namespace Luna.Application.EventEdit.Commands.Start;

public record EventEditStartRequest(
    ulong ExecutorDiscordId,
    int EventId
) : IRequest<ErrorOr<bool>>;