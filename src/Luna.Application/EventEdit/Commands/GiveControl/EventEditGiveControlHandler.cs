using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.EventEdit.Commands.GiveControl;

public class EventEditGiveControlHandler
    : IRequestHandler<EventEditGiveControlRequest, ErrorOr<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventEditRepository _eventEditRepository;

    public EventEditGiveControlHandler(
        IUserRepository userRepository,
        IEventEditRepository eventEditRepository
    )
    {
        _userRepository = userRepository;
        _eventEditRepository = eventEditRepository;
    }

    public async Task<ErrorOr<bool>> Handle(
        EventEditGiveControlRequest request, 
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
                "'куратор' или выше.");

        Domain.Entities.EventEdit? eventEdit = request.EventId is null ?
            await _eventEditRepository
                .GetLastByExecutorIdAsync(executor.Id, ct) :
            await _eventEditRepository
                .GetAsync(request.EventId.Value, ct);

        if (eventEdit is null)
            return Error.NotFound(
                "EventEdit.NotFound", 
                "процессов изменения, где вы " + 
                "являетесь редактором, не найдено.");
        
        var target = await _userRepository
            .GetByDiscordIdAsync(request.TargetDiscordId, ct);

        if (target is null)
            return Error.NotFound(
                "User.NotFound", 
                "Записи об аккаунте целевого пользователя не найдено.");

        if (target.Role < UserRole.Curator)
            return Error.Forbidden(
                "User.NoPermission", 
                "У целевого пользователя недостаточно прав. "+ 
                "Роль исполнителя должна быть 'куратор' или выше.");
        
        var eventExecutor = await _eventEditRepository
            .GetExecutorAsync(eventEdit.EventId, target.Id, ct);

        if (eventExecutor is not null)
            return Error.Conflict("EventEditExecutor.AlreadyExists",
                "Такой исполнитель уже существует.");
        
        bool success = await _eventEditRepository
            .AddExecutor(eventEdit.EventId, target.Id, ct);
    
        if (!success)
            return Error.Failure("Repository.Error", "Ошибка репозитория.");

        return true;
    }
}