using ErrorOr;
using MediatR;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Entities;

namespace Luna.Application.Users.Commands.Add;

public sealed class UserAddHandler 
    : IRequestHandler<UserAddRequest, ErrorOr<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly IExecutorRepository _executorRepository;

    public UserAddHandler(
        IExecutorRepository executorRepository,
        IUserRepository userRepository)
    {
        _executorRepository = executorRepository;
        _userRepository = userRepository;
    }

    public async Task<ErrorOr<bool>> Handle(
        UserAddRequest request, 
        CancellationToken ct)
    {
        var executor = await _executorRepository
            .GetByDiscordIdAsync(request.ExecutorDiscordId, ct);

        if (executor is null)
            return Error.NotFound(
                "User.ExecutorNotFound", "Исполнитель не найден.");
        
        var user = await _userRepository
            .GetByDiscordIdAsync(request.DiscordId, ct);

        if (user is not null)
            return Error.Conflict(
                "User.AlreadyExists", "Пользователь уже существует.");

        bool success = await _userRepository.AddAsync(new User
        {
            DiscordId = request.DiscordId
        }, ct);

        if (!success)
            return Error.Failure(
                "Repository.Failure", "Ошибка репозитория.");

        return true;
    }
}