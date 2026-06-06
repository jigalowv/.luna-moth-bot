using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.Record.Commands.Cancel;

public sealed class RecordCancelHandler
    : IRequestHandler<RecordCancelRequest, ErrorOr<RecordCancelResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRecordRepository _recordRepository;
    private readonly IRecordAttendanceRepository _recordAttendanceRepository;

    public RecordCancelHandler(
        IUserRepository userRepository,
        IRecordRepository recordRepository,
        IRecordAttendanceRepository recordAttendanceRepository
    )
    {
        _userRepository = userRepository;
        _recordRepository = recordRepository;
        _recordAttendanceRepository = recordAttendanceRepository;
    }

    public async Task<ErrorOr<RecordCancelResponse>> Handle(
        RecordCancelRequest request, 
        CancellationToken ct)
    {
        var executor = await _userRepository
            .GetByDiscordIdAsync(request.ExecutorDiscordId, ct);

        if (executor is null)
            return Error.NotFound(
                "User.ExecutorNotFound", 
                "Записи о вашем аккаунте не существует в репозитории.");

        if (executor.Role < UserRole.Curator)
            return Error.Forbidden(
                "User.NoPermission", 
                "У вас недостаточно прав. Роль исполнителя должна быть " + 
                    $"`{UserRole.Curator}` или выше.");
        
        var record = await _recordRepository
                .GetAsync(request.ChannelId, ct);
        
        if (record is null)
            return Error.NotFound(
                "record.NotFound", 
                "Запись канала не найдена в репозитории.");

        if (executor.Role == UserRole.Curator &&
            executor.Id != record.ExecutorId)
            return Error.Forbidden(
                "User.NoPermission", 
                "У вас недостаточно прав. Если роль исполнителя " + 
                $"`{UserRole.Curator}`, то он может использовать " + 
                "эту команду только на свои записи.");

        var userDeafenDurations = await _recordAttendanceRepository
            .GetUserDurations(record.Id, true, ct);
        
        var userNotDeafenDurations = await _recordAttendanceRepository
            .GetUserDurations(record.Id, false, ct);

        bool success = await _recordRepository
            .CancelAsync(
                channelId: record.ChannelId, 
                ct: ct);
        
        if (!success)
            return Error.Failure(
                "Repository.Error", "Repository error.");

        return new RecordCancelResponse(
            UserDeafenDurations: userDeafenDurations,
            UserNotDeafenDurations: userNotDeafenDurations);
    }
}