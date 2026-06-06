using ErrorOr;
using MediatR;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Entities;

namespace Luna.Application.Users.Commands.AddUser;

public sealed class AddUserHandler 
    : IRequestHandler<AddUserRequest, ErrorOr<bool>>
{
    private readonly IUserRepository _userRepository;

    public AddUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ErrorOr<bool>> Handle(
        AddUserRequest request, 
        CancellationToken ct)
    {
        var executor = await _userRepository
            .GetByDiscordIdAsync(request.ExecutorDiscordId, ct);
        
        var user = await _userRepository
            .GetByDiscordIdAsync(request.DiscordId, ct);

        if (executor is null)
            return Error.NotFound(
                "User.ExecutorNotFound", "Executor not found.");
        
        if (!executor.CanAssignRole(request.Role))
            return Error.Forbidden(
                "User.NoPermission", "Executor has no rights.");

        if (user is not null)
            return Error.Conflict(
                "User.AlreadyExists", "User already exists.");

        User newUser = User.Create(request.DiscordId, request.Role);
        
        bool success = await _userRepository.AddUserAsync(newUser, ct);

        if (!success)
            return Error.Failure(
                "Repository.Failure", "Repository error.");

        return true;
    }
}