using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.Record.Commands.List;

public class RecordListHandler
    : IRequestHandler<RecordListRequest, ErrorOr<RecordListResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRecordRepository _recordRepository;

    public RecordListHandler(
        IUserRepository userRepository,
        IRecordRepository recordRepository
    )
    {
        _userRepository = userRepository;
        _recordRepository = recordRepository;
    }
    
    public async Task<ErrorOr<RecordListResponse>> Handle(
        RecordListRequest request, 
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

        var channels = await _recordRepository.GetAllAsync(ct);
        
        if (channels.Count == 0)
            return Error.NotFound(
                "Record.NotFound", 
                "Текущих записей не найдено.");

        var items = channels
            .Select(i => new RecordListItem(
                StartAt: i.StartAt!.Value,
                ChannelId: i.ChannelId,
                ExecutorDiscordId: i.Executor.DiscordId))
            .OrderByDescending(i => i.StartAt)
            .ToArray();

        return new RecordListResponse(items);
    }
}