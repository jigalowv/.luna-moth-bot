using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.Record.Commands.Users;

public sealed class RecordUsersHandler
    : IRequestHandler<RecordUsersRequest, ErrorOr<RecordUsersResponse>>
{
    private IUserRepository _userRepository;
    private IRecordRepository _recordRepository;
    private IRecordAttendanceRepository _recordAttendanceRepository;

    public RecordUsersHandler(
        IUserRepository userRepository,
        IRecordRepository recordRepository,
        IRecordAttendanceRepository recordAttendanceRepository
    )
    {
        _userRepository = userRepository;
        _recordRepository = recordRepository;
        _recordAttendanceRepository = recordAttendanceRepository;
    }
    
    public async Task<ErrorOr<RecordUsersResponse>> Handle(
        RecordUsersRequest request, 
        CancellationToken ct)
    {
        var executor = await _userRepository
            .GetByDiscordIdAsync(request.ExecutorDiscordId, ct);

        if (executor is null)
            return Error.NotFound(
                "User.ExecutorNotFound", 
                "Записи о вашем аккаунте нет в репозитории.");

        if (executor.Role < UserRole.Curator)
            return Error.Forbidden(
                "User.NoPermission", 
                "У вас недостаточно прав. Роль исполнителя должна быть " + 
                    $"`{UserRole.Curator}` или выше.");
            
        var record = request.ChannelId is null ?
            await _recordRepository
                .GetByCreatorIdAsync(executor.Id, ct) : 
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