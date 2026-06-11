using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Luna.Application.EventEdit.Commands.Activity;
using Luna.Application.EventEdit.Commands.List;
using Luna.Application.EventEdit.Commands.Role;
using Luna.Application.EventEdit.Commands.Show;
using Luna.Application.EventEdit.Commands.Type;
using Luna.Domain.Enums;
using Luna.Presentation.Enums;
using Luna.Presentation.Extensions;
using MediatR;

namespace Luna.Presentation.Modules;

public abstract class EventEditModule 
    : InteractionModuleBase<SocketInteractionContext>
{
    protected readonly IMediator _mediator;
    protected readonly ILogger<EventEditModule> _logger;

    public EventEditModule(
        IMediator mediator,
        ILogger<EventEditModule> logger
    )
    {
        _logger = logger;
        _mediator = mediator;
    }

    [SlashCommand("list", "Вывести все доступные процессы изменений.")]
    public async Task ListAsync()
    {
        await DeferAsync(ephemeral: true);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        
        try
        {
            var request = new EventEditListRequest(
                ExecutorDiscordId: Context.User.Id
            );

            var response = await _mediator.Send(request, cts.Token);

            if (response.IsError)
            {
                await FollowupAsync(
                    $"Ошибка: {response.Errors.First().Description}");
                return;
            }

            if (response.Value.Items.Count == 0)
            {
                await FollowupAsync($"Возвращено 0 записей.");
                return;
            }

            var sb = new StringBuilder();

            foreach (var item in response.Value.Items)
            {
                string creatorStr = item.CreatorDiscordId == Context.User.Id ?
                    "**вы**" : $"<@{item.CreatorDiscordId}>";

                long timestamp = ((DateTimeOffset)item.StartAt)
                    .ToUnixTimeSeconds();

                sb.Append($"- [`{item.EventId}`] ")
                  .Append($"<t:{timestamp}:f> ")
                  .Append($"`{item.EventTypeTitle}`: ")
                  .AppendLine(creatorStr);
            }

            var eb = new EmbedBuilder()
                .WithDescription(sb.ToString())
                .WithTitle("Процессы изменений:")
                .WithColor(Color.Blue)
                .WithCurrentTimestamp();

            await FollowupAsync(embed: eb.Build());
        }
        catch (Exception ex)
        {
            await FollowupAsync("The request timed out. Please try again later.");
            _logger.LogError(ex, "Command Error");
        }
        
    }

    [SlashCommand("show", "Вывести информацию о редактируемом событии.")]
    public async Task ShowAsync([MinValue(1)]int? eventId = null)
    {
        await DeferAsync(ephemeral: true);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        
        try
        {
            var request = new EventEditShowRequest(
                ExecutorDiscordId: Context.User.Id,
                EventId: eventId
            );

            var response = await _mediator.Send(request, cts.Token);

            if (response.IsError)
            {
                await FollowupAsync(
                    $"Ошибка: {response.Errors.First().Description}");
                return;
            }

            string title = response.Value.EventTypeTitle.Before == 
                response.Value.EventTypeTitle.After ? 
                response.Value.EventTypeTitle.Before :
                $"{response.Value.EventTypeTitle.Before} → " +
                $"{response.Value.EventTypeTitle.After}";

            long startTS = ((DateTimeOffset)response.Value.StartAt)
                .ToUnixTimeSeconds();
            
            long endTS = ((DateTimeOffset)response.Value.EndAt)
                .ToUnixTimeSeconds();

            var eb = new EmbedBuilder()
                .WithTitle(title)
                .WithColor(Color.Blue)
                .AddField("Начало", $"<t:{startTS}:f>", true)
                .AddField("Конец", $"<t:{endTS}:f>", true )
                .AddField("Создал", $"<@{response.Value.EventCreatorDiscordId}>", true)
                .WithCurrentTimestamp();

            var sb = new StringBuilder()
                .AppendLine("## Участники:");
            
            foreach (var member in response.Value.Members
                .OrderByDescending(i => i.Role.Before))
            {
                string role = member.Role.Before == member.Role.After ?
                    member.Role.Before.GetName() :
                    $"{member.Role.Before.GetName()} → " + 
                    $"{member.Role.After.GetName()}";
                
                string bIsActive = member.IsActive.Before ? 
                    ":green_circle:" : ":red_circle:";

                string aIsActive = member.IsActive.After ? 
                    ":green_circle:" : ":red_circle:";
 
                string isActive = member.IsActive.Before == 
                    member.IsActive.After ? bIsActive :
                    $"{bIsActive} → {aIsActive}";

                sb.AppendLine($"- ({isActive}) (`{role}`) <@{member.DiscordId}>");
            }

            eb.WithDescription(sb.ToString());
            
            await FollowupAsync(embed: eb.Build());
        }
        catch (Exception ex)
        {
            await FollowupAsync("The request timed out. Please try again later.");
            _logger.LogError(ex, "Command Error");
        }
    }

    // edit start

    [SlashCommand("role", "Установить роль участника события.")]
    public async Task RoleAsync(SocketUser target, DiscordMemberRole role, int? eventId = null)
    {
        await DeferAsync(ephemeral: true);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        
        try
        {
            var request = new EventEditRoleRequest(
                ExecutorDiscordId: Context.User.Id, 
                TargetDiscordId: target.Id,
                Role: (MemberRole)role,
                EventId: eventId);

            var result = await _mediator.Send(request, cts.Token);
            
            await result.Match(
                success => FollowupAsync("Роль успешно обновлена."),
                errors => FollowupAsync($"Ошибка: {errors.First().Description}")
            );
        }
        catch (Exception ex)
        {
            await FollowupAsync("The request timed out. Please try again later.");
            _logger.LogError(ex, "Command Error");
        }
    }

    // edit roles

    [SlashCommand("activity", "Установить статус активности участника события.")]
    public async Task ActivityAsync(SocketUser target, bool isActive, int? eventId = null)
    {
        await DeferAsync(ephemeral: true);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        
        try
        {
            var request = new EventEditActivityRequest(
                ExecutorDiscordId: Context.User.Id, 
                TargetDiscordId: target.Id,
                IsActive: isActive,
                EventId: eventId);

            var result = await _mediator.Send(request, cts.Token);
            
            await result.Match(
                success => FollowupAsync("Статус успешно обновлен."),
                errors => FollowupAsync($"Ошибка: {errors.First().Description}")
            );
        }
        catch (Exception ex)
        {
            await FollowupAsync("The request timed out. Please try again later.");
            _logger.LogError(ex, "Command Error");
        }
    }

    [SlashCommand("type", "Задать тип события.")]
    public async Task TypeAsync(
        [Autocomplete(typeof(EventTypeAutocomplete))] string title, 
        int? eventId = null)
    {
        await DeferAsync(ephemeral: true);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        
        try
        {
            var request = new EventEditTypeRequest(
                ExecutorDiscordId: Context.User.Id,
                EventTypeTitle: title,
                EventId: eventId);

            var result = await _mediator.Send(request, cts.Token);
            
            await result.Match(
                success => FollowupAsync("Тип события успешно обновлен."),
                errors => FollowupAsync($"Ошибка: {errors.First().Description}")
            );
        }
        catch (Exception ex)
        {
            await FollowupAsync("The request timed out. Please try again later.");
            _logger.LogError(ex, "Command Error");
        }
    }

    // edit activities
    // edit takecontrol
    // edit givecontrol
    // edit cancel
    // edit delete
    // edit end
}