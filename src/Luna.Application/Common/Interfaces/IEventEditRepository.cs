using Luna.Domain.Enums;

namespace Luna.Application.Common.Interfaces;

public interface IEventEditRepository
{
    Task<ICollection<Domain.Entities.EventEdit>> GetAllWithDetailsAsync(CancellationToken ct);
    Task<ICollection<Domain.Entities.EventEdit>> GetByExecutorIdWithDetailsAsync(int id, CancellationToken ct);
    Task<Domain.Entities.EventEdit?> GetLastByExecutorIdWithDetailsAsync(int id, CancellationToken ct);
    Task<Domain.Entities.EventEdit?> GetWithDetailsAsync(int id, CancellationToken ct);
    Task<Domain.Entities.EventEdit?> GetLastByExecutorIdAsync(int id, CancellationToken ct);
    Task<Domain.Entities.EventEdit?> GetAsync(int id, CancellationToken ct);
    Task<Domain.Entities.EventMember?> GetMemberByDiscordIdAsync(int eventId, ulong targetDiscordId, CancellationToken ct);
    Task<bool> SetRoleAsync(int memberId, MemberRole role, CancellationToken ct);
    Task<bool> SetActivityAsync(int memberId, bool isActive, CancellationToken ct);
    Task<bool> SetType(int id, int eventTypeId, CancellationToken ct);
}