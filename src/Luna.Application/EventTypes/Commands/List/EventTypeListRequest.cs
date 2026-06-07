using ErrorOr;
using MediatR;

namespace Luna.Application.EventTypes.Commands.List;

public record EventTypeListRequest(
    ulong ExecutorDiscordId
) : IRequest<ErrorOr<EventTypeListResponse>>;