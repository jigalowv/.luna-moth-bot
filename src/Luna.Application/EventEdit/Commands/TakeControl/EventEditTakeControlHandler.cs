using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.EventEdit.Commands.TakeControl;

public class EventEditTakeControlHandler
    : IRequestHandler<EventEditTakeControlRequest, ErrorOr<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventEditRepository _eventEditRepository;

    public EventEditTakeControlHandler(
        IUserRepository userRepository,
        IEventEditRepository eventEditRepository
    )
    {
        _userRepository = userRepository;
        _eventEditRepository = eventEditRepository;
    }

    public async Task<ErrorOr<bool>> Handle(
        EventEditTakeControlRequest request, 
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
                "'модератор' или выше.");

        var eventEdit = await _eventEditRepository
            .GetAsync(request.EventId, ct);
        
        if (eventEdit is null)
            return Error.NotFound("EventEdit.NotFound", 
                "Процесс изменения не найден.");

        var eventExecutor = await _eventEditRepository
            .GetExecutorAsync(request.EventId, executor.Id, ct);

        if (eventExecutor is not null)
            return Error.Conflict("EventEditExecutor.AlreadyExists",
                "Такой исполнитель уже существует.");
        
        bool success = await _eventEditRepository
            .AddExecutor(request.EventId, executor.Id, ct);
    
        if (!success)
            return Error.Failure("Repository.Error", "Ошибка репозитория.");

        return true;
    }
}