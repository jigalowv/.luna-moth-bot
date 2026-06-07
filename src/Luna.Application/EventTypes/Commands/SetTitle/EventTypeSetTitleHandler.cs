using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.EventTypes.Commands.SetTitle;

public class EventTypeSetTitleHandler
    : IRequestHandler<EventTypeSetTitleRequest, ErrorOr<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventTypeRepository _eventTypeRepository;

    public EventTypeSetTitleHandler(
        IUserRepository userRepository,
        IEventTypeRepository eventTypeRepository)
    {
        _userRepository = userRepository;
        _eventTypeRepository = eventTypeRepository;
    }
    
    public async Task<ErrorOr<bool>> Handle(
        EventTypeSetTitleRequest request, 
        CancellationToken ct)
    {
        var executor = await _userRepository
            .GetByDiscordIdAsync(request.ExecutorDiscordId, ct);

        if (executor is null)
            return Error.NotFound(
                "User.ExecutorNotFound", 
                "Записи о вашем аккаунте не существует в репозитории.");

        if (executor.Role < UserRole.Moderator)
            return Error.Forbidden(
                "User.NoPermission", 
                "У вас недостаточно прав. Роль исполнителя должна быть " + 
                    $"`{UserRole.Moderator}` или выше.");

        var normalizedOldTitle = request.OldTitle.Trim().ToLower();
        var normalizedNewTitle = request.NewTitle.Trim().ToLower();
        
        var oldTitleEventType = await _eventTypeRepository
            .GetByTitleAsync(normalizedOldTitle, ct);

        if (oldTitleEventType is null)
            return Error.Conflict("EventType.AlreadyExists", 
                $"События с таким названием ({normalizedOldTitle}) " + 
                "не существует.");

        var newTitleEventType = await _eventTypeRepository
            .GetByTitleAsync(normalizedNewTitle, ct);

        if (newTitleEventType is not null)
            return Error.Conflict("EventType.AlreadyExists", 
                $"Событие с таким названием ({normalizedNewTitle}) " + 
                "уже существует.");

        bool success = await _eventTypeRepository
            .SetTitleAsync(normalizedOldTitle, normalizedNewTitle, ct);

        if (!success)
            return Error.Failure(
                "Repository.Error", 
                "Ошибка репозитория.");
        
        return true;
    }
}