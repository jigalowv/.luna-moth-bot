using Discord.Interactions;
using Discord.WebSocket;
using MediatR;
using Luna.Application.Users.Commands.AddUser;
using Luna.Application.Users.Commands.SetUserRole;
using Luna.Domain.Enums;

namespace Luna.Presentation.Modules;

[Group("users", "управление данными о пользователях.")]
public sealed class UsersModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IMediator _mediator;

    public UsersModule(IMediator mediator)
    {
        _mediator = mediator;
    }

    [SlashCommand("add", "добавляет пользователя в базу данных.")]
    public async Task AddAsync(SocketUser user, UserRole role = UserRole.None)
    {
        await DeferAsync(ephemeral: true);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        try
        {
            var request = new AddUserRequest(
                ExecutorDiscordId: Context.User.Id, 
                DiscordId: user.Id,
                Role: role);

            var result = await _mediator.Send(request, cts.Token);
            
            await result.Match(
                success => FollowupAsync("Успешно добавлен."),
                errors => FollowupAsync($"Ошибка: {errors.First().Description}")
            );
        }
        catch (Exception)
        {
            await FollowupAsync("The request timed out. Please try again later.");
        }
    }

    [SlashCommand("setrole", "устанавливает новую роль для пользователя.")]
    public async Task SetRoleAsync(SocketUser user, UserRole newRole)
    {
        await DeferAsync(ephemeral: true);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        try
        {
            var request = new SetUserRoleRequest(
                ExecutorDiscordId: Context.User.Id, 
                DiscordId: user.Id, 
                NewRole: newRole);

            var result = await _mediator.Send(request, cts.Token);

            await result.Match(
                success => FollowupAsync("Роль успешно обновлена."),
                errors => FollowupAsync($"Ошибка: {errors.First().Description}")
            );
        }
        catch (Exception)
        {
            await FollowupAsync("The request timed out. Please try again later.");
        }
    }
}