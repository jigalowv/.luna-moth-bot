using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.Record.Commands.Users;

public sealed class RecordUsersHandler
    : IRequestHandler<RecordUsersRequest, ErrorOr<RecordUsersResponse>>
{
    private IExecutorRepository _executorRepository;
    private IRecordRepository _recordRepository;
    private IRecordAttendanceRepository _recordAttendanceRepository;

    public RecordUsersHandler(
        IExecutorRepository executorRepository,
        IRecordRepository recordRepository,
        IRecordAttendanceRepository recordAttendanceRepository
    )
    {
        _executorRepository = executorRepository;
        _recordRepository = recordRepository;
        _recordAttendanceRepository = recordAttendanceRepository;
    }
    
    public async Task<ErrorOr<RecordUsersResponse>> Handle(
        RecordUsersRequest request, 
        CancellationToken ct)
    {
        var executor = await _executorRepository
            .GetByDiscordIdAsync(request.ExecutorDiscordId, ct);

        if (executor is null)
            return Error.NotFound(
                "User.ExecutorNotFound", 
                "Записи о вашем аккаунте нет в репозитории.");

        if (executor.Role < ExecutorRole.Curator)
            return Error.Forbidden(
                "User.NoPermission", 
                "У вас недостаточно прав. Роль исполнителя должна быть " + 
                    $"`{ExecutorRole.Curator}` или выше.");
            
        var record = request.ChannelId is null ?
            await _recordRepository
                .GetByCreatorIdAsync(executor.UserId, ct) : 
            await _recordRepository
                .GetAsync(request.ChannelId.Value, ct);

        if (record is null)
            return Error.Conflict(
                "Repository.Conflict", 
                "Канал не найден в репозитории.");

        var deafenDurations = await _recordAttendanceRepository
            .GetUserDurations(record.Id, true, ct);
        
        var notDeafenDurations = await _recordAttendanceRepository
            .GetUserDurations(record.Id, false, ct);

        return new RecordUsersResponse(
            ChannelId: record.ChannelId,
            UserDeafenDurations: deafenDurations,
            UserNotDeafenDurations: notDeafenDurations
        );
    }
}