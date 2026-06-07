using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Entities;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.EventTypes.Commands.Add;

public sealed class EventTypeAddHandler 
    : IRequestHandler<EventTypeAddRequest, ErrorOr<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventTypeRepository _eventTypeRepository;

    public EventTypeAddHandler(
        IUserRepository userRepository,
        IEventTypeRepository eventTypeRepository
    )
    {
        _userRepository = userRepository;
        _eventTypeRepository = eventTypeRepository;
    }

    public async Task<ErrorOr<bool>> Handle(
        EventTypeAddRequest request, 
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
        
        var normalizedTitle = request.Title.Trim().ToLower();

        var eventType = await _eventTypeRepository
            .GetByTitleAsync(normalizedTitle, ct);

        if (eventType is not null)
            return Error.Conflict("EventType.AlreadyExists", 
                $"Событие с таким названием ({normalizedTitle}) " + 
                "уже существует.");

        eventType = EventType.Create(normalizedTitle);

        bool success = await _eventTypeRepository
            .AddAsync(eventType, ct);

        if (!success)
            return Error.Failure(
                "Repository.Error", 
                "Ошибка репозитория.");
        
        return true;
    }
}