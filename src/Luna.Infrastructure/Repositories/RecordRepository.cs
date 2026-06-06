using Luna.Application.Common.Interfaces;
using Luna.Domain.Entities;
using Luna.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Luna.Infrastructure.Repositories;

public sealed class RecordRepository : IRecordRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<RecordRepository> _logger;

    public RecordRepository(
        ILogger<RecordRepository> logger,
        AppDbContext context
    )
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> CancelAsync(
        ulong channelId,
        CancellationToken ct)
    {
        var record = await _context.Records
            .Include(i => i.Attendances)
            .FirstOrDefaultAsync(i => 
                i.ChannelId == channelId);

        if (record is null)
        {
            _logger.LogError("The channel not found");
            return false;
        }

        using var transaction = await _context.Database
            .BeginTransactionAsync(ct);

        try
        {
            _context.Records.Remove(record);
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            
            _logger.LogError(ex, 
                "Error occurred while start tracking");

            return false;
        }
    }

    public async Task<Record?> GetAsync(
        ulong channelId, CancellationToken ct) =>
        await _context.Records.FirstOrDefaultAsync(i => 
            i.ChannelId == channelId, ct);

    public async Task<Record?> GetByCreatorIdAsync(
        int executorId, CancellationToken ct) =>
        await _context.Records.FirstOrDefaultAsync(
            tc => tc.ExecutorId == executorId, ct
        );

    public async Task<bool> AddAsync(
        Record record, 
        ICollection<RecordAttendance> recordAttendances,
        CancellationToken ct)
    {
        using var transaction = await _context.Database
            .BeginTransactionAsync(ct);

        try
        {
            await _context.Records.AddAsync(record, ct);

            await _context.SaveChangesAsync();

            foreach (var attendance in recordAttendances)
            {
                attendance.RecordId = record.Id;
            }

            await _context.RecordAttendances
                .AddRangeAsync(recordAttendances, ct);

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            
            _logger.LogError(ex, 
                "Error occurred while adding");

            return false;
        }
    }

    public async Task<IReadOnlyCollection<Record>> GetAllAsync(
        CancellationToken ct) => 
        await _context.Records
            .AsNoTracking()
            .Include(i => i.Executor)
            .ToListAsync(ct);

    public async Task<bool> SetNewChannel(
        ICollection<RecordAttendance> newRecordAttendances,
        ulong oldChannelId,
        ulong newChannelId,
        CancellationToken ct)
    {
        var record = await _context.Records
            .Include(i => i.Attendances)
            .FirstOrDefaultAsync(i => 
                i.ChannelId == oldChannelId, ct);

        if (record is null)
            return false;

        try
        {
            var oldRecordActiveAttendances = record.Attendances
                .Where(i => i.EndAt == null);

            foreach (var attendance in oldRecordActiveAttendances)
            {
                attendance.EndAt = DateTime.UtcNow;
            }

            foreach (var attendance in newRecordAttendances)
            {
                attendance.RecordId = record.Id;
            }

            _context.RecordAttendances.AddRange(newRecordAttendances);
            record.ChannelId = newChannelId;

            await _context.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error occurred set new channel");
            
            return false;
        }

        return true;
    }
}