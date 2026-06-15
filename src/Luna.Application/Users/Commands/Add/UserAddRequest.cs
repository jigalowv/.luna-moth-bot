using ErrorOr;
using MediatR;

namespace Luna.Application.Users.Commands.Add;

public record UserAddRequest(
    ulong ExecutorDiscordId,
    ulong DiscordId
) : IRequest<ErrorOr<bool>>;