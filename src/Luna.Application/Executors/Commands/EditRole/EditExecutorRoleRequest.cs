using ErrorOr;
using MediatR;
using Luna.Domain.Enums;

namespace Luna.Application.Executors.Commands.EditRole;

public record ExecutorEditRoleRequest(
    ulong ExecutorDiscordId,
    ulong DiscordId,
    ExecutorRole NewRole
) : IRequest<ErrorOr<bool>>;