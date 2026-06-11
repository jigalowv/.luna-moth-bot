using Discord.Interactions;
using Luna.Domain.Enums;

namespace Luna.Presentation.Enums;

public enum SetUserRole
{
    [ChoiceDisplay("без роли")]
    None = UserRole.None,

    [ChoiceDisplay("куратор")]
    Curator = UserRole.Curator,
    
    [ChoiceDisplay("модератор")]
    Moderator = UserRole.Moderator,
}