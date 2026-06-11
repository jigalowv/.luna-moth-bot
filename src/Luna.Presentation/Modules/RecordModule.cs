using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Luna.Application.Record.Commands.Cancel;
using Luna.Application.Record.Commands.Users;
using Luna.Application.Record.Commands.Start;
using MediatR;
using Luna.Application.Record.Commands.List;
using Luna.Application.Record.Commands.Move;
using Luna.Application.Record.Commands.End;
using Luna.Domain.Enums;
using Luna.Presentation.Enums;

namespace Luna.Presentation.Modules;

[Group("record", "Группа команд для записи события.")]
public sealed class RecordModule
    : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<RecordModule> _logger;
    private readonly IMediator _mediator;

    public RecordModule(
        ILogger<RecordModule> logger,
        IMediator mediator
    )
    {
        _logger = logger;
        _mediator = mediator;
    }

    [SlashCommand("start", "Начать запись.")]
    public async Task StartAsync(SocketVoiceChannel channel)
    {
        await DeferAsync(ephemeral: true);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        if (channel is null ||
            channel.ChannelType != ChannelType.Voice &&
            channel.ChannelType != ChannelType.Stage)
        {
            await FollowupAsync("Выберите ГОЛОСОВОЙ канал или ТРИБУНУ.");
            return;
        }

        try
        {
            Dictionary<ulong, bool> idAndIsDeafenPairs = [];

            foreach (var user in channel.ConnectedUsers.Where(i => !i.IsBot))
            {
                idAndIsDeafenPairs[user.Id] = user.IsDeafened;
            }

            var request = new RecordStartRequest(
                ExecutorDiscordId: Context.User.Id,
                IdAndIsDeafenPairs: idAndIsDeafenPairs,
                ChannelId: channel.Id
            );

            var response = await _mediator.Send(request, cts.Token);
            
            await response.Match(
                success => FollowupAsync("Запись начата."),
                errors => FollowupAsync($"Ошибка: {errors.First().Description}")
            );
        }
        catch (Exception ex)
        {
            await FollowupAsync("The request timed out. Please try again later.");
            _logger.LogError(ex, "Command Error");
        }
    }

    [SlashCommand("cancel", "Отменить запись.")]
    public async Task CancelAsync(SocketVoiceChannel channel)
    {
        await DeferAsync(ephemeral: true);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        if (channel is null ||
            channel.ChannelType != ChannelType.Voice &&
            channel.ChannelType != ChannelType.Stage)
        {
            await FollowupAsync("Выберите ГОЛОСОВОЙ канал или ТРИБУНУ.");
            return;
        }

        try
        {
            var request = new RecordCancelRequest(
                ExecutorDiscordId: Context.User.Id,
                ChannelId: channel.Id);
            
            var eb = new EmbedBuilder()
                .WithTitle($"Участники ({channel.Mention})")
                .WithCurrentTimestamp();

            var response = await _mediator.Send(request, cts.Token);

            var sb = new StringBuilder();

            if (response.IsSuccess)
            {
                var deafenDurations = response.Value.UserDeafenDurations;
                var notDeafenDurations = response.Value.UserNotDeafenDurations;

                var allKeys = notDeafenDurations.Keys.Union(deafenDurations.Keys);

                foreach (var key in allKeys)
                {
                    var notDeafenTime = notDeafenDurations.GetValueOrDefault(key, TimeSpan.Zero);
                    var deafenTime = deafenDurations.GetValueOrDefault(key, TimeSpan.Zero);

                    sb.Append($"- {notDeafenTime.ToString(@"h\:mm\:ss")}")
                      .Append($" (:mute:: {deafenTime.ToString(@"h\:mm\:ss")})")
                      .Append($": <@{key}>")
                      .AppendLine();
                }

                eb.WithDescription(sb.ToString())
                  .WithColor(Color.Blue);
            }

            await response.Match(
                success => FollowupAsync(embed: eb.Build()),
                errors => FollowupAsync($"Ошибка: {errors.First().Description}")
            );
        }
        catch (Exception ex)
        {
            await FollowupAsync("The request timed out. Please try again later.");
            _logger.LogError(ex, "Command Error");
        }
    }

    [SlashCommand("users", "Вывести список записанных пользователей.")]
    public async Task UsersAsync(SocketVoiceChannel? channel = null)
    {
        await DeferAsync(ephemeral: true);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        if (channel is not null &&
            channel.ChannelType != ChannelType.Voice &&
            channel.ChannelType != ChannelType.Stage)
        {
            await FollowupAsync("Select a VOICE or STAGE channel.");
            return;
        }

        try
        {
            var request = new RecordUsersRequest(
                ExecutorDiscordId: Context.User.Id,
                ChannelId: channel?.Id);

            var result = await _mediator.Send(request, cts.Token);
            
            var sb = new StringBuilder();
            var eb = new EmbedBuilder()
                .WithCurrentTimestamp();

            if (result.IsSuccess)
            {   
                eb.WithTitle($"Участники (<#{result.Value.ChannelId}>)");

                var deafenDurations = result.Value.UserDeafenDurations;
                var notDeafenDurations = result.Value.UserNotDeafenDurations;

                var allKeys = notDeafenDurations.Keys.Union(deafenDurations.Keys);

                foreach (var key in allKeys)
                {
                    var notDeafenTime = notDeafenDurations.GetValueOrDefault(key, TimeSpan.Zero);
                    var deafenTime = deafenDurations.GetValueOrDefault(key, TimeSpan.Zero);

                    sb.Append($"- {notDeafenTime.ToString(@"h\:mm\:ss")}")
                      .Append($" (:mute:: {deafenTime.ToString(@"h\:mm\:ss")})")
                      .Append($": <@{key}>")
                      .AppendLine();
                }

                eb.WithDescription(sb.ToString())
                  .WithColor(Color.Blue);
            }

            await result.Match(
                success => FollowupAsync(embed: eb.Build()),
                errors => FollowupAsync($"Ошибка: {errors.First().Description}")
            );
        }
        catch (Exception ex)
        {
            await FollowupAsync("The request timed out. Please try again later.");
            _logger.LogError(ex, "Command Error");
        }
    }

    [SlashCommand("list", "Вывести список записей.")]
    public async Task ListAsync()
    {
        await DeferAsync(ephemeral: true);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        try
        {
            var sb = new StringBuilder();

            var request = new RecordListRequest(Context.User.Id);
            
            var response = await _mediator.Send(request);

            if (response.IsError)
            {
                await FollowupAsync(
                    $"{response.Errors.First().Description}");
                return;
            }

            if (response.Value.Items.Count == 0)
            {
                await FollowupAsync($"Возвращено 0 записей.");
                return;
            }

            foreach (var item in response.Value.Items)
            {
                TimeSpan ts = DateTime.UtcNow - item.StartAt;
                string duration = $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
                string own = item.ExecutorDiscordId == Context.User.Id ? 
                    "**(ваша)**" : 
                    $"<@{item.ExecutorDiscordId}> ({item.ExecutorDiscordId})";
                sb.AppendLine($"- (`{duration}`): <#{item.ChannelId}> (`{item.ChannelId}`): {own}");
            }

            var eb = new EmbedBuilder()
                .WithTitle("Записи")
                .WithColor(Color.Blue)
                .WithDescription(sb.ToString())
                .WithCurrentTimestamp();

            await FollowupAsync(embed: eb.Build());
        }
        catch (Exception ex)
        {
            await FollowupAsync("The request timed out. Please try again later.");
            _logger.LogError(ex, "Command Error");
        }
    }

    [SlashCommand("move", "Назначить другой канал для записи.")]
    public async Task MoveAsync(
        [Summary("old-channel-id")] string oldChannelIdStr, 
        SocketVoiceChannel newChannel)
    {
        await DeferAsync(ephemeral: true);

        bool success = ulong
            .TryParse(oldChannelIdStr, out ulong oldChannelId);
        
        if (!success)
            await FollowupAsync(
                "Неправильный формат для old-channel-id");
        
        if (newChannel is null ||
            newChannel.ChannelType != ChannelType.Voice &&
            newChannel.ChannelType != ChannelType.Stage)
        {
            await FollowupAsync(
                "В качестве нового канала выберите " + 
                "ГОЛОСОВОЙ канал или ТРИБУНУ.");
            return;
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        try
        {
            Dictionary<ulong, bool> idAndIsDeafenPairs = [];

            foreach (var user in newChannel.ConnectedUsers.Where(i => !i.IsBot))
            {
                idAndIsDeafenPairs[user.Id] = user.IsDeafened;
            }

            var request = new RecordMoveRequest(
                ExecutorDiscordId: Context.User.Id,
                IdAndIsDeafenPairs: idAndIsDeafenPairs,
                OldChannelId: oldChannelId,
                NewChannelId: newChannel.Id
            );
            
            var response = await _mediator.Send(request, cts.Token);

            await response.Match(
                success => FollowupAsync("Запись изменена."),
                errors => FollowupAsync($"Ошибка: {errors.First().Description}")
            );
        }
        catch (Exception ex)
        {
            await FollowupAsync("The request timed out. Please try again later.");
            _logger.LogError(ex, "Command Error");
        }
    }

    [SlashCommand("end", "Закончить запись с дальнейшим созданием события в репозитории.")]
    public async Task EndAsync(
        SocketVoiceChannel channel,
        [Summary("event-type", "Тип события (выберите из списка)")] 
        [Autocomplete(typeof(EventTypeAutocomplete))] string eventTypeTitle,

        [Summary("role", "Роль участников")] AllowedEndRoles? role = null,
        [Summary("min-duration", "Минимальная длительность")] int? minDuration = null)
    {
        await DeferAsync(ephemeral: true);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        if (channel is null ||
            channel.ChannelType != ChannelType.Voice &&
            channel.ChannelType != ChannelType.Stage)
        {
            await FollowupAsync("Select a VOICE or STAGE channel.");
            return;
        }

        try
        {
            var request = new RecordEndRequest(
                ExecutorDiscordId: Context.User.Id,
                ChannelId: channel.Id,
                EventTypeTitle: eventTypeTitle,
                Role: (MemberRole)(role ?? AllowedEndRoles.Player),
                MinDuration: minDuration
            );

            var response = await _mediator.Send(request, cts.Token);

            await response.Match(
                success => FollowupAsync("Запись успешно завершена и событие создано."),
                errors => FollowupAsync($"Ошибка: {errors.First().Description}")
            );
        }
        catch (Exception ex)
        {
            await FollowupAsync("Время ожидания запроса истекло. Попробуйте позже.");
            _logger.LogError(ex, "Ошибка при выполнении команды /end");
        }
    }
}