using ErrorOr;
using Luna.Application.Common.Dtos;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.EventEdit.Commands.Show;

public class EventEditShowHandler 
    : IRequestHandler<EventEditShowRequest, ErrorOr<EventEditShowResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventEditRepository _eventEditRepository;

    public EventEditShowHandler(
        IUserRepository userRepository,
        IEventEditRepository eventEditRepository
    )
    {
        _userRepository = userRepository;
        _eventEditRepository = eventEditRepository;
    }
    
    public async Task<ErrorOr<EventEditShowResponse>> Handle(
        EventEditShowRequest request, 
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
                .GetLastByExecutorIdWithDetailsAsync(executor.Id, ct) :
            await _eventEditRepository
                .GetWithDetailsAsync(request.EventId.Value, ct);

        if (eventEdit is null)
            return Error.NotFound(
                "EventEdit.NotFound", 
                "процессов изменения, где вы " + 
                "являетесь редактором, не найдено.");

        List<EventEditShowResponseMember> members = [];

        var editsDictionary = eventEdit.MembersEdits
            .ToDictionary(m => m.MemberId);

        foreach (var before in eventEdit.Event.Members)
        {
            if (editsDictionary.TryGetValue(before.Id, out var after))
            {
                members.Add(new EventEditShowResponseMember(
                    DiscordId: before.User.DiscordId,
                    IsActive: new StructChange<bool>(
                        Before: before.IsActive,
                        After: after.NewActivityStatus
                    ),
                    Role: new StructChange<MemberRole>(
                        Before: before.Role, 
                        After: after.NewRole
                    )
                ));
            }
        }

        return new EventEditShowResponse(
            EventTypeTitle: new ClassChange<string>(
                Before: eventEdit.Event.Type.Title, 
                After: eventEdit.NewEventType?.Title),
            Members: members,
            EventCreatorDiscordId: eventEdit.Event.Creator.DiscordId,
            StartAt: eventEdit.Event.StartAt,
            EndAt: eventEdit.Event.EndAt);
    }
}