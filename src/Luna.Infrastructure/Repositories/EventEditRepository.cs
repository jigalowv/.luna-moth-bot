using Luna.Application.Common.Interfaces;
using Luna.Domain.Entities;
using Luna.Domain.Enums;
using Luna.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Luna.Infrastructure.Repositories;

public sealed class EventEditRepository : IEventEditRepository
{
    private readonly ILogger<EventEditRepository> _logger;
    private readonly AppDbContext _context;

    public EventEditRepository(
        ILogger<EventEditRepository> logger,
        AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<ICollection<EventEdit>> GetAllWithDetailsAsync(
        CancellationToken ct)
    {
        return await _context.EventEdits
            .AsNoTracking()
            .AsSplitQuery()
            .Include(i => i.Executors)
                .ThenInclude(i => i.Executor)
            .Include(i => i.Event)
                .ThenInclude(i => i.Creator)
            .Include(i => i.Event)
                .ThenInclude(i => i.Members)
                    .ThenInclude(i => i.User)
            .Include(i => i.Event)
                .ThenInclude(i => i.Type)
            .Include(i => i.TempEvent)
                .ThenInclude(i => i.Creator)
            .Include(i => i.TempEvent)
                .ThenInclude(i => i.Members)
                    .ThenInclude(i => i.User)
            .Include(i => i.TempEvent)
                .ThenInclude(i => i.Type)
            .ToListAsync(ct);
    }

    public async Task<EventEdit?> GetWithDetailsAsync(int id, CancellationToken ct)
    {
        return await _context.EventEdits
            .AsNoTracking()
            .AsSplitQuery()
            .Include(i => i.Executors)
                .ThenInclude(e => e.Executor)
            .Include(i => i.Event)
                .ThenInclude(c => c.Creator)
            .Include(i => i.Event)
                .ThenInclude(m => m.Members)
                    .ThenInclude(m => m.User)
            .Include(i => i.Event)
                .ThenInclude(t => t.Type)
            .Include(i => i.TempEvent)
                .ThenInclude(tc => tc.Creator)
            .Include(i => i.TempEvent)
                .ThenInclude(tm => tm.Members)
                    .ThenInclude(m => m.User)
            .Include(i => i.TempEvent)
                .ThenInclude(tt => tt.Type)
            .FirstOrDefaultAsync(ee => ee.EventId == id);
    }

    public async Task<EventEdit?> GetLastByExecutorIdWithDetailsAsync(
        int id, 
        CancellationToken ct)
    {
        return await _context.EventEdits
            .AsNoTracking()
            .AsSplitQuery()
            .Where(ee => ee.Executors.Any(e => e.ExecutorId == id))
            .Include(i => i.Executors)
                .ThenInclude(e => e.Executor)
            .Include(i => i.Event)
                .ThenInclude(c => c.Creator)
            .Include(i => i.Event)
                .ThenInclude(m => m.Members)
                    .ThenInclude(m => m.User)
            .Include(i => i.Event)
                .ThenInclude(t => t.Type)
            .Include(i => i.TempEvent)
                .ThenInclude(tc => tc.Creator)
            .Include(i => i.TempEvent)
                .ThenInclude(tm => tm.Members)
                    .ThenInclude(m => m.User)
            .Include(i => i.TempEvent)
                .ThenInclude(tt => tt.Type)
            .OrderByDescending(ee => ee.Executors
                .Where(e => e.ExecutorId == id)
                .Max(e => e.CreatedAt))
            .FirstOrDefaultAsync(ct); 
    }

    public async Task<EventEdit?> GetLastByExecutorIdAsync(int id, CancellationToken ct)
    {
        return await _context.EventEdits
            .AsNoTracking()
            .Where(ee => ee.Executors.Any(e => e.ExecutorId == id))
            .OrderByDescending(ee => ee.Executors
                .Where(e => e.ExecutorId == id)
                .Max(e => e.CreatedAt))
            .FirstOrDefaultAsync(ct); 
    }

    public async Task<EventEdit?> GetAsync(int id, CancellationToken ct)
    {
        return await _context.EventEdits
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.EventId == id, ct);
    }

    public Task<EventMember?> GetMemberByDiscordIdAsync(
        int eventId, ulong targetDiscordId, CancellationToken ct)
    {
        return _context.EventMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(i => 
                i.EventId == eventId &&
                i.User.DiscordId == targetDiscordId, 
                ct);
    }

    public async Task<bool> SetRoleAsync(
        int memberId, MemberRole role, CancellationToken ct)
    {
        var member = await _context.EventMembers
            .FindAsync(memberId, ct);

        if (member is null)
            return false;

        try
        {
            member.Role = role;

            await _context.SaveChangesAsync(ct);
            
            return true;   
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "An exception occured while set member role.");
            
            return false;
        }
    }

    public async Task<ICollection<EventEdit>> GetByExecutorIdWithDetailsAsync(int id, CancellationToken ct)
    {
        return await _context.EventEdits
            .AsNoTracking()
            .AsSplitQuery()
            .Where(ee => ee.Executors.Any(e => e.ExecutorId == id)) // Filter early
            .Include(i => i.Executors)
                .ThenInclude(e => e.Executor)
            .Include(i => i.Event)
                .ThenInclude(c => c.Creator)
            .Include(i => i.Event)
                .ThenInclude(m => m.Members)
                    .ThenInclude(m => m.User)
            .Include(i => i.Event)
                .ThenInclude(t => t.Type)
            .Include(i => i.TempEvent)
                .ThenInclude(tc => tc.Creator)
            .Include(i => i.TempEvent)
                .ThenInclude(tm => tm.Members)
                    .ThenInclude(m => m.User)
            .Include(i => i.TempEvent)
                .ThenInclude(tt => tt.Type)
            .ToListAsync(ct);
    }

    public async Task<bool> SetActivityAsync(int memberId, bool isActive, CancellationToken ct)
    {
        var member = await _context.EventMembers
            .FindAsync(memberId, ct);

        if (member is null)
            return false;

        try
        {
            member.IsActive = isActive;

            await _context.SaveChangesAsync(ct);
            
            return true;   
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "An exception occured while set member activity status.");
            
            return false;
        }
    }

    public async Task<bool> SetType(int id, int typeId, CancellationToken ct)
    {
        var eventEdit = await _context.EventEdits
            .Include(ee => ee.TempEvent)
            .FirstOrDefaultAsync(ee => ee.EventId == id, ct);

        if (eventEdit is null)
            return false;

        try
        {
            eventEdit.TempEvent.TypeId = typeId;

            await _context.SaveChangesAsync(ct);
            
            return true;   
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "An exception occured while set event type status.");
            
            return false;
        }
    }
}