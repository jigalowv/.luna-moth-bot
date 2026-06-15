using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.EventTypes.Queries.EventTypeSearch;

public class EventTypeSearchHandler
    : IRequestHandler<EventTypeSearchRequest, ErrorOr<EventTypeSearchResponse>>
{
    private readonly IExecutorRepository _executorRepository;
    private readonly IEventTypeRepository _eventTypeRepository;

    public EventTypeSearchHandler(
        IExecutorRepository executorRepository,
        IEventTypeRepository eventTypeRepository)
    {
        _executorRepository = executorRepository;
        _eventTypeRepository = eventTypeRepository;
    }
    
    public async Task<ErrorOr<EventTypeSearchResponse>> Handle(
        EventTypeSearchRequest request, 
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
                    $"`куратор` или выше.");
        
        var eventTypes = await _eventTypeRepository.SearchAsync(
            request.SearchTerm, ct);

        return new EventTypeSearchResponse(
            Titles: [.. eventTypes
                .Select(i => i.Title)
                .OrderBy(i => i)]
        );
    }
}