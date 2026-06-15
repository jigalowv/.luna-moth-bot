using ErrorOr;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Entities;
using MediatR;

namespace Luna.Application.Executors.Commands.Add;

public class ExecutorAddHandler
    : IRequestHandler<ExecutorAddRequest, ErrorOr<bool>>
{
    public readonly IUserRepository _userRepository;
    public readonly IExecutorRepository _executorRepository;

    public ExecutorAddHandler(
        IExecutorRepository executorRepository,
        IUserRepository userRepository
    )
    {
        _executorRepository = executorRepository;
        _userRepository = userRepository;
    }

    public async Task<ErrorOr<bool>> Handle(
        ExecutorAddRequest request, 
        CancellationToken ct)
    {
        var executor = await _executorRepository
            .GetByDiscordIdAsync(request.ExecutorDiscordId, ct);
        
        if (executor is null)
            return Error.NotFound(
                "Executor.NotFound", "Исполнитель не найден.");

        var target = await _userRepository
            .GetByDiscordIdAsync(request.TargetDiscordId, ct);
        
        bool success;
        if (target is null)
        {
            success = await _userRepository.AddAsync(new User
            { 
                DiscordId = request.TargetDiscordId 
            }, ct);

            if (!success)
                return Error.Failure(
                    "Repository.Error", "Ошибка репозитория.");
        }

        var targetExecutor = await _executorRepository
            .GetByDiscordIdAsync(request.TargetDiscordId, ct);
        
        if (targetExecutor is not null)
            return Error.Conflict(
                "Executor.AlreadyExists", 
                "Исполнитель уже существует.");
        
        success = await _executorRepository
            .AddAsync(request.TargetDiscordId, new Executor
        {
            Name = request.Name,
            ImageUrl = request.ImageUrl,
            Role = request.Role
        }, ct);

        if (!success)
            return Error.Failure(
                "Repository.Error", "Ошибка репозитория.");
        
        return true;
    }
}