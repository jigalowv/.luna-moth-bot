using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Luna.Application.EventEdit.Commands.Activities;
using Luna.Application.EventEdit.Commands.Activity;
using Luna.Application.EventEdit.Commands.Cancel;
using Luna.Application.EventEdit.Commands.Delete;
using Luna.Application.EventEdit.Commands.End;
using Luna.Application.EventEdit.Commands.GiveControl;
using Luna.Application.EventEdit.Commands.List;
using Luna.Application.EventEdit.Commands.Role;
using Luna.Application.EventEdit.Commands.Roles;
using Luna.Application.EventEdit.Commands.Show;
using Luna.Application.EventEdit.Commands.Start;
using Luna.Application.EventEdit.Commands.TakeControl;
using Luna.Application.EventEdit.Commands.Type;
using Luna.Domain.Entities;
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

            string title = 
                response.Value.EventTypeTitle.After is null ||
                response.Value.EventTypeTitle.Before == 
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
            
            foreach (var member in response.Value.Members.OrderByDescending(i => i.Role.Before))
            {
                string bActivity = member.IsActive?.Before == true ? "активный " : ""; 
                string bRole = member.Role.Before.GetName();
                string before = bActivity + bRole;
                
                bool isActiveAfter = member.IsActive?.After ?? member.IsActive?.Before ?? false;
                string aActivity = isActiveAfter ? "активный " : "";

                string aRole = member.Role.After?.GetName() ?? bRole;
                
                string after = aActivity + aRole; 

                string difference = after != before ? 
                    $"{before} → {after}" : 
                    before;

                sb.AppendLine($"- <@{member.DiscordId}>: {difference}");
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

    [SlashCommand("start", "Начать процесс редактирования.")]
    public async Task StartAsync(int eventId)
    {
        await DeferAsync(ephemeral: true);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        
        try
        {
            var request = new EventEditStartRequest(
                ExecutorDiscordId: Context.User.Id,
                EventId: eventId);

            var response = await _mediator.Send(request, cts.Token);
            
            await response.Match(
                success => FollowupAsync("Процесс редактирования начат."),
                errors => FollowupAsync($"Ошибка: {errors.First().Description}")
            );
        }
        catch (Exception ex)
        {
            await FollowupAsync("The request timed out. Please try again later.");
            _logger.LogError(ex, "Command Error");
        }
    }

    [SlashCommand("role", "Установить роль участника события.")]
    public async Task RoleAsync(
        SocketUser target, 
        DiscordMemberRole role, 
        int? eventId = null)
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

            var response = await _mediator.Send(request, cts.Token);
            
            await response.Match(
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

    [SlashCommand("roles", "Установить роли участников события.")]
    public async Task RolesAsync(
        [Summary(description: "format: id_1, id_2,..., id_n")]
        string targets, 
        DiscordMemberRole role, 
        int? eventId = null)
    {
        await DeferAsync(ephemeral: true);

        var parsedTargets = targets.ParseToUlongList();

        if (parsedTargets.Count == 0)
        {
            await FollowupAsync(
                "Ошибка: Не удалось распознать ни один Discord ID. Проверьте формат.");
            return;
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        
        try
        {
            var request = new EventEditRolesRequest(
                ExecutorDiscordId: Context.User.Id, 
                TargetsDiscordIds: parsedTargets,
                Role: (MemberRole)role,
                EventId: eventId);

            var response = await _mediator.Send(request, cts.Token);
            
            await response.Match(
                success => FollowupAsync("Роли успешно обновлены."),
                errors => FollowupAsync($"Ошибка: {errors.First().Description}")
            );
        }
        catch (Exception ex)
        {
            await FollowupAsync("The request timed out. Please try again later.");
            _logger.LogError(ex, "Command Error");
        }
    }

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

            var response = await _mediator.Send(request, cts.Token);
            
            await response.Match(
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

    [SlashCommand("activities", "Установить статусы активности участников события.")]
    public async Task ActivitiesAsync(
        [Summary(description: "format: id_1, id_2,..., id_n")]
        string targets, 
        bool isActive, 
        int? eventId = null)
    {
        await DeferAsync(ephemeral: true);

        var parsedTargets = targets.ParseToUlongList();

        if (parsedTargets.Count == 0)
        {
            await FollowupAsync(
                "Ошибка: Не удалось распознать ни один Discord ID. Проверьте формат.");
            return;
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        
        try
        {
            var request = new EventEditActivitiesRequest(
                ExecutorDiscordId: Context.User.Id, 
                TargetsDiscordIds: parsedTargets,
                IsActive: isActive,
                EventId: eventId);

            var response = await _mediator.Send(request, cts.Token);
            
            await response.Match(
                success => FollowupAsync("статусы активности успешно обновлены."),
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

            var response = await _mediator.Send(request, cts.Token);
            
            await response.Match(
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

    [SlashCommand("takecontrol", "Получить контроль над процессом редактирования.")]
    public async Task TakeControlAsync(int eventId)
    {
        await DeferAsync(ephemeral: true);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        
        try
        {
            var request = new EventEditTakeControlRequest(
                ExecutorDiscordId: Context.User.Id,
                EventId: eventId);

            var response = await _mediator.Send(request, cts.Token);
            
            await response.Match(
                success => FollowupAsync("Контроль успешно получен."),
                errors => FollowupAsync($"Ошибка: {errors.First().Description}")
            );
        }
        catch (Exception ex)
        {
            await FollowupAsync("The request timed out. Please try again later.");
            _logger.LogError(ex, "Command Error");
        }
    }

    [SlashCommand("givecontrol", "Дать контроль над процессом редактирования.")]
    public async Task GiveControlAsync(SocketUser target, int? eventId = null)
    {
        await DeferAsync(ephemeral: true);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        
        try
        {
            var request = new EventEditGiveControlRequest(
                ExecutorDiscordId: Context.User.Id,
                TargetDiscordId: target.Id,
                EventId: eventId);

            var response = await _mediator.Send(request, cts.Token);
            
            await response.Match(
                success => FollowupAsync("Пользователь успешно получил контроль.."),
                errors => FollowupAsync($"Ошибка: {errors.First().Description}")
            );
        }
        catch (Exception ex)
        {
            await FollowupAsync("The request timed out. Please try again later.");
            _logger.LogError(ex, "Command Error");
        }
    }

    [SlashCommand("cancel", "Отменить процесс редактирования.")]
    public async Task CancelAsync(string? endCode = null, int? eventId = null)
    {
        await DeferAsync(ephemeral: true);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        
        try
        {
            var request = new EventEditCancelRequest(
                ExecutorDiscordId: Context.User.Id,
                EndCode: endCode,
                EventId: eventId);

            var response = await _mediator.Send(request, cts.Token);
            
            if (response.IsError)
            {
                await FollowupAsync(
                    $"Ошибка: {response.Errors.First().Description}");
                return;
            }

            if (response.Value is not null)
            {
                await FollowupAsync(
                    $"Код ОТМЕНЫ: `{response.Value}`.");
                return;
            }

            await FollowupAsync("Процесс редактирования отменён.");
        }
        catch (Exception ex)
        {
            await FollowupAsync("The request timed out. Please try again later.");
            _logger.LogError(ex, "Command Error");
        }
    }
    
    [SlashCommand("delete", "Удалить событие")]
    public async Task DeleteAsync(string? endCode = null, int? eventId = null)
    {
        await DeferAsync(ephemeral: true);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        
        try
        {
            var request = new EventEditDeleteRequest(
                ExecutorDiscordId: Context.User.Id,
                EndCode: endCode,
                EventId: eventId);

            var response = await _mediator.Send(request, cts.Token);
            
            if (response.IsError)
            {
                await FollowupAsync(
                    $"Ошибка: {response.Errors.First().Description}");
                return;
            }

            if (response.Value is not null)
            {
                await FollowupAsync(
                    $"Код УДАЛЕНИЯ: `{response.Value}`.");
                return;
            }

            await FollowupAsync("Событие удалено.");
        }
        catch (Exception ex)
        {
            await FollowupAsync("The request timed out. Please try again later.");
            _logger.LogError(ex, "Command Error");
        }
        
    }

    [SlashCommand("end", "Завершить процесс редактирования.")]
    public async Task EndAsync(string? endCode = null, int? eventId = null)
    {
        await DeferAsync(ephemeral: true);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        
        try
        {
            var request = new EventEditEndRequest(
                ExecutorDiscordId: Context.User.Id,
                EndCode: endCode,
                EventId: eventId);

            var response = await _mediator.Send(request, cts.Token);
            
            if (response.IsError)
            {
                await FollowupAsync(
                    $"Ошибка: {response.Errors.First().Description}");
                return;
            }

            if (response.Value is not null)
            {
                await FollowupAsync(
                    $"Код ПОДТВЕРЖДЕНИЯ: `{response.Value}`.");
                return;
            }

            await FollowupAsync("Процесс редактирования завершён.");
        }
        catch (Exception ex)
        {
            await FollowupAsync("The request timed out. Please try again later.");
            _logger.LogError(ex, "Command Error");
        }
    }
}