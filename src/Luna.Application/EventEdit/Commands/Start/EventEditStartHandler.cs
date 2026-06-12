using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.EventEdit.Commands.Start;

public class EventEditStartHandler 
    : IRequestHandler<EventEditStartRequest, ErrorOr<bool>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEventEditRepository _eventEditRepository;

    public EventEditStartHandler(
        IEventRepository eventRepository,
        IUserRepository userRepository,
        IEventEditRepository eventEditRepository
    )
    {
        _eventRepository = eventRepository;
        _userRepository = userRepository;
        _eventEditRepository = eventEditRepository;
    }
    
    public async Task<ErrorOr<bool>> Handle(
        EventEditStartRequest request, CancellationToken ct)
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
                "'куратор' или выше.");

        var ev = await _eventRepository.GetAsync(request.EventId, ct);

        if (ev is null)
            return Error.NotFound("Event.NotFound", 
                $"Событие с ID {request.EventId} не найдено.");
        
        var eventEdit = await _eventEditRepository
            .GetAsync(request.EventId, ct);
        
        if (eventEdit is not null)
            return Error.Conflict("EventEdit.AlreadyExists", 
                $"Процесс редактирования с ID {request.EventId}" +
                " уже существует.");
        
        bool success = await _eventEditRepository
            .StartAsync(ev.Id, executor.Id, ct);
        
        if (!success)
            return Error.Failure("Ошибка репозитория.");
        
        return true;
    }
}