using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.Record.Commands.List;

public class RecordListHandler
    : IRequestHandler<RecordListRequest, ErrorOr<RecordListResponse>>
{
    private readonly IExecutorRepository _executorRepository;
    private readonly IRecordRepository _recordRepository;

    public RecordListHandler(
        IExecutorRepository executorRepository,
        IRecordRepository recordRepository
    )
    {
        _executorRepository = executorRepository;
        _recordRepository = recordRepository;
    }
    
    public async Task<ErrorOr<RecordListResponse>> Handle(
        RecordListRequest request, 
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
                    $"`{ExecutorRole.Curator}` или выше.");

        var channels = await _recordRepository.GetAllAsync(ct);
        
        if (channels.Count == 0 || channels is null)
            return Error.NotFound(
                "Record.NotFound", 
                "Текущих записей не найдено.");

        var items = channels
            .Select(i => new RecordListItem(
                StartAt: i.StartAt!.Value,
                ChannelId: i.ChannelId,
                ExecutorDiscordId: i.Executor.User.DiscordId))
            .OrderByDescending(i => i.StartAt)
            .ToArray();

        return new RecordListResponse(items);
    }
}