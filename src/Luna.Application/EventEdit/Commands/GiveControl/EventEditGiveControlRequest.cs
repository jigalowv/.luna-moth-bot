using ErrorOr;
using MediatR;

namespace Luna.Application.EventEdit.Commands.GiveControl;

public record EventEditGiveControlRequest(
    ulong ExecutorDiscordId,
    ulong TargetDiscordId,
    int? EventId
) : IRequest<ErrorOr<bool>>;