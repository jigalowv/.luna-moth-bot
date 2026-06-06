using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Entities;
using MediatR;

namespace Luna.Application.Record.Events.UserJoinChannel;

public sealed class UserJoinChannelHandler
    : IRequestHandler<UserJoinChannelRequest, ErrorOr<bool>>
{
    private readonly IRecordRepository _recordRepository;
    private readonly IRecordAttendanceRepository _recordAttendanceRepository;

    public UserJoinChannelHandler(
        IRecordRepository recordRepository,
        IRecordAttendanceRepository recordAttendanceRepository
    )
    {
        _recordRepository = recordRepository;
        _recordAttendanceRepository = recordAttendanceRepository;
    }

    public async Task<ErrorOr<bool>> Handle(
        UserJoinChannelRequest request, 
        CancellationToken ct)
    {
        var record = await _recordRepository
            .GetAsync(request.VoiceChannelId, ct);

        if (record is null)
            return Error.NotFound(
                "record.NotFound", "Запись не найдена.");

        var attendance = RecordAttendance.Create(
            recordId: record.Id, 
            discordUserId: request.DiscordUserId, 
            isDeafened: request.IsDeafened);

        bool success = await _recordAttendanceRepository
            .AddAsync(attendance, ct);

        if (!success)
            return Error.Failure(
                "Repository.Failure", "Ошибка репозитория.");

        return true;
    }
}