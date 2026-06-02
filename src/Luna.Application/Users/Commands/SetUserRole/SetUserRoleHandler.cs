using ErrorOr;
using MediatR;
using Luna.Application.Common.Interfaces;

namespace Luna.Application.Users.Commands.SetUserRole;

public class SetUserRoleHandler 
    : IRequestHandler<SetUserRoleRequest, ErrorOr<bool>>
{
    public readonly IUserRepository _userRepository;

    public SetUserRoleHandler(
        IUserRepository userRepository
    )
    {
        _userRepository = userRepository;
    }

    public async Task<ErrorOr<bool>> Handle(
        SetUserRoleRequest request, 
        CancellationToken ct)
    {
        var executor = await _userRepository
            .GetByDiscordIdAsync(request.ExecutorDiscordId, ct);
        
        if (executor is null)
            return Error.NotFound(
                "User.ExecutorNotFound", "Executor not found");

        var user = await _userRepository
            .GetByDiscordIdAsync(request.DiscordId, ct);

        if (user is null)
            return Error.NotFound(
                "User.NotFound", "User not found");

        if (!executor.CanReassignRole(user.Role, request.NewRole))
            return Error.Forbidden(
                "User.NoPermission", "Executor has no rights");
        
        if (user.Role == request.NewRole)
            return Error.Conflict(
                "User.AlreadyHasRole", "User already has this role");

        var success = await _userRepository
            .SetUserRoleAsync(request.DiscordId, request.NewRole, ct);

        if (!success)
            return Error.Failure(
                "Repository.Failure", "Repository error.");

        return true;
    }
}