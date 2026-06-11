using ErrorOr;
using MediatR;

namespace Luna.Application.EventEdit.Commands.Type;

public record EventEditTypeRequest(
    ulong ExecutorDiscordId,
    string EventTypeTitle,
    int? EventId
) : IRequest<ErrorOr<bool>>;