using Luna.Domain.Entities;

namespace Luna.Application.Common.Interfaces;

public interface IEventTypeRepository
{
    Task<EventType?> GetByTitleAsync(string title, CancellationToken ct);
    Task<ICollection<EventType>> SearchAsync(string searchTerm, CancellationToken ct);
    Task<ICollection<EventType>> GetAllAsync(CancellationToken ct);
    Task<bool> AddAsync(EventType eventType, CancellationToken ct);
    Task<bool> RemoveAsync(string title, CancellationToken ct);
    Task<bool> SetTitleAsync(string oldTitle, string newTitle, CancellationToken ct);
}