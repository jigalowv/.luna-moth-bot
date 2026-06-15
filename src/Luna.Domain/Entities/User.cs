using Luna.Domain.Enums;

namespace Luna.Domain.Entities;

public class User
{
    public int Id { get; init; }
    public ulong DiscordId { get; init; }
    public DateTime? CreatedAt { get; init; }
}