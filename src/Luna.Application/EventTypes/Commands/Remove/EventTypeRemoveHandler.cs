using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.EventTypes.Commands.Remove;

public class EventTypeRemoveHandler 
    : IRequestHandler<EventTypeRemoveRequest, ErrorOr<bool>>
{
    private readonly IExecutorRepository _executorRepository;
    private readonly IEventTypeRepository _eventTypeRepository;

    public EventTypeRemoveHandler(
        IExecutorRepository executorRepository,
        IEventTypeRepository eventTypeRepository)
    {
        _executorRepository = executorRepository;
        _eventTypeRepository = eventTypeRepository;
    }

    public async Task<ErrorOr<bool>> Handle(
        EventTypeRemoveRequest request, 
        CancellationToken ct)
    {
        var executor = await _executorRepository
            .GetByDiscordIdAsync(request.ExecutorDiscordId, ct);

        if (executor is null)
            return Error.NotFound(
                "User.ExecutorNotFound", 
                "Записи о вашем аккаунте не существует в репозитории.");

        if (executor.Role < ExecutorRole.Moderator)
            return Error.Forbidden(
                "User.NoPermission", 
                "У вас недостаточно прав. Роль исполнителя должна быть " + 
                    $"`модератор` или выше.");
        
        var normalizedTitle = request.Title.Trim().ToLower();

        var eventType = await _eventTypeRepository
            .GetByTitleAsync(normalizedTitle, ct);

        if (eventType is null)
            return Error.Conflict("EventType.AlreadyExists", 
                $"События с таким названием ({normalizedTitle}) " + 
                "не существует.");

        bool success = await _eventTypeRepository
            .RemoveAsync(normalizedTitle, ct);

        if (!success)
            return Error.Failure(
                "Repository.Error", 
                "Ошибка репозитория.");
        
        return true;
    }
}