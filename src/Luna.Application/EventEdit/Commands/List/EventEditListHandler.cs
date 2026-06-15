using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.EventEdit.Commands.List;

public sealed class EventEditListHandler 
    : IRequestHandler<EventEditListRequest, ErrorOr<EventEditListResponse>>
{
    private readonly IExecutorRepository _executorRepository;
    private readonly IEventEditRepository _eventEditRepository;

    public EventEditListHandler(
        IExecutorRepository executorRepository,
        IEventEditRepository eventEditRepository
    )
    {
        _executorRepository = executorRepository;
        _eventEditRepository = eventEditRepository;
    }

    public async Task<ErrorOr<EventEditListResponse>> Handle(
        EventEditListRequest request, 
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
                    $"куратор или выше.");

        ICollection<Domain.Entities.EventEdit> eventEdits = 
            executor.Role == ExecutorRole.Curator
            ? await _eventEditRepository
                .GetByExecutorIdWithDetailsAsync(executor.UserId, ct)
            : await _eventEditRepository
                .GetAllWithDetailsAsync(ct);

        var items = eventEdits
            .Where(ee => 
                ee.Event is not null && 
                ee.Event.Creator is not null && 
                ee.Event.Type is not null)
            .Select(ee => new EventEditListResponseItem(
                EventId: ee.EventId,
                EventTypeTitle: ee.Event.Type!.Title,
                CreatorDiscordId: ee.Event.Creator!.User.DiscordId,
                StartAt: ee.Event.StartAt
            ))
            .ToList();

        return new EventEditListResponse(items);
    }
}