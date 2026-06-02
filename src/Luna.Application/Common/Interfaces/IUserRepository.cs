using Luna.Domain.Entities;
using Luna.Domain.Enums;

namespace Luna.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByDiscordIdAsync(ulong discordId, CancellationToken ct);
    Task<bool> AddUserAsync(User newUser, CancellationToken ct);
    Task<bool> SetUserRoleAsync(ulong discordId, UserRole role, CancellationToken ct);
}