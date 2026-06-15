using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.Executors.Commands.List;

public class ExecutorListHandler 
    : IRequestHandler<ExecutorListRequest, ErrorOr<ExecutorListResponse>>
{
    private readonly IExecutorRepository _executorRepository;

    public ExecutorListHandler(
        IExecutorRepository executorRepository
    )
    {
        _executorRepository = executorRepository;
    }

    public async Task<ErrorOr<ExecutorListResponse>> Handle(
        ExecutorListRequest request, 
        CancellationToken ct)
    {
        var executor = await _executorRepository
            .GetByDiscordIdAsync(request.ExecutorDiscordId, ct);

        if (executor is null)
            return Error.NotFound(
                "User.ExecutorNotFound", 
                "Вы не являетесь исполнителем.");

        if (executor.Role < ExecutorRole.Curator)
            return Error.Forbidden(
                "User.NoPermission", 
                "У вас недостаточно прав. Роль исполнителя должна быть " + 
                "'куратор' или выше.");
        
        var executors = await _executorRepository.GetAllAsync(ct);

        return new ExecutorListResponse(
            Executors: executors
                .OrderByDescending(e => e.Role)
                .Select(i => new ExecutorListResponseItem(
                    DiscordId: i.User.DiscordId,
                    Role: i.Role,
                    Name: i.Name))
                .ToList()
        );
    }
}