using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Entities;
using MediatR;

namespace Luna.Application.Record.Events.UserSetDeafenStatus;

public class UserSetDeafenStatusHandler 
    : IRequestHandler<UserSetDeafenStatusRequest, ErrorOr<bool>>
{
    private readonly IRecordRepository _recordRepository;
    private readonly IRecordAttendanceRepository _recordAttendanceRepository;

    public UserSetDeafenStatusHandler(
        IRecordRepository recordRepository,
        IRecordAttendanceRepository recordAttendanceRepository
    )
    {
        _recordRepository = recordRepository;
        _recordAttendanceRepository = recordAttendanceRepository;
    }

    public async Task<ErrorOr<bool>> Handle(
        UserSetDeafenStatusRequest request, 
        CancellationToken ct)
    {
        var record = await _recordRepository
            .GetAsync(request.ChannelId, ct);

        if (record is null)
            return Error.NotFound(
                "record.NotFound", "Запись не найдена");

        var attendance = RecordAttendance.Create(
            recordId: record.Id,
            discordUserId: request.DiscordUserId,
            isDeafened: request.IsDeafened
        );

        bool success = await _recordAttendanceRepository.EndAndAddAsync(
            attendance: attendance, 
            ct: ct);

        if (!success)
            return Error.Failure(
                "Repository.Failure", "Repository error.");

        return true;
    }
}