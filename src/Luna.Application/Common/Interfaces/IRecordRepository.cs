using Luna.Domain.Entities;

namespace Luna.Application.Common.Interfaces;

public interface IRecordRepository
{
    Task<bool> AddAsync(
        Domain.Entities.Record record, 
        ICollection<RecordAttendance> recordAttendances, 
        CancellationToken ct);

    Task<bool> CancelAsync(ulong channelId, CancellationToken ct);
    Task<bool> SetNewChannel(
        ICollection<RecordAttendance> recordAttendances, 
        ulong oldChannelId, 
        ulong newChannelId, 
        CancellationToken ct);

    Task<Domain.Entities.Record?> GetAsync(ulong id, CancellationToken ct);
    Task<Domain.Entities.Record?> GetByCreatorIdAsync(int CreatorId, CancellationToken ct);
    Task<IReadOnlyCollection<Domain.Entities.Record>> GetAllAsync(CancellationToken ct);
}