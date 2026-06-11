using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.EventEdit.Commands.Role;

public class EventEditRoleHandler 
    : IRequestHandler<EventEditRoleRequest, ErrorOr<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventEditRepository _eventEditRepository;

    public EventEditRoleHandler(
        IUserRepository userRepository,
        IEventEditRepository eventEditRepository
    )
    {
        _userRepository = userRepository;
        _eventEditRepository = eventEditRepository;
    }
    
    public async Task<ErrorOr<bool>> Handle(
        EventEditRoleRequest request, 
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
                "куратор или выше.");

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

        var target = await _eventEditRepository
            .GetMemberByDiscordIdAsync(
                eventEdit.TempEventId, request.TargetDiscordId, ct);
        
        if (target is null)
            return Error.NotFound("EventMember.NotFound", 
                $"Участник события (ID: {eventEdit.EventId}) " + 
                $"с Discord ID '{request.TargetDiscordId}' не найден.");
        
        bool success = await _eventEditRepository
            .SetRoleAsync(target.Id, request.Role, ct);

        if (!success)
            return Error.Failure(
                "Repository.Error", 
                "Ошибка репозитория.");

        return true;
    }
}