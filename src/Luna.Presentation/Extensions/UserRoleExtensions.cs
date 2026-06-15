using Luna.Domain.Enums;

namespace Luna.Presentation.Extensions;

public static class UserRoleExtensions
{
    public static string GetName(this ExecutorRole role) => role switch
    {
        ExecutorRole.Curator => "куратор",
        ExecutorRole.Moderator => "модератор",
        ExecutorRole.head => "глава",
        _ => "без роли",
    };
}