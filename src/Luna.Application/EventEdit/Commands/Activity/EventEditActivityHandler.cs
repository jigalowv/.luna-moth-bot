using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.EventEdit.Commands.Activity;

public class EventEditActivityHandler 
    : IRequestHandler<EventEditActivityRequest, ErrorOr<bool>>
{
    private readonly IExecutorRepository _executorRepository;
    private readonly IEventEditRepository _eventEditRepository;

    public EventEditActivityHandler(
        IExecutorRepository executorRepository,
        IEventEditRepository eventEditRepository
    )
    {
        _executorRepository = executorRepository;
        _eventEditRepository = eventEditRepository;
    }
    
    public async Task<ErrorOr<bool>> Handle(
        EventEditActivityRequest request, 
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

        var target = await _eventEditRepository
            .GetMemberByDiscordIdAsync(
                eventEdit.EventId, request.TargetDiscordId, ct);
        
        if (target is null)
            return Error.NotFound("EventMember.NotFound", 
                $"Участник события (ID: {eventEdit.EventId}) " + 
                $"с Discord ID '{request.TargetDiscordId}' не найден.");
        
        bool success = await _eventEditRepository
            .SetActivityAsync(target.MemberId, request.IsActive, ct);

        if (!success)
            return Error.Failure(
                "Repository.Error", 
                "Ошибка репозитория.");

        return true;
    }
}