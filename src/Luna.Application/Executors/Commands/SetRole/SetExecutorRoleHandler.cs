using ErrorOr;
using MediatR;
using Luna.Application.Common.Interfaces;

namespace Luna.Application.Executors.Commands.SetRole;

public class SetExecutorRoleHandler 
    : IRequestHandler<ExecutorSetRoleRequest, ErrorOr<bool>>
{
    public readonly IExecutorRepository _executorRepository;

    public SetExecutorRoleHandler(
        IExecutorRepository executorRepository
    )
    {
        _executorRepository = executorRepository;
    }

    public async Task<ErrorOr<bool>> Handle(
        ExecutorSetRoleRequest request, 
        CancellationToken ct)
    {
        var executor = await _executorRepository
            .GetByDiscordIdAsync(request.ExecutorDiscordId, ct);
        
        if (executor is null)
            return Error.NotFound(
                "User.ExecutorNotFound", "Исполнитель не найден.");

        var target = await _executorRepository
            .GetByDiscordIdAsync(request.DiscordId, ct);

        if (target is null)
            return Error.NotFound(
                "User.NotFound", "Целевой исполнитель не найден.");

        if (!executor.CanReassignRole(target.Role, request.NewRole))
            return Error.Forbidden(
                "User.NoPermission", "Недостаточно прав.");
        
        if (target.Role == request.NewRole)
            return Error.Conflict(
                "User.AlreadyHasRole", "Целевой исполнитель уже имеет эту роль");

        var success = await _executorRepository
            .SetRoleAsync(request.DiscordId, request.NewRole, ct);

        if (!success)
            return Error.Failure(
                "Repository.Failure", "Ошибка репозитория.");

        return true;
    }
}