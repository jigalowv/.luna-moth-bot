using Luna.Domain.Entities;
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
    Task<Domain.Entities.EventMemberEdit?> GetMemberByDiscordIdAsync(int eventId, ulong targetDiscordId, CancellationToken ct);
    Task<bool> SetRoleAsync(int memberId, MemberRole role, CancellationToken ct);
    Task<bool> SetRolesAsync(int eventId, ICollection<int> userIds, MemberRole role, CancellationToken ct);
    Task<bool> SetActivityAsync(int memberId, bool isActive, CancellationToken ct);
    Task<bool> SetActivitiesAsync(int eventId, ICollection<int> userIds, bool isActive, CancellationToken ct);
    Task<bool> SetType(int eventId, int eventTypeId, CancellationToken ct);
    Task<bool> CancelAsync(int eventId, CancellationToken ct);
    Task<bool> StartAsync(int eventId, int executorId, CancellationToken ct);
    Task<bool> EndAsync(int eventId, CancellationToken ct);
    Task<bool> DeleteAsync(int eventId, CancellationToken ct);
    Task<EventEditExecutor?> GetExecutorAsync(int eventId, int userId, CancellationToken ct);
    Task<bool> AddExecutor(int eventId, int userId, CancellationToken ct);
}