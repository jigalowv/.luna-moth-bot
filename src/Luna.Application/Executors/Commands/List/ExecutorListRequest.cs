using ErrorOr;
using MediatR;

namespace Luna.Application.Executors.Commands.List;

public record ExecutorListRequest(
    ulong ExecutorDiscordId
) : IRequest<ErrorOr<ExecutorListResponse>>;