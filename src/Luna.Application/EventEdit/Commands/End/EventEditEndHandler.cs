using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.EventEdit.Commands.End;

public class EventEditEndHandler 
    : IRequestHandler<EventEditEndRequest, ErrorOr<string?>>
{
    private readonly IExecutorRepository _executorRepository;
    private readonly IEventEditRepository _eventEditRepository;

    public EventEditEndHandler(
        IExecutorRepository executorRepository,
        IEventEditRepository eventEditRepository
    )
    {
        _executorRepository = executorRepository;
        _eventEditRepository = eventEditRepository;
    }
    
    public async Task<ErrorOr<string?>> Handle(
        EventEditEndRequest request, 
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
                "'куратор' или выше.");

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
        
        if (request.EndCode != eventEdit.EndCode)
            return eventEdit.EndCode;
        
        bool success = await _eventEditRepository
            .EndAsync(eventEdit.EventId, ct);

        if (!success)
            return Error.Failure(
                "Repository.Error", 
                "Ошибка репозитория.");
        
        return (string?)null;
    }
}