using ErrorOr;
using MediatR;
using Luna.Domain.Enums;

namespace Luna.Application.Users.Commands.AddUser;

public record AddUserRequest(
    ulong ExecutorDiscordId,
    ulong DiscordId,
    UserRole Role
) : IRequest<ErrorOr<bool>>;