using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.EventEdit.Commands.List;

public sealed class EventEditListHandler 
    : IRequestHandler<EventEditListRequest, ErrorOr<EventEditListResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventEditRepository _eventEditRepository;

    public EventEditListHandler(
        IUserRepository userRepository,
        IEventEditRepository eventEditRepository
    )
    {
        _userRepository = userRepository;
        _eventEditRepository = eventEditRepository;
    }

    public async Task<ErrorOr<EventEditListResponse>> Handle(
        EventEditListRequest request, 
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
                    $"куратор или выше.");

        ICollection<Domain.Entities.EventEdit> eventEdits = 
            executor.Role == UserRole.Curator
            ? await _eventEditRepository
                .GetByExecutorIdWithDetailsAsync(executor.Id, ct)
            : await _eventEditRepository
                .GetAllWithDetailsAsync(ct);

        var items = eventEdits
            .Where(ee => 
                ee.Event is not null && 
                ee.TempEvent is not null &&
                ee.Event.Creator is not null && 
                ee.Event.Type is not null)
            .Select(ee => new EventEditListResponseItem(
                TempEventId: ee.TempEventId,
                EventId: ee.EventId,
                EventTypeTitle: ee.Event.Type!.Title,
                CreatorDiscordId: ee.Event.Creator!.DiscordId,
                StartAt: ee.Event.StartAt
            ))
            .ToList();

        return new EventEditListResponse(items);
    }
}