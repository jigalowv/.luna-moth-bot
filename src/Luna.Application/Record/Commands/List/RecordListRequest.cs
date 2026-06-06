using ErrorOr;
using MediatR;

namespace Luna.Application.Record.Commands.List;

public record RecordListRequest(
    ulong ExecutorDiscordId
) : IRequest<ErrorOr<RecordListResponse>>;