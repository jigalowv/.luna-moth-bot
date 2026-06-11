using ErrorOr;
using MediatR;

namespace Luna.Application.EventEdit.Commands.Show;

public record EventEditShowRequest(
    ulong ExecutorDiscordId,
    int? EventId
) : IRequest<ErrorOr<EventEditShowResponse>>;