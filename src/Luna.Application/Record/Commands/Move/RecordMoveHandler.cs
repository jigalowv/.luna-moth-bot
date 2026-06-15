using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Entities;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.Record.Commands.Move;

public class RecordMoveHandler : IRequestHandler<RecordMoveRequest, ErrorOr<bool>>
{
    private readonly IExecutorRepository _executorRepository;
    private readonly IRecordRepository _recordRepository;

    public RecordMoveHandler(
        IExecutorRepository executorRepository,
        IRecordRepository recordRepository
    )
    {
        _executorRepository = executorRepository;
        _recordRepository = recordRepository;
    }
    
    public async Task<ErrorOr<bool>> Handle(
        RecordMoveRequest request, 
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
        
        var oldChannel = await _recordRepository
            .GetAsync(request.OldChannelId, ct);
        
        if (oldChannel is null)
            return Error.NotFound(
                "record.NotFound", 
                $"Запись канала (`{request.OldChannelId}`) " + 
                "не найдена в репозитории.");

        if (executor.Role == ExecutorRole.Curator &&
            executor.UserId != oldChannel.ExecutorId)
            return Error.Forbidden(
                "User.NoPermission", 
                "У вас недостаточно прав. Если роль исполнителя " + 
                $"`{ExecutorRole.Curator}`, то он может использовать " + 
                "эту команду только на свои записи.");

        var newChannelId = await _recordRepository
            .GetAsync(request.NewChannelId, ct);

        if (newChannelId is not null)
            return Error.NotFound(
                "record.NotFound", 
                $"Запись канала (`{request.NewChannelId}`) " + 
                "уже существует.");

        List<RecordAttendance> recordAttendances = [];

        foreach (var pair in request.IdAndIsDeafenPairs)
        {
            recordAttendances.Add(
                RecordAttendance.Create(
                recordId: 0,
                discordUserId: pair.Key,
                isDeafened: pair.Value)
            );
        }

        bool success = await _recordRepository.SetNewChannel(
            recordAttendances: recordAttendances, 
            oldChannelId: request.OldChannelId, 
            newChannelId: request.NewChannelId, 
            ct);

        if (!success)
            return Error.Failure(
                "Repository.Failure", "Ошибка репозитория.");

        return true;
    }
}