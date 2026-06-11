using Discord.Interactions;
using Luna.Domain.Enums;

namespace Luna.Presentation.Enums;

public enum AllowedEndRoles
{
    [ChoiceDisplay("Игрок")]
    Player = MemberRole.Player,

    [ChoiceDisplay("Зритель")]
    Spectator = MemberRole.Spectator
}