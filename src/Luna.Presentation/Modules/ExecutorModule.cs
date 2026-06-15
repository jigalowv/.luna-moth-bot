using System.Text;
using Discord.Interactions;
using Discord.WebSocket;
using Luna.Application.Executors.Commands.Add;
using Luna.Application.Executors.Commands.List;
using Luna.Application.Executors.Commands.SetRole;
using Luna.Domain.Enums;
using Luna.Presentation.Enums;
using Luna.Presentation.Extensions;
using MediatR;

namespace Luna.Presentation.Modules;

[Group("executors", "Управление исполнителями.")]
public class ExecutorModule 
    : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<ExecutorModule> _logger;
    private readonly IMediator _mediator;

    public ExecutorModule(
        IMediator mediator,
        ILogger<ExecutorModule> logger)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [SlashCommand("add", "Добавить нового исполнителя.")]
    public async Task AddAsync(
        SocketUser user, 
        SetExecutorRole role, 
        string? name = null, 
        string? imageUrl = null)
    {
        await DeferAsync(ephemeral: false);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        try
        {
            name ??= user.Username;

            var request = new ExecutorAddRequest(
                ExecutorDiscordId: Context.User.Id, 
                TargetDiscordId: user.Id, 
                Role: (ExecutorRole)role,
                Name: name,
                ImageUrl: imageUrl);

            var response = await _mediator.Send(request, cts.Token);

            await response.Match(
                success => FollowupAsync(embed: EmbedHelper
                    .CreateUpdate("Исполнитель добавлен.").Build()),
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

    [SlashCommand("setrole", "Установить новую роль.")]
    public async Task SetRoleAsync(SocketUser user, SetExecutorRole newRole)
    {
        await DeferAsync(ephemeral: false);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        try
        {
            var request = new ExecutorSetRoleRequest(
                ExecutorDiscordId: Context.User.Id, 
                DiscordId: user.Id, 
                NewRole: (ExecutorRole)newRole);

            var response = await _mediator.Send(request, cts.Token);

            await response.Match(
                success => FollowupAsync(embed: EmbedHelper
                    .CreateUpdate("Роль исполнителя обновлена.").Build()),
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

    [SlashCommand("list", "выводит текущих действующих исполнителей.")]
    public async Task ListAsync()
    {
        await DeferAsync(ephemeral: false);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        try
        {
            var request = new ExecutorListRequest(
                ExecutorDiscordId: Context.User.Id);

            var response = await _mediator.Send(request, cts.Token);

            if (response.IsError)
            {
                await FollowupAsync(embed: EmbedHelper
                    .CreateError(response.Errors.First().Description).Build());
                return;
            }
            
            var sb = new StringBuilder();
            foreach (var executor in response.Value.Executors)
            {
                string role = executor.Role.GetName();
                string id = $"<@{executor.DiscordId}>";
                sb.AppendLine($"- {role} {id} ({executor.Name})");
            }

            await FollowupAsync(embed: EmbedHelper.CreateBase(
                title: "Исполнители", 
                description: sb.ToString()
            ).Build());
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