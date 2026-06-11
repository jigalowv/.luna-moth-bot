using ErrorOr;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.EventEdit.Commands.Role;

public record EventEditRoleRequest
(
    ulong ExecutorDiscordId,
    ulong TargetDiscordId,
    MemberRole Role,
    int? EventId
) : IRequest<ErrorOr<bool>>;