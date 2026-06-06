using Luna.Application.Common.Interfaces;
using Luna.Domain.Entities;
using Luna.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Luna.Infrastructure.Repositories;

public sealed class RecordAttendanceRepository : IRecordAttendanceRepository
{
    private readonly ILogger<RecordAttendanceRepository> _logger;
    private readonly AppDbContext _context;

    public RecordAttendanceRepository(
        ILogger<RecordAttendanceRepository> logger,
        AppDbContext context
    )
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> AddAsync(
        RecordAttendance attendance, 
        CancellationToken ct)
    {
        try
        {
            await _context.RecordAttendances
                .AddAsync(attendance, ct);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error occurred while adding an attendance for User: " +
                "{UserId}", attendance.DiscordUserId);
            return false;
        }
    }

    public async Task<bool> EndAndAddAsync(
        RecordAttendance attendance, 
        CancellationToken ct)
    {
        using var transaction = await _context.Database
            .BeginTransactionAsync(ct);
        
        try
        {
            var activeAttendance = await GetActiveAttendance(
                recordId: attendance.RecordId, 
                discordUserId: attendance.DiscordUserId, 
                ct: ct);
            
            if (activeAttendance is null)
            {
                _logger.LogWarning(
                    "Active attendance not found for User: {UserId}.", 
                    attendance.DiscordUserId);
                
                return false;
            }

            activeAttendance.EndAt = DateTime.UtcNow;

            _context.RecordAttendances.Add(attendance);

            await _context.SaveChangesAsync(ct);
            
            await transaction.CommitAsync(ct);

            return true;            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error occurred while ending and adding attendance for User: " + 
                "{UserId}.", attendance.DiscordUserId);
            
            return false;
        }
    }

    public async Task<bool> EndAsync(
        int recordId, 
        ulong discordUserId, 
        CancellationToken ct)
    {
        try
        {
            var activeAttendance = await GetActiveAttendance(
                recordId, discordUserId, ct);
            
            if (activeAttendance is null)
            {
                _logger.LogWarning(
                    "Active attendance not found for User: " + 
                    "{UserId}", discordUserId);
                return false;
            }

            activeAttendance.EndAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            
            return true;            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error occurred while ending attendance for User: " +
                "{UserId}.", discordUserId);
            
            return false;
        }
    }

    public async Task<IReadOnlyDictionary<ulong, TimeSpan>> GetUserDurations(
        int recordId,
        bool isDeafen,
        CancellationToken ct)
    {
        Dictionary<ulong, TimeSpan> result = []; 
        
        var attendances = await _context.RecordAttendances
            .AsNoTracking()
            .Where(ta => 
                ta.RecordId == recordId &&
                ta.IsDeafened == isDeafen)
            .ToListAsync(ct);

        var now = DateTime.UtcNow;

        foreach (var attendance in attendances)
        {       
            if (attendance.StartAt is null) continue;

            var endAt = attendance.EndAt ?? now;
            TimeSpan timeSpan = endAt - attendance.StartAt.Value;
            
            if (result.TryGetValue(attendance.DiscordUserId, 
                out var currentDuration))
                result[attendance.DiscordUserId] = currentDuration + timeSpan;
            else 
                result[attendance.DiscordUserId] = timeSpan;
        }

        return result;
    }

    private async Task<RecordAttendance?> GetActiveAttendance(
        int recordId, 
        ulong discordUserId, 
        CancellationToken ct
    ) => await _context.RecordAttendances
        .Where(i => 
            i.RecordId == recordId &&
            i.DiscordUserId == discordUserId &&
            i.EndAt == null)
        .OrderByDescending(i => i.StartAt)
        .FirstOrDefaultAsync(ct);
}