using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.EventTypes.Commands.List;

public sealed class EventTypeListHandler
    : IRequestHandler<EventTypeListRequest, ErrorOr<EventTypeListResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventTypeRepository _eventTypeRepository;

    public EventTypeListHandler(
        IUserRepository userRepository,
        IEventTypeRepository eventTypeRepository)
    {
        _userRepository = userRepository;
        _eventTypeRepository = eventTypeRepository;
    }

    public async Task<ErrorOr<EventTypeListResponse>> Handle(
        EventTypeListRequest request, 
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
        
        var eventTypes = await _eventTypeRepository.GetAllAsync(ct);

        return new EventTypeListResponse(
            Titles: eventTypes
                .Select(i => i.Title)
                .OrderBy(i => i)
                .ToList()
        );
    }
}