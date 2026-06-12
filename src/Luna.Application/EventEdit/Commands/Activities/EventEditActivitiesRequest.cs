using ErrorOr;
using MediatR;

namespace Luna.Application.EventEdit.Commands.Activities;

public record EventEditActivitiesRequest
(
    ulong ExecutorDiscordId,
    List<ulong> TargetsDiscordIds,
    bool IsActive,
    int? EventId
) : IRequest<ErrorOr<bool>>;

