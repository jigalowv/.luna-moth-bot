using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.Record.Commands.Cancel;

public sealed class RecordCancelHandler
    : IRequestHandler<RecordCancelRequest, ErrorOr<RecordCancelResponse>>
{
    private readonly IExecutorRepository _executorRepository;
    private readonly IRecordRepository _recordRepository;
    private readonly IRecordAttendanceRepository _recordAttendanceRepository;

    public RecordCancelHandler(
        IExecutorRepository executorRepository,
        IRecordRepository recordRepository,
        IRecordAttendanceRepository recordAttendanceRepository
    )
    {
        _executorRepository = executorRepository;
        _recordRepository = recordRepository;
        _recordAttendanceRepository = recordAttendanceRepository;
    }

    public async Task<ErrorOr<RecordCancelResponse>> Handle(
        RecordCancelRequest request, 
        CancellationToken ct)
    {
        var executor = await _executorRepository
            .GetByDiscordIdAsync(request.ExecutorDiscordId, ct);

        if (executor is null)
            return Error.NotFound(
                "User.ExecutorNotFound", 
                "Записи о вашем аккаунте не существует в репозитории.");

        if (executor.Role < ExecutorRole.Curator)
            return Error.Forbidden(
                "User.NoPermission", 
                "У вас недостаточно прав. Роль исполнителя должна быть " + 
                    $"`{ExecutorRole.Curator}` или выше.");
        
        var record = await _recordRepository
                .GetAsync(request.ChannelId, ct);
        
        if (record is null)
            return Error.NotFound(
                "record.NotFound", 
                "Запись канала не найдена в репозитории.");

        if (executor.Role == ExecutorRole.Curator &&
            executor.UserId != record.ExecutorId)
            return Error.Forbidden(
                "User.NoPermission", 
                "У вас недостаточно прав. Если роль исполнителя " + 
                $"`{ExecutorRole.Curator}`, то он может использовать " + 
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
                "Repository.Error", "Ошибка репозитория.");

        return new RecordCancelResponse(
            UserDeafenDurations: userDeafenDurations,
            UserNotDeafenDurations: userNotDeafenDurations);
    }
}