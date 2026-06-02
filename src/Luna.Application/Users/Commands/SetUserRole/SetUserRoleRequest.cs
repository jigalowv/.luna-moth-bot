using ErrorOr;
using MediatR;
using Luna.Domain.Enums;

namespace Luna.Application.Users.Commands.SetUserRole;

public record SetUserRoleRequest(
    ulong ExecutorDiscordId,
    ulong DiscordId,
    UserRole NewRole
) : IRequest<ErrorOr<bool>>;