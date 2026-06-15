using Discord.Interactions;
using Discord.WebSocket;
using MediatR;
using Luna.Application.Users.Commands.Add;
using Luna.Presentation.Extensions;

namespace Luna.Presentation.Modules;

[Group("users", "управление данными о пользователях.")]
public sealed class UsersModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersModule> _logger;

    public UsersModule(
        ILogger<UsersModule> logger,
        IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [SlashCommand("add", "добавляет пользователя в базу данных.")]
    public async Task AddAsync(SocketUser user)
    {
        await DeferAsync(ephemeral: true);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        try
        {
            var request = new UserAddRequest(
                ExecutorDiscordId: Context.User.Id, 
                DiscordId: user.Id);

            var result = await _mediator.Send(request, cts.Token);
            
            await result.Match(
                success => FollowupAsync(embed: EmbedHelper
                    .CreateUpdate("Пользователь добавлен.").Build()),
                errors => FollowupAsync(embed: EmbedHelper
                    .CreateError(errors.First().Description).Build())
            );
        }
        catch (Exception ex)
        {
            await FollowupAsync(embed: EmbedHelper
                .CreateError(
                    "Время ожидания запроса истекло. " + 
                    "Пожалуйста, попробуйте позже.")
                .Build());

            _logger.LogError(ex, "Command Error");
        }
    }
}