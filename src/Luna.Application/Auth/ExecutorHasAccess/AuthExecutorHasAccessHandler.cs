using ErrorOr;
using Luna.Application.Common.Interfaces;
using MediatR;

namespace Luna.Application.Auth.ExecutorHasAccess;

public sealed class AuthExecutorHasAccessHandler
    : IRequestHandler<AuthExecutorHasAccessRequest, ErrorOr<bool>>
{
    private readonly IExecutorRepository _executorRepository;

    public AuthExecutorHasAccessHandler(
        IExecutorRepository executorRepository
    )
    {
        _executorRepository = executorRepository;
    }
    public async Task<ErrorOr<bool>> Handle(
        AuthExecutorHasAccessRequest request, 
        CancellationToken ct)
    {
        var executor = await _executorRepository
            .GetByDiscordIdAsync(request.ExecutorDiscordId, ct);
        
        if (executor is null)
            return Error.NotFound(
                "Executor.NotFound", "Исполнитель не найден.");

        if (executor.Role < request.MinRole)
            return Error.NotFound(
                "Executor.NoPermission", "Недостаточно прав.");
        
        return true;
    }
}