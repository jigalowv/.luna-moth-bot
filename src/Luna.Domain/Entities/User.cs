using Luna.Domain.Enums;

namespace Luna.Domain.Entities;

public class User
{
    public int Id { get; init; }
    public ulong DiscordId { get; init; }
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; init; }

    private User() {}

    public static User Create(ulong discordId, UserRole role)
    {
        if (discordId == 0)
        {
            throw new ArgumentException("Discord ID cannot be zero.", nameof(discordId));
        }

        return new User
        {
            Id = 0,
            DiscordId = discordId,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void SetRole(UserRole newRole)
    {
        Role = newRole;
    }

    public bool CanAssignRole(UserRole targetRole)
    {
        return this.Role >= UserRole.Moderator && this.Role > targetRole;
    }

    public bool CanReassignRole(UserRole beforeRole, UserRole afterRole)
    {
        return CanAssignRole(afterRole) && Role > beforeRole;
    }
}