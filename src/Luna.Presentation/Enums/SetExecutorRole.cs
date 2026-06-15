using Discord.Interactions;
using Luna.Domain.Enums;

namespace Luna.Presentation.Enums;

public enum SetExecutorRole
{
    [ChoiceDisplay("куратор")]
    Curator = ExecutorRole.Curator,
    
    [ChoiceDisplay("модератор")]
    Moderator = ExecutorRole.Moderator,
}