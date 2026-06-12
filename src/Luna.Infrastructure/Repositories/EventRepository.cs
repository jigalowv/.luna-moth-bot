using Luna.Application.Common.Interfaces;
using Luna.Domain.Entities;
using Luna.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Luna.Infrastructure.Repositories;

public sealed class EventRepository : IEventRepository
{
    private readonly AppDbContext _context;

    public EventRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Event?> GetAsync(int id, CancellationToken ct)
    {
        return await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }
}