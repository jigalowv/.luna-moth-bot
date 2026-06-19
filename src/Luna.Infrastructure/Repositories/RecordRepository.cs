using System.Security.Cryptography;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Entities;
using Luna.Domain.Enums;
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
        
    public async Task<Record?> GetLastByExecutorIdAsync(
        int executorId, CancellationToken ct)
    {
        return await _context.Records
            .AsNoTracking()
            .Where(r => r.ExecutorId == executorId)
            .OrderByDescending(r => r.StartAt)
            .FirstOrDefaultAsync(ct);
    }

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
                .ThenInclude(i => i.User)
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
                "Error occurred while setting new channel");
            
            return false;
        }

        return true;
    }

    public async Task<bool> ToEventAsync(
        int recordId, 
        int eventTypeId, 
        int executorId, 
        int minDuration, 
        MemberRole role, 
        CancellationToken ct)
    {
        var eventTypeExists = await _context.EventTypes
            .AnyAsync(e => e.Id == eventTypeId, ct);
        if (!eventTypeExists) return false;

        var executor = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == executorId, ct);
        if (executor is null) return false;

        var record = await _context.Records
            .Include(i => i.Attendances)
            .FirstOrDefaultAsync(i => i.Id == recordId, ct);
        
        if (record?.StartAt is null) 
            return false;

        using var transaction = await _context.Database
            .BeginTransactionAsync(ct);
        
        try
        {
            var now = DateTime.UtcNow;

            foreach (var attendance in record.Attendances)
            {
                attendance.EndAt ??= now;
            }

            var uniqueDiscordIds = record.Attendances
                .Select(i => i.DiscordUserId)
                .Distinct()
                .ToList();

            var existingUsers = await _context.Users
                .Where(u => uniqueDiscordIds.Contains(u.DiscordId))
                .ToDictionaryAsync(u => u.DiscordId, ct);

            foreach (var discordId in uniqueDiscordIds)
            {
                if (!existingUsers.ContainsKey(discordId))
                {
                    var newUser = new User { DiscordId = discordId };
                    _context.Users.Add(newUser);
                    existingUsers.Add(discordId, newUser);
                }
            }

            var newEvent = new Event
            {
                StartAt = record.StartAt.Value,
                EndAt = now,
                TypeId = eventTypeId,
                CreatorId = executorId,
                Members = new List<EventMember>()
            };

            var attendancesByDiscordId = record.Attendances
                .GroupBy(i => i.DiscordUserId);

            foreach (var group in attendancesByDiscordId)
            {
                if (!existingUsers.TryGetValue(group.Key, out var user)) 
                    continue;

                int duration = (int)group
                    .Sum(i => 
                        (i.EndAt!.Value - i.StartAt!.Value).TotalMinutes);

                var memberRole = duration >= minDuration ? 
                    role : MemberRole.None;
                if (user.Id == record.ExecutorId) 
                    memberRole = MemberRole.Host;

                var eventAttendances = group
                    .Select(a => new EventAttendance
                {
                    StartAt = a.StartAt!.Value,
                    EndAt = a.EndAt!.Value,
                    IsDeafened = a.IsDeafened
                }).ToList();

                newEvent.Members.Add(new EventMember
                {
                    User = user,
                    Role = memberRole,
                    Attendances = eventAttendances
                });
            }

            _context.Events.Add(newEvent);

            await _context.SaveChangesAsync(ct);

            var eventEdit = new EventEdit
            {
                EndCode = Convert.ToHexString(
                    RandomNumberGenerator.GetBytes(3)),
                EventId = newEvent.Id,
                MembersEdits = [],
                EventEditExecutors = []
            };

            eventEdit.EventEditExecutors.Add(new EventEditExecutor
            {
                ExecutorId = executorId
            });

            foreach (var member in newEvent.Members)
            {
                eventEdit.MembersEdits.Add(new EventMemberEdit
                {
                    MemberId = member.Id,
                });
            }

            _context.EventEdits.Add(eventEdit);
            _context.Records.Remove(record);

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error occurred while ending event for RecordId: {RecordId}", 
                recordId);
            return false;
        }
    }
}