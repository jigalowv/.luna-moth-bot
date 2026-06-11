using Luna.Domain.Enums;

namespace Luna.Presentation.Extensions;

public static class UserRoleExtensions
{
    public static string GetName(this UserRole role) => role switch
    {
        UserRole.Curator => "куратор",
        UserRole.Moderator => "модератор",
        UserRole.head => "глава",
        _ => "без роли",
    };
}