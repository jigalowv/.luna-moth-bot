using Discord.Interactions;
using Luna.Domain.Enums;

namespace Luna.Presentation.Enums;

public enum DiscordMemberRole
{
    [ChoiceDisplay("мимокрокодил")]
    None = MemberRole.None,

    [ChoiceDisplay("спектатор")]
    Spectator = MemberRole.Spectator,

    [ChoiceDisplay("игрок")]
    Player = MemberRole.Player,

    [ChoiceDisplay("ведущий")]
    Host = MemberRole.Host
}