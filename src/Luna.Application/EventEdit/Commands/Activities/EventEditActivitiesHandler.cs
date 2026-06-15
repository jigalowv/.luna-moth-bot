using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.EventEdit.Commands.Activities;

public class EventEditActivitiesHandler 
    : IRequestHandler<EventEditActivitiesRequest, ErrorOr<bool>>
{
    private readonly IExecutorRepository _executorRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEventEditRepository _eventEditRepository;

    public EventEditActivitiesHandler(
        IExecutorRepository executorRepository,
        IUserRepository userRepository,
        IEventEditRepository eventEditRepository
    )
    {
        _executorRepository = executorRepository;
        _userRepository = userRepository;
        _eventEditRepository = eventEditRepository;
    }
    
    public async Task<ErrorOr<bool>> Handle(
        EventEditActivitiesRequest request, 
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

        var targets = await _userRepository.GetAllByDiscordIdsAsync(
            request.TargetsDiscordIds, ct);
        
        if (targets.Count == 0)
            return Error.NotFound("EventMember.NotFound", 
                $"Участников события (ID: {eventEdit.EventId}) не найдено.");
        
        bool success = await _eventEditRepository
            .SetActivitiesAsync(
                eventEdit.EventId, 
                [.. targets.Select(i => i.Id)], 
                request.IsActive, 
                ct);

        if (!success)
            return Error.Failure(
                "Repository.Error", 
                "Ошибка репозитория.");

        return true;
    }
}