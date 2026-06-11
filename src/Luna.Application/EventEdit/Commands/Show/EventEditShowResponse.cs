using Luna.Application.Common.Dtos;
using Luna.Domain.Enums;

namespace Luna.Application.EventEdit.Commands.Show;

public record EventEditShowResponse(
    Change<string> EventTypeTitle,
    ICollection<EventEditShowResponseMember> Members,

    ulong EventCreatorDiscordId,
    DateTime StartAt,
    DateTime EndAt
);

public record EventEditShowResponseMember(
    ulong DiscordId,
    Change<bool> IsActive,
    Change<MemberRole> Role
);