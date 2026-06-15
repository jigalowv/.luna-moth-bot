using Luna.Domain.Entities;
using Luna.Domain.Enums;

namespace Luna.Application.Common.Interfaces;

public interface IExecutorRepository
{
    Task<Executor?> GetByDiscordIdAsync(ulong discordId, CancellationToken ct);
    Task<bool> AddAsync(ulong discordId, Executor newExecutor, CancellationToken ct);
    Task<bool> SetRoleAsync(ulong discordId, ExecutorRole role, CancellationToken ct);
    Task<ICollection<Executor>> GetAllAsync(CancellationToken ct);
}