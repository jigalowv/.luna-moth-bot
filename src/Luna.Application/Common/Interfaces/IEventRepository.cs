using Luna.Domain.Entities;

namespace Luna.Application.Common.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetAsync(int id, CancellationToken ct);
}