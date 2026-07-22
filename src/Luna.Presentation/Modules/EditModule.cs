using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using Luna.Application.Auth.ExecutorHasAccess;
using Luna.Domain.Enums;
using Luna.Presentation.Extensions;
using Luna.Presentation.Modules.Modals;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace Luna.Presentation.Modules;

public record EditMessageContext(
    ulong GuildId,
    ulong ChannelId,
    ulong MessageId
);

[Group("edit", "Команды для редактирования сообщений.")]
public class EditModule : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly Regex MessageUrlRegex = new(
        @"^https://(?:canary\.|ptb\.)?discord\.com/channels/(\d+)/(\d+)/(\d+)$",
        RegexOptions.Compiled);

    private static readonly Regex TimestampRegex = new(
        @"^<t:\d+:[a-zA-Z]>$",
        RegexOptions.Compiled);
    
    private readonly ILogger<EditModule> _logger;
    private readonly IMediator _mediator;
    private readonly IMemoryCache _cache;

    public EditModule(
        ILogger<EditModule> logger,
        IMediator mediator,
        IMemoryCache cache)
    {
        _logger = logger;
        _mediator = mediator;
        _cache = cache;
    }
    
    [SlashCommand("raw", "Редактирование сообщения.")]
    public async Task EditRawAsync(string messageUrl)
    {
        try
        {
            var request = new AuthExecutorHasAccessRequest(
                ExecutorDiscordId: Context.User.Id,
                MinRole: ExecutorRole.Curator
            );

            var response = await _mediator.Send(request);
            
            if (response.IsError)
            {
                var errorMsg = response.Errors.First().Description;
                var errorEmbed = EmbedHelper.CreateError(errorMsg).Build();

                await RespondAsync(embed: errorEmbed, ephemeral: true);
                return;
            }

            var match = MessageUrlRegex.Match(messageUrl.Trim());
            if (!match.Success)
            {
                await RespondAsync(
                    "Указана неверная ссылка на сообщение.", 
                    ephemeral: true);
                return;
            }

            var guildId = ulong.Parse(match.Groups[1].Value);
            var channelId = ulong.Parse(match.Groups[2].Value);
            var messageId = ulong.Parse(match.Groups[3].Value);

            if (Context.Guild.Id != guildId)
            {
                await RespondAsync(
                    "Сообщение находится на другом сервере.", 
                    ephemeral: true);
                return;
            }

            var channel = Context.Guild.GetTextChannel(channelId);
            if (channel == null)
            {
                await RespondAsync(
                    "Текстовый канал не найден.", 
                    ephemeral: true);
                return;
            }

            var message = await channel.GetMessageAsync(messageId) 
                as IUserMessage;

            if (message == null)
            {
                await RespondAsync(
                    "Сообщение не найдено.", 
                    ephemeral: true);
                return;
            }

            var cacheKey = Guid.NewGuid().ToString("N")[..12];
            var contextData = new EditMessageContext(
                guildId, 
                channelId, 
                messageId);

            _cache.Set(
                $"edit_raw:{cacheKey}", 
                contextData, 
                TimeSpan.FromMinutes(15));

            var modalBuilder = new ModalBuilder()
                .WithTitle("Редактирование поста")
                .WithCustomId($"raw_sub:{cacheKey}")
                .AddTextInput(
                    label: "Содержание поста",
                    customId: "edit_raw_content",
                    style: TextInputStyle.Paragraph,
                    value: message.Content,
                    required: true);

            await RespondWithModalAsync(modalBuilder.Build());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при открытии модального окна raw");

            if (!Context.Interaction.HasResponded)
            {
                await RespondAsync(
                    "Произошла ошибка при обработке команды.", 
                    ephemeral: true);
            }
        }
    }

    [ModalInteraction("raw_sub:*", ignoreGroupNames: true)]
    public async Task HandleEditRawSubmitAsync(
        string cacheKey, 
        Modals.EditRawModal modal)
    {
        await DeferAsync(ephemeral: true);

        if (!_cache.TryGetValue<EditMessageContext>(
                $"edit_raw:{cacheKey}", 
                out var contextData) || contextData == null)
        {
            await FollowupAsync(
                "Срок действия формы истек. Запросите форму заново.", 
                ephemeral: true);
            return;
        }

        _cache.Remove($"edit_raw:{cacheKey}");

        var channel = Context.Guild.GetTextChannel(contextData.ChannelId);
        if (channel == null)
        {
            await FollowupAsync("Канал не найден.", ephemeral: true);
            return;
        }

        var message = await channel.GetMessageAsync(contextData.MessageId) 
            as IUserMessage;

        if (message == null)
        {
            await FollowupAsync("Сообщение не найдено.", ephemeral: true);
            return;
        }

        var existingEmbeds = message.Embeds
            .Select(e => e.ToEmbedBuilder().Build())
            .ToArray();

        await message.ModifyAsync(msg =>
        {
            msg.Content = modal.Content;
            msg.Embeds = existingEmbeds;
        });

        var successEmbed = EmbedHelper.CreateBaseWithTitle(
            "Успех", 
            "Сообщение было успешно отредактировано!"
        ).Build();

        await FollowupAsync(embed: successEmbed, ephemeral: true);
    }

    [SlashCommand("embed", "Редактирование embed-сообщения.")]
    public async Task EditEmbedAsync(string messageUrl)
    {
        try
        {
            var request = new AuthExecutorHasAccessRequest(
                ExecutorDiscordId: Context.User.Id,
                MinRole: ExecutorRole.Curator
            );

            var response = await _mediator.Send(request);
            
            if (response.IsError)
            {
                var errorMsg = response.Errors.First().Description;
                var errorEmbed = EmbedHelper.CreateError(errorMsg).Build();

                await RespondAsync(embed: errorEmbed, ephemeral: true);
                return;
            }

            var match = MessageUrlRegex.Match(messageUrl.Trim());
            if (!match.Success)
            {
                await RespondAsync(
                    "Указана неверная ссылка на сообщение.", 
                    ephemeral: true);
                return;
            }

            var guildId = ulong.Parse(match.Groups[1].Value);
            var channelId = ulong.Parse(match.Groups[2].Value);
            var messageId = ulong.Parse(match.Groups[3].Value);

            if (Context.Guild.Id != guildId)
            {
                await RespondAsync(
                    "Сообщение находится на другом сервере.", 
                    ephemeral: true);
                return;
            }

            var channel = Context.Guild.GetTextChannel(channelId);
            if (channel == null)
            {
                await RespondAsync(
                    "Текстовый канал не найден.", 
                    ephemeral: true);
                return;
            }

            var message = await channel.GetMessageAsync(messageId) 
                as IUserMessage;

            if (message == null)
            {
                await RespondAsync(
                    "Сообщение не найдено.", 
                    ephemeral: true);
                return;
            }

            var existingEmbed = message.Embeds.FirstOrDefault();
            if (existingEmbed == null)
            {
                await RespondAsync(
                    "У этого сообщения нет embed для редактирования.", 
                    ephemeral: true);
                return;
            }

            var cacheKey = Guid.NewGuid().ToString("N")[..12];
            var contextData = new EditMessageContext(
                guildId, 
                channelId, 
                messageId);

            _cache.Set(
                $"edit_embed:{cacheKey}", 
                contextData, 
                TimeSpan.FromMinutes(15));

            var modalBuilder = new ModalBuilder()
                .WithTitle("Редактирование embed-сообщения")
                .WithCustomId($"embed_sub:{cacheKey}")
                .AddTextInput(
                    label: "Содержание поста",
                    customId: "edit_embed_content",
                    style: TextInputStyle.Paragraph,
                    value: existingEmbed.Description,
                    required: true)
                .AddTextInput(
                    label: "Ссылка на изображение",
                    customId: "edit_embed_image",
                    style: TextInputStyle.Short,
                    placeholder: "https://...",
                    value: existingEmbed.Image?.Url,
                    required: false);

            await RespondWithModalAsync(modalBuilder.Build());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при открытии модального окна embed");

            if (!Context.Interaction.HasResponded)
            {
                await RespondAsync(
                    "Произошла ошибка при обработке команды.", 
                    ephemeral: true);
            }
        }
    }

    [ModalInteraction("embed_sub:*", ignoreGroupNames: true)]
    public async Task HandleEditEmbedSubmitAsync(
        string cacheKey, 
        Modals.EditEmbedModal modal)
    {
        await DeferAsync(ephemeral: true);

        if (!_cache.TryGetValue<EditMessageContext>(
                $"edit_embed:{cacheKey}", 
                out var contextData) || contextData == null)
        {
            await FollowupAsync(
                "Срок действия формы истек. Запросите форму заново.", 
                ephemeral: true);
            return;
        }

        _cache.Remove($"edit_embed:{cacheKey}");

        var channel = Context.Guild.GetTextChannel(contextData.ChannelId);
        if (channel == null)
        {
            await FollowupAsync("Канал не найден.", ephemeral: true);
            return;
        }

        var message = await channel.GetMessageAsync(contextData.MessageId) 
            as IUserMessage;

        if (message == null)
        {
            await FollowupAsync("Сообщение не найдено.", ephemeral: true);
            return;
        }

        var targetEmbed = message.Embeds.FirstOrDefault();
        if (targetEmbed == null)
        {
            await FollowupAsync("В сообщении отсутствует embed.", ephemeral: true);
            return;
        }

        var embedBuilder = targetEmbed.ToEmbedBuilder();

        embedBuilder.WithDescription(modal.Content);

        if (!string.IsNullOrWhiteSpace(modal.ImageUrl) && 
            Uri.IsWellFormedUriString(modal.ImageUrl, UriKind.Absolute))
        {
            embedBuilder.WithImageUrl(modal.ImageUrl);
        }
        else if (string.IsNullOrWhiteSpace(modal.ImageUrl))
        {
            embedBuilder.WithImageUrl(null);
        }

        var updatedEmbeds = message.Embeds
            .Select((e, index) => index == 0 
                ? embedBuilder.Build() 
                : e.ToEmbedBuilder().Build())
            .ToArray();

        await message.ModifyAsync(msg =>
        {
            msg.Embeds = updatedEmbeds;
        });

        var successEmbed = EmbedHelper.CreateBaseWithTitle(
            "Успех", 
            "Embed-сообщение было успешно отредактировано!"
        ).Build();

        await FollowupAsync(embed: successEmbed, ephemeral: true);
    }

    [SlashCommand("event-time", "Устанавливает время начала события в embed.")]
    public async Task SetEventTimeAsync(string messageUrl, string timestamp)
    {
        await DeferAsync(ephemeral: true);

        try
        {
            var request = new AuthExecutorHasAccessRequest(
                ExecutorDiscordId: Context.User.Id,
                MinRole: ExecutorRole.Curator
            );

            var response = await _mediator.Send(request);
            
            if (response.IsError)
            {
                var errorMsg = response.Errors.First().Description;
                var errorEmbed = EmbedHelper.CreateError(errorMsg).Build();

                await FollowupAsync(embed: errorEmbed, ephemeral: true);
                return;
            }

            var trimmedTimestamp = timestamp.Trim();
            if (!TimestampRegex.IsMatch(trimmedTimestamp))
            {
                await FollowupAsync(
                    "Неверный формат таймштампа. Пример: `<t:1700000000:f>`", 
                    ephemeral: true);
                return;
            }

            var match = MessageUrlRegex.Match(messageUrl.Trim());
            if (!match.Success)
            {
                await FollowupAsync(
                    "Указана неверная ссылка на сообщение.", 
                    ephemeral: true);
                return;
            }

            var guildId = ulong.Parse(match.Groups[1].Value);
            var channelId = ulong.Parse(match.Groups[2].Value);
            var messageId = ulong.Parse(match.Groups[3].Value);

            if (Context.Guild.Id != guildId)
            {
                await FollowupAsync(
                    "Сообщение находится на другом сервере.", 
                    ephemeral: true);
                return;
            }

            await ProcessEmbedModificationAsync(
                channelId, 
                messageId, 
                fieldName: "Начало", 
                newValue: trimmedTimestamp, 
                successMessage: $"Время начала успешно обновлено на {trimmedTimestamp}!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выполнении команды event-time");

            await FollowupAsync(
                "Произошла ошибка при обработке команды.", 
                ephemeral: true);
        }
    }

    [SlashCommand("event-link", "Обновляет ссылку на событие в embed.")]
    public async Task SetEventLinkAsync(string messageUrl, string eventUrl)
    {
        await DeferAsync(ephemeral: true);

        if (!await CheckAccessAsync()) return;

        var newValue = $"[Ссылка на событие]({eventUrl.Trim()})";

        await ModifyEventEmbedAsync(
            messageUrl, 
            fieldName: "\u200B", 
            newValue: newValue, 
            successMessage: "Ссылка на событие успешно обновлена!");
    }

    [SlashCommand("event-place", "Обновляет место проведения в embed.")]
    public async Task SetEventPlaceAsync(
        string messageUrl, 
        IChannel channel)
    {
        await DeferAsync(ephemeral: true);

        if (!await CheckAccessAsync()) return;

        var newValue = $"<#{channel.Id}>";

        await ModifyEventEmbedAsync(
            messageUrl, 
            fieldName: "Место", 
            newValue: newValue, 
            successMessage: "Место проведения успешно обновлено!");
    }

    [SlashCommand("event-title", "Обновляет тему события в embed.")]
    public async Task SetEventTitleAsync(string messageUrl, string topic)
    {
        await DeferAsync(ephemeral: true);

        if (!await CheckAccessAsync()) return;

        await ModifyEventEmbedAsync(
            messageUrl, 
            fieldName: "Тема", 
            newValue: topic.Trim(), 
            successMessage: "Тема события успешно обновлена!");
    }

    [SlashCommand("event-rules", "Обновляет правила проведения в embed.")]
    public async Task SetEventRulesAsync(string messageUrl, string rules)
    {
        await DeferAsync(ephemeral: true);

        if (!await CheckAccessAsync()) return;

        await ModifyEventEmbedAsync(
            messageUrl, 
            fieldName: "Правила", 
            newValue: rules.Trim(), 
            successMessage: "Правила события успешно обновлены!");
    }

    [SlashCommand("event-executors", "Открывает модалку выбора ведущих.")]
    public async Task PromptEventExecutorsAsync(string messageUrl)
    {
        try
        {
            var request = new AuthExecutorHasAccessRequest(
                ExecutorDiscordId: Context.User.Id,
                MinRole: ExecutorRole.Curator
            );

            var response = await _mediator.Send(request);
            
            if (response.IsError)
            {
                var errorEmbed = EmbedHelper.CreateError(
                    response.Errors.First().Description).Build();

                await RespondAsync(embed: errorEmbed, ephemeral: true);
                return;
            }

            var match = MessageUrlRegex.Match(messageUrl.Trim());
            if (!match.Success)
            {
                await RespondAsync(
                    embed: EmbedHelper.CreateError(
                        "Указана неверная ссылка на сообщение.").Build(), 
                    ephemeral: true);
                return;
            }

            var guildId = ulong.Parse(match.Groups[1].Value);
            var channelId = ulong.Parse(match.Groups[2].Value);
            var messageId = ulong.Parse(match.Groups[3].Value);

            if (Context.Guild.Id != guildId)
            {
                await RespondAsync(
                    embed: EmbedHelper.CreateError(
                        "Сообщение находится на другом сервере.").Build(), 
                    ephemeral: true);
                return;
            }

            var cacheKey = Guid.NewGuid().ToString("N")[..12];
            var contextData = new EditMessageContext(
                guildId, 
                channelId, 
                messageId);

            _cache.Set(
                $"exec_modal:{cacheKey}", 
                contextData, 
                TimeSpan.FromMinutes(15));

            await RespondWithModalAsync<ExecutorsModal>($"exec_sub:{cacheKey}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при открытии окна выбора ведущих");

            if (!Context.Interaction.HasResponded)
            {
                await RespondAsync(
                    embed: EmbedHelper.CreateError(
                        "Произошла ошибка при обработке команды.").Build(), 
                    ephemeral: true);
            }
        }
    }

    [ModalInteraction("exec_sub:*", ignoreGroupNames: true)]
    public async Task HandleExecutorsSubmitAsync(
        string cacheKey, 
        ExecutorsModal modal)
    {
        await DeferAsync(ephemeral: true);

        if (!_cache.TryGetValue<EditMessageContext>(
                $"exec_modal:{cacheKey}", 
                out var contextData) || contextData == null)
        {
            await FollowupAsync(
                embed: EmbedHelper.CreateError(
                    "Срок действия формы истек. Запросите форму заново.").Build(), 
                ephemeral: true);
            return;
        }

        _cache.Remove($"exec_modal:{cacheKey}");

        if (modal.Users == null || modal.Users.Length == 0)
        {
            await FollowupAsync(
                embed: EmbedHelper.CreateError(
                    "Вы не выбрали ни одного ведущего.").Build(), 
                ephemeral: true);
            return;
        }

        var leadersStr = string.Join(
            " ", 
            modal.Users.Select(u => $"<@{u.Id}>"));

        await ProcessEmbedModificationAsync(
            contextData.ChannelId, 
            contextData.MessageId, 
            fieldName: "Ведущие", 
            newValue: leadersStr, 
            successMessage: "Список ведущих успешно обновлен!");
    }

    private async Task<bool> CheckAccessAsync()
    {
        var request = new AuthExecutorHasAccessRequest(
            ExecutorDiscordId: Context.User.Id,
            MinRole: ExecutorRole.Curator
        );

        var response = await _mediator.Send(request);
        if (response.IsError)
        {
            var errorEmbed = EmbedHelper.CreateError(
                response.Errors.First().Description).Build();

            await FollowupAsync(embed: errorEmbed, ephemeral: true);
            return false;
        }

        return true;
    }

    private async Task ModifyEventEmbedAsync(
        string messageUrl, 
        string fieldName, 
        string newValue, 
        string successMessage)
    {
        var match = MessageUrlRegex.Match(messageUrl.Trim());
        if (!match.Success)
        {
            await FollowupAsync(
                embed: EmbedHelper.CreateError(
                    "Указана неверная ссылка на сообщение.").Build(), 
                ephemeral: true);
            return;
        }

        var guildId = ulong.Parse(match.Groups[1].Value);
        var channelId = ulong.Parse(match.Groups[2].Value);
        var messageId = ulong.Parse(match.Groups[3].Value);

        if (Context.Guild.Id != guildId)
        {
            await FollowupAsync(
                embed: EmbedHelper.CreateError(
                    "Сообщение находится на другом сервере.").Build(), 
                ephemeral: true);
            return;
        }

        await ProcessEmbedModificationAsync(
            channelId, 
            messageId, 
            fieldName, 
            newValue, 
            successMessage);
    }

    private async Task ProcessEmbedModificationAsync(
        ulong channelId, 
        ulong messageId, 
        string fieldName, 
        string newValue, 
        string successMessage)
    {
        try
        {
            var channel = Context.Guild.GetTextChannel(channelId);
            if (channel == null)
            {
                await FollowupAsync(
                    embed: EmbedHelper.CreateError(
                        "Текстовый канал не найден.").Build(), 
                    ephemeral: true);
                return;
            }

            var message = await channel.GetMessageAsync(messageId) 
                as IUserMessage;

            if (message == null)
            {
                await FollowupAsync(
                    embed: EmbedHelper.CreateError(
                        "Сообщение не найдено.").Build(), 
                    ephemeral: true);
                return;
            }

            var targetEmbed = message.Embeds.FirstOrDefault();
            if (targetEmbed == null)
            {
                await FollowupAsync(
                    embed: EmbedHelper.CreateError(
                        "В сообщении отсутствует embed.").Build(), 
                    ephemeral: true);
                return;
            }

            var fieldToUpdate = targetEmbed.Fields.FirstOrDefault(
                f => f.Name.Equals(
                    fieldName, 
                    StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(fieldToUpdate.Name))
            {
                await FollowupAsync(
                    embed: EmbedHelper.CreateError(
                        $"В embed отсутствует поле \"{fieldName}\".").Build(), 
                    ephemeral: true);
                return;
            }

            var embedBuilder = targetEmbed.ToEmbedBuilder();

            var builderField = embedBuilder.Fields.FirstOrDefault(
                f => f.Name.Equals(
                    fieldName, 
                    StringComparison.OrdinalIgnoreCase));

            if (builderField != null)
            {
                builderField.Value = newValue;
            }

            var updatedEmbeds = message.Embeds
                .Select((e, idx) => idx == 0 
                    ? embedBuilder.Build() 
                    : e.ToEmbedBuilder().Build())
                .ToArray();

            await message.ModifyAsync(msg =>
            {
                msg.Embeds = updatedEmbeds;
            });

            await FollowupAsync(
                embed: EmbedHelper.CreateUpdate(successMessage).Build(), 
                ephemeral: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при модификации поля {FieldName}", fieldName);

            await FollowupAsync(
                embed: EmbedHelper.CreateError(
                    "Произошла ошибка при обновлении сообщения.").Build(), 
                ephemeral: true);
        }
    }
}