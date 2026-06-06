using ErrorOr;
using MediatR;

namespace Luna.Application.Record.Commands.Cancel;

public record RecordCancelRequest(
    ulong ExecutorDiscordId,
    ulong ChannelId
) : IRequest<ErrorOr<RecordCancelResponse>>;