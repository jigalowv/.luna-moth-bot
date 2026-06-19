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
using Luna.Presentation.Extensions;

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
        await DeferAsync(ephemeral: false);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        if (channel is null ||
            channel.ChannelType != ChannelType.Voice &&
            channel.ChannelType != ChannelType.Stage)
        {
            await FollowupAsync(embed: EmbedHelper.CreateError(
                "Выберите ГОЛОСОВОЙ канал или ТРИБУНУ.").Build());
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
                success => FollowupAsync(embed: EmbedHelper
                    .CreateUpdate("Запись начата.").Build()),
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

    [SlashCommand("cancel", "Отменить запись.")]
    public async Task CancelAsync(SocketVoiceChannel channel)
    {
        await DeferAsync(ephemeral: false);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        if (channel is null ||
            channel.ChannelType != ChannelType.Voice &&
            channel.ChannelType != ChannelType.Stage)
        {
            await FollowupAsync(embed: EmbedHelper.CreateError(
                "Выберите ГОЛОСОВОЙ канал или ТРИБУНУ.").Build());
            return;
        }

        try
        {
            var request = new RecordCancelRequest(
                ExecutorDiscordId: Context.User.Id,
                ChannelId: channel.Id);

            var response = await _mediator.Send(request, cts.Token);

            if (response.IsError)
            {
                await FollowupAsync(embed: EmbedHelper.CreateError(
                    $"{response.Errors.First().Description}").Build());
                return;
            }

            var deafenDurations = response.Value.UserDeafenDurations;
            var notDeafenDurations = response.Value.UserNotDeafenDurations;

            var allKeys = notDeafenDurations.Keys.Union(deafenDurations.Keys);

            var sb = new StringBuilder();

            foreach (var key in allKeys)
            {
                var notDeafenTime = notDeafenDurations
                    .GetValueOrDefault(key, TimeSpan.Zero);
                
                var deafenTime = deafenDurations
                    .GetValueOrDefault(key, TimeSpan.Zero);

                sb.Append($"- {notDeafenTime.ToString(@"h\:mm\:ss")}")
                    .Append($" (:mute:: {deafenTime.ToString(@"h\:mm\:ss")})")
                    .Append($": <@{key}>")
                    .AppendLine();
            }
            
            await FollowupAsync(embed: EmbedHelper.CreateUpdate(
                $"""
                Запись **отменена**.

                ### Участники ({channel.Mention}):
                {sb}
                """
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

    [SlashCommand("users", "Вывести список записанных пользователей.")]
    public async Task UsersAsync(SocketVoiceChannel? channel = null)
    {
        await DeferAsync(ephemeral: false);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        if (channel is not null &&
            channel.ChannelType != ChannelType.Voice &&
            channel.ChannelType != ChannelType.Stage)
        {
            await FollowupAsync(embed: EmbedHelper
                .CreateError("Выберите ГОЛОСОВОЙ канал или ТРИБУНУ.").Build());
            await FollowupAsync(embed: EmbedHelper.CreateError("Выберите ГОЛОСОВОЙ канал или ТРИБУНУ.").Build());
            return;
        }

        try
        {
            var request = new RecordUsersRequest(
                ExecutorDiscordId: Context.User.Id,
                ChannelId: channel?.Id);

            var response = await _mediator.Send(request, cts.Token);
            
            if (response.IsError)
            {
                await FollowupAsync(embed: EmbedHelper.CreateError(
                    $"{response.Errors.First().Description}").Build());
                return;
            }

            var deafenDurations = response.Value.UserDeafenDurations;
            var notDeafenDurations = response.Value.UserNotDeafenDurations;

            var allKeys = notDeafenDurations.Keys.Union(deafenDurations.Keys);

            var sb = new StringBuilder();

            foreach (var key in allKeys)
            {
                var notDeafenTime = notDeafenDurations.GetValueOrDefault(key, TimeSpan.Zero);
                var deafenTime = deafenDurations.GetValueOrDefault(key, TimeSpan.Zero);

                sb.Append($"- {notDeafenTime.ToString(@"h\:mm\:ss")}")
                    .Append($" (:mute:: {deafenTime.ToString(@"h\:mm\:ss")})")
                    .Append($": <@{key}>")
                    .AppendLine();
            }

            await FollowupAsync(embed: EmbedHelper.CreateBaseWithTitle(
                title: $"Участники (<#{response.Value.ChannelId}>)",
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

    [SlashCommand("list", "Вывести список записей.")]
    public async Task ListAsync()
    {
        await DeferAsync(ephemeral: false);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        try
        {
            var sb = new StringBuilder();

            var request = new RecordListRequest(Context.User.Id);
            
            var response = await _mediator.Send(request);

            if (response.IsError)
            {
                await FollowupAsync(embed: EmbedHelper
                    .CreateError($"{response.Errors.First().Description}")
                    .Build());
                return;
            }

            if (response.Value.Items.Count == 0)
            {
                await FollowupAsync(embed: EmbedHelper
                    .CreateError("Записей событий не найдено.").Build());
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

            await FollowupAsync(embed: EmbedHelper.CreateBaseWithTitle(
                title: "Записи",
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

    [SlashCommand("move", "Назначить другой канал для записи.")]
    public async Task MoveAsync(
        [Summary("old-channel-id")] string oldChannelIdStr, 
        SocketVoiceChannel newChannel)
    {
        await DeferAsync(ephemeral: false);

        bool success = ulong
            .TryParse(oldChannelIdStr, out ulong oldChannelId);
        
        if (!success)
        {
            await FollowupAsync(embed: EmbedHelper.CreateError(
                "Неправильный формат для old-channel-id").Build());
            return;
        }
        
        if (newChannel is null ||
            newChannel.ChannelType != ChannelType.Voice &&
            newChannel.ChannelType != ChannelType.Stage)
        {
            await FollowupAsync(embed: EmbedHelper.CreateError(
                "В качестве нового канала выберите " + 
                "ГОЛОСОВОЙ канал или ТРИБУНУ.").Build());
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
                success => FollowupAsync(embed: EmbedHelper
                    .CreateUpdate("Запись была перемещена:" + 
                        $" <#{oldChannelIdStr}> → {newChannel.Mention}")
                    .Build()),
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

    [SlashCommand("end", "Закончить запись с дальнейшим созданием события в репозитории.")]
    public async Task EndAsync(
        SocketVoiceChannel channel,
        [Summary("event-type", "Тип события (выберите из списка)")] 
        [Autocomplete(typeof(EventTypeAutocomplete))] string eventTypeTitle,

        [Summary("role", "Роль участников")] AllowedEndRoles? role = null,
        [Summary("min-duration", "Минимальная длительность")] int? minDuration = null)
    {
        await DeferAsync(ephemeral: false);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        if (channel is null ||
            channel.ChannelType != ChannelType.Voice &&
            channel.ChannelType != ChannelType.Stage)
        {
            await FollowupAsync(embed: EmbedHelper
                .CreateError("Выберите ГОЛОСОВОЙ канал или ТРИБУНУ.").Build());
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
                success => FollowupAsync(embed: EmbedHelper
                .CreateUpdate("""
                - Запись успешно завершена. 
                - Событие создано.
                """).Build()),
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