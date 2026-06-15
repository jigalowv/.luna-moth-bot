using Luna.Domain.Entities;

namespace Luna.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByDiscordIdAsync(ulong discordId, CancellationToken ct);
    Task<ICollection<User>> GetAllByDiscordIdsAsync(ICollection<ulong> discordIds, CancellationToken ct);
    Task<bool> AddAsync(User newUser, CancellationToken ct);
}