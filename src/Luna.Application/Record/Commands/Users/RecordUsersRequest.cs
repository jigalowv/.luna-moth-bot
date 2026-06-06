using ErrorOr;
using Luna.Application.Record.Commands.Users;
using MediatR;

namespace Luna.Application.Record.Commands.Users;

public record RecordUsersRequest(
    ulong? ChannelId,
    ulong ExecutorDiscordId
) : IRequest<ErrorOr<RecordUsersResponse>>;