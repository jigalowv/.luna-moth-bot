using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Entities;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.Record.Commands.Start;

public sealed class RecordStartHandler 
    : IRequestHandler<RecordStartRequest, ErrorOr<bool>>
{
    private IExecutorRepository _executorRepository;
    private IRecordRepository _recordRepository;

    public RecordStartHandler(
        IExecutorRepository executorRepository,
        IRecordRepository recordRepository
    )
    {
        _executorRepository = executorRepository;
        _recordRepository = recordRepository;
    }

    public async Task<ErrorOr<bool>> Handle(
        RecordStartRequest request, 
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
            
        var record = await _recordRepository
            .GetAsync(request.ChannelId, ct);

        if (record is not null)
            return Error.Conflict(
                "Repository.Conflict", 
                "Канал не должен записываться в текущий момент.");

        record = Domain.Entities.Record.Create(
            channelId: request.ChannelId,
            executorId: executor.UserId);

        List<RecordAttendance> recordAttendances = [];

        foreach (var idAndIsDeafenPair in request.IdAndIsDeafenPairs)
        {
            var attendance = RecordAttendance.Create(
                recordId: 0,
                discordUserId: idAndIsDeafenPair.Key,
                isDeafened: idAndIsDeafenPair.Value);
            
            recordAttendances.Add(attendance);
        }

        var success = await _recordRepository.AddAsync(
            record: record, 
            recordAttendances: recordAttendances, 
            ct: ct);

        if (!success)
            return Error.Failure(
                "Repository.Failure", "Ошибка репозитория.");

        return true;
    }
}