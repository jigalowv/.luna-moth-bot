using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.Record.Commands.End;

public class RecordEndHandler 
    : IRequestHandler<RecordEndRequest, ErrorOr<RecordEndResponse>>
{
    private readonly IEventTypeRepository _eventTypeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRecordRepository _recordRepository;

    public RecordEndHandler(
        IUserRepository userRepository,
        IRecordRepository recordRepository,
        IEventTypeRepository eventTypeRepository
    )
    {
        _eventTypeRepository = eventTypeRepository;
        _userRepository = userRepository;
        _recordRepository = recordRepository;
    }
    
    public async Task<ErrorOr<RecordEndResponse>> Handle(
        RecordEndRequest request, 
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

        var eventType = await _eventTypeRepository
            .GetByTitleAsync(request.EventTypeTitle, ct); 

        if (eventType is null)
            return Error.NotFound("EventType.NotFound", 
                "Событие с таким названием не найдено.");

        var now = DateTime.UtcNow; 
        int minDuration = request.MinDuration ?? 
            (int)(now - record.StartAt!.Value).TotalMinutes / 4;
        MemberRole role = request.Role ?? MemberRole.Player;
        
        var success = await _recordRepository.ToEventAsync(
            recordId: record.Id,
            eventTypeId: eventType.Id,
            executorId: executor.Id,
            minDuration: minDuration, 
            role: role,
            ct: ct);

        if (!success)
            return Error.Failure(
                "Repository.Error", "Ошибка репозитория.");
        
        return new RecordEndResponse();
    }
}