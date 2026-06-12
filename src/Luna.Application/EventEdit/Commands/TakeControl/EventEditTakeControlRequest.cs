using ErrorOr;
using MediatR;

namespace Luna.Application.EventEdit.Commands.TakeControl;

public record EventEditTakeControlRequest(
    ulong ExecutorDiscordId,
    int EventId
) : IRequest<ErrorOr<bool>>;