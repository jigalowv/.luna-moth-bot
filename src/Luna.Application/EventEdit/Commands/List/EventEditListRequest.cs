using ErrorOr;
using MediatR;

namespace Luna.Application.EventEdit.Commands.List;

public record EventEditListRequest(
    ulong ExecutorDiscordId
) : IRequest<ErrorOr<EventEditListResponse>>;