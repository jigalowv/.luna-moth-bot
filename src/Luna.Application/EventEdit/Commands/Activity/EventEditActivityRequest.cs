using ErrorOr;
using MediatR;

namespace Luna.Application.EventEdit.Commands.Activity;

public record EventEditActivityRequest
(
    ulong ExecutorDiscordId,
    ulong TargetDiscordId,
    bool IsActive,
    int? EventId
) : IRequest<ErrorOr<bool>>;