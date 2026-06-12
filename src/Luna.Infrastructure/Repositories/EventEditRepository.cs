using System.Security.Cryptography;
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
            .Include(i => i.MembersEdits)
            .Include(i => i.NewEventType)
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
            .Include(i => i.MembersEdits)
            .Include(i => i.NewEventType)
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
            .Include(i => i.MembersEdits)
            .Include(i => i.NewEventType)
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

    public Task<EventMemberEdit?> GetMemberByDiscordIdAsync(
        int eventEditId, ulong targetDiscordId, CancellationToken ct)
    {
        return _context.EventMemberEdits
            .AsNoTracking()
            .Include(i => i.Member)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(i => 
                i.EventEditId == eventEditId &&
                i.Member.User.DiscordId == targetDiscordId, 
                ct);
    }

    public async Task<bool> SetRoleAsync(
        int memberId, MemberRole role, CancellationToken ct)
    {
        var memberEdit = await _context.EventMemberEdits
            .FindAsync(memberId, ct);

        if (memberEdit is null)
            return false;

        try
        {
            memberEdit.NewRole = role;

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
            .Include(i => i.NewEventType)
            .Include(i => i.MembersEdits)
            .ToListAsync(ct);
    }

    public async Task<bool> SetActivityAsync(int memberId, bool isActive, CancellationToken ct)
    {
        var memberEdit = await _context.EventMemberEdits
            .FindAsync(memberId, ct);

        if (memberEdit is null)
            return false;

        try
        {
            memberEdit.NewActivityStatus = isActive;

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
            .FirstOrDefaultAsync(ee => ee.EventId == id, ct);

        if (eventEdit is null)
            return false;

        try
        {
            eventEdit.NewTypeId = typeId;

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

    public async Task<bool> CancelAsync(int id, CancellationToken ct)
    {
        var eventEdit = await _context.EventEdits
            .FirstOrDefaultAsync(i => i.EventId == id, ct);

        if (eventEdit is null)
            return false;

        try
        {
            _context.EventEdits.Remove(eventEdit);

            await _context.SaveChangesAsync(ct);
            
            return true;   
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "An exception occurred while cancelling " + 
                "event edit for ID {EventId}.", id);
            return false;
        }
    }

    public async Task<bool> StartAsync(
        int eventId, int executorId, CancellationToken ct)
    {
        var ev = await _context.Events
            .Include(e => e.Members)
            .FirstOrDefaultAsync(e => e.Id == eventId, ct);
        
        if (ev is null)
            return false;

        var executorExists = await _context.Users
            .AnyAsync(u => u.Id == executorId, ct);

        if (!executorExists)
            return false;

        try
        {
            var eventEdit = new EventEdit
            {
                EventId = ev.Id,
                EndCode = Convert.ToHexString(
                    RandomNumberGenerator.GetBytes(3)),
                Executors = [],
                MembersEdits = []
            };

            eventEdit.Executors.Add(new EventEditExecutor
            {
                ExecutorId = executorId
            });

            foreach (var member in ev.Members)
            {
                eventEdit.MembersEdits.Add(new EventMemberEdit
                {
                    MemberId = member.Id,
                });
            }
                
            _context.EventEdits.Add(eventEdit);
            await _context.SaveChangesAsync(ct);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "An exception occurred while starting event " + 
                "edit for event for ID {EventId}.", eventId);
            return false;
        }
    }

    public async Task<bool> EndAsync(int eventId, CancellationToken ct)
    {
        var eventEdit = await _context.EventEdits
            .Include(i => i.Event)
            .Include(i => i.MembersEdits)
                .ThenInclude(i => i.Member)
            .FirstOrDefaultAsync(i => i.EventId == eventId);

        if (eventEdit is null)
            return false;

        try
        {
            if (eventEdit.NewTypeId is not null)
                eventEdit.Event.TypeId = eventEdit.NewTypeId.Value;

            foreach (var edit in eventEdit.MembersEdits)
            {
                if (edit.NewActivityStatus is not null)
                    edit.Member.IsActive = edit.NewActivityStatus.Value;

                if (edit.NewRole is not null)
                    edit.Member.Role = edit.NewRole.Value;
            }

            _context.EventEdits.Remove(eventEdit);

            await _context.SaveChangesAsync(ct);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "An exception occurred while ending event " + 
                "edit for event for ID {EventId}.", eventId);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int eventId, CancellationToken ct)
    {
        var eventEdit = await _context.EventEdits
            .Include(i => i.Event)
            .FirstOrDefaultAsync(i => i.EventId == eventId, ct);
        
        if (eventEdit is null)
            return false;

        try
        {
            _context.EventEdits.Remove(eventEdit);
            _context.Events.Remove(eventEdit.Event);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "An exception occurred while deleting event " + 
                "edit for event for ID {EventId}.", eventId);
            return false;
        }
    }

    public async Task<bool> SetRolesAsync(
        int eventId, 
        ICollection<int> userIds, 
        MemberRole role, 
        CancellationToken ct)
    {
        var members = await _context.EventMemberEdits
            .Include(i => i.Member)
            .Where(i =>
                i.EventEditId == eventId && 
                userIds.Contains(i.Member.UserId))
            .ToListAsync(ct);

        try
        {
            foreach (var member in members)
                member.NewRole = role;
            
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "An exception occurred while updating event " + 
                "edit for event for ID {EventId}.", eventId);
            return false;
        }
    }

    public async Task<bool> SetActivitiesAsync(
        int eventId, 
        ICollection<int> userIds, 
        bool isActive, 
        CancellationToken ct)
    {
        var members = await _context.EventMemberEdits
            .Include(i => i.Member)
            .Where(i =>
                i.EventEditId == eventId && 
                userIds.Contains(i.Member.UserId))
            .ToListAsync(ct);

        try
        {
            foreach (var member in members)
                member.NewActivityStatus = isActive;
            
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "An exception occurred while updating event " + 
                "edit for event for ID {EventId}.", eventId);
            return false;
        }
    }

    public async Task<EventEditExecutor?> GetExecutorAsync(
        int eventId, int userId, CancellationToken ct)
    {
        return await _context.EventEditExecutors
            .FirstOrDefaultAsync(i => 
                i.EventEditId == eventId &&
                i.ExecutorId == userId, ct);
    }

    public async Task<bool> AddExecutor(
        int eventId, int userId, CancellationToken ct)
    {
        var eventEdit = await _context.EventEdits.FindAsync(eventId, ct);
        
        if (eventEdit is null)
            return false;

        var user = await _context.Users.FindAsync(userId, ct);

        if (user is null)
            return false;

        try
        {
            var executor = new EventEditExecutor
            {
                EventEditId = eventEdit.EventId,
                ExecutorId = user.Id
            };
            
            _context.EventEditExecutors.Add(executor);
            await _context.SaveChangesAsync(ct);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "An exception occurred while updating event " + 
                "edit for event for ID {EventId}.", eventId);
            return false;
        }
    }
}