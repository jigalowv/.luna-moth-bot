using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.EventEdit.Commands.Type;

public class EventEditTypeHandler 
    : IRequestHandler<EventEditTypeRequest, ErrorOr<bool>>
{
    private readonly IExecutorRepository _executorRepository;
    private readonly IEventEditRepository _eventEditRepository;
    private readonly IEventTypeRepository _eventTypeRepository;

    public EventEditTypeHandler(
        IEventTypeRepository eventTypeRepository,
        IExecutorRepository executorRepository,
        IEventEditRepository eventEditRepository
    )
    {
        _eventTypeRepository = eventTypeRepository;
        _executorRepository = executorRepository;
        _eventEditRepository = eventEditRepository;
    }
    
    public async Task<ErrorOr<bool>> Handle(
        EventEditTypeRequest request, 
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
                "куратор или выше.");

        Domain.Entities.EventEdit? eventEdit = request.EventId is null ?
            await _eventEditRepository
                .GetLastByExecutorIdAsync(executor.UserId, ct) :
            await _eventEditRepository
                .GetAsync(request.EventId.Value, ct);

        if (eventEdit is null)
            return Error.NotFound(
                "EventEdit.NotFound", 
                "процессов изменения, где вы " + 
                "являетесь редактором, не найдено.");

        var eventType = await _eventTypeRepository
            .GetByTitleAsync(request.EventTypeTitle, ct);
        
        if (eventType is null)
            return Error.NotFound("EventType.NotFound", 
                $"Тип события '{request.EventTypeTitle}' не найден.");
        
        bool success = await _eventEditRepository
            .SetType(eventEdit.EventId, eventType.Id, ct);

        if (!success)
            return Error.Failure("Ошибка репозитория.");
        
        return true;
    }
}