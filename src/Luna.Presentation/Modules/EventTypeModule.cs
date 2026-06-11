using System.Text;
using Discord;
using Discord.Interactions;
using Luna.Application.EventTypes.Commands.Add;
using Luna.Application.EventTypes.Commands.List;
using Luna.Application.EventTypes.Commands.Remove;
using Luna.Application.EventTypes.Commands.SetTitle;
using Luna.Application.EventTypes.Queries.EventTypeSearch;
using MediatR;

namespace Luna.Presentation.Modules;

[Group("eventtypes", "Группа команд для управления типами событий.")]
public sealed class EventTypeModule 
    : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<PingModule> _logger;
    private readonly IMediator _mediator;

    public EventTypeModule(
        IMediator mediator,
        ILogger<PingModule> logger)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [SlashCommand("remove", "Удалить событие из репозитория.")]
    public async Task RemoveAsync(string title)
    {
        await DeferAsync(ephemeral: true);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        
        try
        {
            var request = new EventTypeRemoveRequest(
                ExecutorDiscordId: Context.User.Id,
                Title: title
            );

            var response = await _mediator.Send(request, cts.Token);
            
            await response.Match(
                success => FollowupAsync("Событие удалено."),
                errors => FollowupAsync($"Ошибка: {errors.First().Description}")
            ); 
        }
        catch (Exception ex)
        {
            await FollowupAsync("The request timed out. Please try again later.");
            _logger.LogError(ex, "Command Error");
        }
    }

    [SlashCommand("list", "Вывести типы событий.")]
    public async Task ListAsync()
    {
        await DeferAsync(ephemeral: true);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        
        try
        {
            var request = new EventTypeListRequest(
                ExecutorDiscordId: Context.User.Id
            );

            var response = await _mediator.Send(request, cts.Token);

            if (response.IsError)
            {
                await FollowupAsync(
                    $"Ошибка: {response.Errors.First().Description}");
                return;
            }

            if (response.Value.Titles.Count == 0)
            {
                await FollowupAsync($"Возвращено 0 типов событий.");
                return;
            }

            var sb = new StringBuilder();

            foreach (var title in response.Value.Titles)
            {
                sb.AppendLine($"- {title}");
            }

            var eb = new EmbedBuilder()
                .WithDescription(sb.ToString())
                .WithTitle("Типы событий")
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
    
    [SlashCommand("add", "Добавить тип события.")]
    public async Task AddAsync([MaxLength(50)]string title)
    {
        await DeferAsync(ephemeral: true);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        try
        {
            var request = new EventTypeAddRequest(
                ExecutorDiscordId: Context.User.Id,
                Title: title
            );

            var response = await _mediator.Send(request, cts.Token);
            
            await response.Match(
                success => FollowupAsync("Событие добавлено."),
                errors => FollowupAsync($"Ошибка: {errors.First().Description}")
            ); 
        }
        catch (Exception ex)
        {
            await FollowupAsync("The request timed out. Please try again later.");
            _logger.LogError(ex, "Command Error");
        }
    }

    [SlashCommand("settitle", "Задать новое название для типа события.")]
    public async Task SetTitleAsync(
        [Autocomplete(typeof(EventTypeAutocomplete))]string oldTitle,
        string newTitle)
    {
        await DeferAsync(ephemeral: true);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        try
        {
            var request = new EventTypeSetTitleRequest(
                ExecutorDiscordId: Context.User.Id,
                oldTitle,
                newTitle
            );

            var response = await _mediator.Send(request, cts.Token);

            await response.Match(
                success => FollowupAsync("Событие переименовано."),
                errors => FollowupAsync($"Ошибка: {errors.First().Description}")
            );
        }
        catch (Exception ex)
        {
            await FollowupAsync("The request timed out. Please try again later.");
            _logger.LogError(ex, "Command Error");
        }
    }
}