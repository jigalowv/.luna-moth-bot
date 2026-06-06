using Luna.Domain.Entities;

namespace Luna.Application.Common.Interfaces;

public interface IRecordAttendanceRepository
{
    Task<IReadOnlyDictionary<ulong, TimeSpan>> GetUserDurations(
        int recordId, 
        bool isDeafen, 
        CancellationToken ct
    );
    
    Task<bool> AddAsync(
        RecordAttendance attendance, 
        CancellationToken ct
    );
    
    Task<bool> EndAsync(
        int recordId, 
        ulong discordUserId, 
        CancellationToken ct
    );
    
    Task<bool> EndAndAddAsync(
        RecordAttendance attendance,
        CancellationToken ct
    );
}