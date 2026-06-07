using Luna.Application.Common.Interfaces;
using Luna.Domain.Entities;
using Luna.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Luna.Infrastructure.Repositories;


public sealed class EventTypeRepository : IEventTypeRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<EventTypeRepository> _logger;

    public EventTypeRepository(
        AppDbContext context,
        ILogger<EventTypeRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> AddAsync(
        EventType eventType, 
        CancellationToken ct)
    {
        try
        {
            _context.EventTypes.Add(eventType);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error occurred while adding event type: " +
                "{title}.", eventType.Title);
            
            return false;
        }
    }

    public async Task<EventType?> GetByTitleAsync(
        string title, CancellationToken ct)
    {
        return await _context.EventTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Title == title, ct);
    }

    public async Task<ICollection<EventType>> GetAllAsync(
        CancellationToken ct)
    {
        return await _context.EventTypes
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<bool> RemoveAsync(
        string title, 
        CancellationToken ct)
    {
        var eventType = await _context.EventTypes
            .FirstOrDefaultAsync(i => i.Title == title);

        try
        {
            if (eventType is null)
            {
                _logger.LogError( 
                    "Event type {title} not found.", title);
                
                return false;
            }

            _context.Remove(eventType);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error occurred while removing event type: " +
                "{title}.", title);
            
            return false;
        }
    }

    public async Task<bool> SetTitleAsync(
        string oldTitle, 
        string newTitle, 
        CancellationToken ct)
    {
        var eventType = await _context.EventTypes
            .FirstOrDefaultAsync(i => i.Title == oldTitle);

        try
        {
            if (eventType is null)
            {
                _logger.LogError( 
                    "Event type {title} not found.", oldTitle);
                
                return false;
            }

            eventType.Title = newTitle;

            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error occurred while setting event type title: " +
                "{oldTitle} -> {newTitle}.", oldTitle, newTitle);
            
            return false;
        }
    }

    public async Task<ICollection<EventType>> SearchAsync(
        string searchTerm, 
        CancellationToken ct)
    {
        var query = _context.EventTypes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(i => i.Title.Contains(searchTerm));
        }

        return await query
            .Select(i => i) 
            .ToListAsync(ct);
    }
}