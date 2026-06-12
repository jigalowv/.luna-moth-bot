using ErrorOr;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.EventEdit.Commands.Roles;

public record EventEditRolesRequest
(
    ulong ExecutorDiscordId,
    List<ulong> TargetsDiscordIds,
    MemberRole Role,
    int? EventId
) : IRequest<ErrorOr<bool>>;

