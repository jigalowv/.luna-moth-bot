using Luna.Domain.Enums;

namespace Luna.Domain.Entities;

public class EventMemberEdit
{
    public int MemberId { get; set; }
    public int EventEditId { get; set; }
    public MemberRole? NewRole { get; set; }
    public bool? NewActivityStatus { get; set; }

    public EventMember Member { get; set; } = null!;
    public EventEdit EventEdit { get; set; } = null!;
}