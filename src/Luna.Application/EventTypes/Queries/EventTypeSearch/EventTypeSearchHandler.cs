using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.EventTypes.Queries.EventTypeSearch;

public class EventTypeSearchHandler
    : IRequestHandler<EventTypeSearchRequest, ErrorOr<EventTypeSearchResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventTypeRepository _eventTypeRepository;

    public EventTypeSearchHandler(
        IUserRepository userRepository,
        IEventTypeRepository eventTypeRepository)
    {
        _userRepository = userRepository;
        _eventTypeRepository = eventTypeRepository;
    }
    
    public async Task<ErrorOr<EventTypeSearchResponse>> Handle(
        EventTypeSearchRequest request, 
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
                    $"`{UserRole.Curator}` или выше.");
        
        var eventTypes = await _eventTypeRepository.SearchAsync(
            request.SearchTerm, ct);

        return new EventTypeSearchResponse(
            Titles: [.. eventTypes
                .Select(i => i.Title)
                .OrderBy(i => i)]
        );
    }
}