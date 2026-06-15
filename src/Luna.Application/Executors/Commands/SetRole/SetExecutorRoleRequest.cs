using ErrorOr;
using MediatR;
using Luna.Domain.Enums;

namespace Luna.Application.Executors.Commands.SetRole;

public record ExecutorSetRoleRequest(
    ulong ExecutorDiscordId,
    ulong DiscordId,
    ExecutorRole NewRole
) : IRequest<ErrorOr<bool>>;