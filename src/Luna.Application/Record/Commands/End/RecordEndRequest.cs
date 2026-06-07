using ErrorOr;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.Record.Commands.End;

public record RecordEndRequest(
    ulong ExecutorDiscordId,
    ulong ChannelId,
    string EventTypeTitle,
    MemberRole? Role,
    int? MinDuration
) : IRequest<ErrorOr<RecordEndResponse>>;