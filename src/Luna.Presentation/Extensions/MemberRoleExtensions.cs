using Luna.Domain.Enums;

namespace Luna.Presentation.Extensions;

public static class MemberRoleExtensions
{
    public static string GetName(this MemberRole role) => role switch
    {
        MemberRole.Spectator => "зритель",
        MemberRole.Player => "игрок",
        MemberRole.Host => "ведущий",
        _ => "мимокрокодил",
    };
}