using Discord;
using Discord.Interactions;
using Luna.Application.Auth.ExecutorHasAccess;
using Luna.Domain.Entities;
using Luna.Domain.Enums;
using Luna.Presentation.Extensions;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace Luna.Presentation.Modules;

[Group("post", "Группа для отправки сообщений.")]
public sealed class PostModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<PostModule> _logger;
    private readonly IMediator _mediator;
    private readonly IMemoryCache _cache;

    public PostModule(
        IMemoryCache cache,
        ILogger<PostModule> logger,
        IMediator mediator
    )
    {
        _cache = cache;
        _logger = logger;
        _mediator = mediator;
    }

    [SlashCommand("raw", "Открыть окно для создания обычного сообщения.")]
    public async Task OpenModalRawAsync()
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
                await RespondAsync(embed: EmbedHelper.CreateError(
                    $"{response.Errors.First().Description}").Build(),
                    ephemeral: true);
                return;
            }

            await RespondWithModalAsync<PostRawModal>($"post_raw_submit");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при открытии модального окна");

            if (!Context.Interaction.HasResponded)
            {
                await RespondAsync("Произошла ошибка при обработке команды.", ephemeral: true);
            }
        }
    }

    [ModalInteraction("post_raw_submit", ignoreGroupNames: true)]
    public async Task OnOpenPostForumModal(PostRawModal modal)
    {
        await DeferAsync(ephemeral: true);
        
        await Context.Channel.SendMessageAsync(modal.Content);

        await FollowupAsync(embed: EmbedHelper
            .CreateBaseWithTitle("Успех", "Вы запостили кринж!").Build());
    }

    [SlashCommand("embed", "Открыть окно для создания embed сообщения.")]
    public async Task OpenModalEmbedAsync()
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
                await RespondAsync(embed: EmbedHelper.CreateError(
                    $"{response.Errors.First().Description}").Build(),
                    ephemeral: true);
                return;
            }

            await RespondWithModalAsync<PostEmbedModal>($"post_embed_submit");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при открытии модального окна");

            if (!Context.Interaction.HasResponded)
            {
                await RespondAsync("Произошла ошибка при обработке команды.", ephemeral: true);
            }
        }
    }

    [ModalInteraction("post_embed_submit", ignoreGroupNames: true)]
    public async Task OnOpenPostForumModal(PostEmbedModal modal)
    {
        await DeferAsync(ephemeral: true);

        var eb = EmbedHelper.CreateBase(modal.Description, Color.Magenta)
            .WithImageUrl(modal.ImageUrl);
        
        await Context.Channel.SendMessageAsync(embed: eb.Build());

        await FollowupAsync(embed: EmbedHelper
            .CreateBaseWithTitle("Успех", "Вы запостили кринж!").Build());
    }

    [SlashCommand("editembed", "Открыть окно для изменения embed сообщения.")]
    public async Task OpenPostEditEmbedModal(string messageLink)
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
                await RespondAsync(embed: EmbedHelper.CreateError(
                    $"{response.Errors.First().Description}").Build(),
                    ephemeral: true);
                return;
            }

            string[] parts = messageLink.Split('/');

            if (parts.Length < 3 ||
                !ulong.TryParse(parts[^3], out var guildId) ||
                !ulong.TryParse(parts[^2], out var channelId) ||
                !ulong.TryParse(parts[^1], out var messageId))
            {
                await RespondAsync(embed: EmbedHelper
                    .CreateError("Неправильная ссылка")
                    .Build(), ephemeral: true);
                return;
            }

            var guild = Context.Client.GetGuild(guildId);
            if (guild == null)
            {
                await RespondAsync(embed: EmbedHelper
                    .CreateError("Сервер не найден.")
                    .Build(), ephemeral: true);
                return;
            }

            var channel = guild.GetTextChannel(channelId);
            if (channel == null)
            {
                await RespondAsync(embed: EmbedHelper.CreateError(
                        "Канал не найден или у бота нет к нему доступа.")
                    .Build(), ephemeral: true);
                return;
            }

            var message = await channel.GetMessageAsync(messageId);
            if (message == null || (message as IUserMessage) == null)
            {
                await RespondAsync(embed: EmbedHelper
                    .CreateError("Сообщение не найдено.")
                    .Build(), ephemeral: true);
                return;
            }

            var firstEmbed = message.Embeds.FirstOrDefault();

            var modal = new PostEmbedModal()
            {
                Description = firstEmbed?.Description ?? message.Content,
                ImageUrl = firstEmbed?.Image?.Url ?? ""
            };

            await RespondWithModalAsync(
                $"post_embed_edit_submit:{guildId}:{channelId}:{messageId}", 
                modal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при открытии модального окна");

            if (!Context.Interaction.HasResponded)
            {
                await RespondAsync("Произошла ошибка при обработке команды.", ephemeral: true);
            }
        }
    }

    [ModalInteraction("post_embed_edit_submit:*", ignoreGroupNames: true)]
    public async Task OnOpenModalPostEmbedEditAsync(string args, PostEmbedModal modal)
    {
        try
        {
            await DeferAsync(ephemeral: true);

            var ids = args.Split(':');
            if (ids.Length < 3)
            {
                await FollowupAsync(embed: EmbedHelper
                    .CreateError("Неверный формат данных взаимодействия.")
                    .Build(), ephemeral: true);
                return;
            }

            ulong guildId = ulong.Parse(ids[0]);
            ulong channelId = ulong.Parse(ids[1]);
            ulong messageId = ulong.Parse(ids[2]);

            var guild = Context.Client.GetGuild(guildId);
            if (guild == null)
            {
                await FollowupAsync(embed: EmbedHelper
                    .CreateError("Сервер не найден.")
                    .Build(), ephemeral: true);
                return;
            }

            var channel = guild.GetTextChannel(channelId);
            if (channel == null)
            {
                await FollowupAsync(embed: EmbedHelper
                    .CreateError("Канал не найден.")
                    .Build(), ephemeral: true);
                return;
            }

            var message = await channel.GetMessageAsync(messageId);
            if (message is not IUserMessage userMessage)
            {
                await FollowupAsync(embed: EmbedHelper
                    .CreateError("Сообщение не найдено или не может быть " + 
                    "отредактировано.").Build(), ephemeral: true);
                return;
            }

            var eb = EmbedHelper.CreateBase(modal.Description, Color.Magenta);
            
            if (!string.IsNullOrWhiteSpace(modal.ImageUrl))
            {
                eb.WithImageUrl(modal.ImageUrl);
            }

            await userMessage.ModifyAsync(properties =>
            {
                properties.Embed = eb.Build();
            });

            await FollowupAsync(embed: 
                EmbedHelper.CreateUpdate("Кринж успешно изменён!")
                .Build(), ephemeral: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при подтверждении модального окна для сообщения");
            
            // На всякий случай уведомляем пользователя, если что-то упало
            try 
            {
                await FollowupAsync(embed: EmbedHelper.CreateError($"Ошибка: {ex.Message}").Build(), ephemeral: true);
            } 
            catch { /* игнорируем, если даже фоллоуап не ушел */ }
        }
    }

    [SlashCommand("forum", "Открыть окно для создания поста для форума.")]
    public async Task OpenModalForumAsync()
    {
        try
        {
            var request = new AuthExecutorHasAccessRequest(
                ExecutorDiscordId: Context.User.Id,
                MinRole: ExecutorRole.Moderator
            );

            var response = await _mediator.Send(request);
            
            if (response.IsError)
            {
                await RespondAsync(embed: EmbedHelper.CreateError(
                    $"{response.Errors.First().Description}").Build(),
                    ephemeral: true);
                return;
            }

            await RespondWithModalAsync<PostForumModal>($"post_forum_submit");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при открытии модального окна");

            if (!Context.Interaction.HasResponded)
            {
                await RespondAsync("Произошла ошибка при обработке команды.", ephemeral: true);
            }
        }
    }

    [ModalInteraction("post_forum_submit", ignoreGroupNames: true)]
    public async Task OnOpenPostForumModal(PostForumModal modal)
    {
        await DeferAsync(ephemeral: true);

        var eb = EmbedHelper.CreateBase(modal.Description, Color.Magenta)
            .WithImageUrl(modal.ImageUrl);
        
        var forum = modal.Forums.First();
        await forum.CreatePostAsync(modal.ForumTitle, embed: eb.Build());

        await FollowupAsync(embed: EmbedHelper
            .CreateBaseWithTitle("Успех", "Вы запостили кринж!").Build());
    }

    [SlashCommand("event", "Открыть окно для ввода анонса")]
    public async Task OpenModalEventAsync(
        [Summary("ссылка-на-правила")]
        string? rulesUrl = null,
        [Summary("ссылка-на-событие-дискорд")]
        string? eventUrl = null)
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
                await RespondAsync(embed: EmbedHelper.CreateError(
                    $"{response.Errors.First().Description}").Build(),
                    ephemeral: true);
                return;
            }

            string cacheKey = Guid.NewGuid().ToString("N");

            var data = new PostData
            {
                RulesUrl = rulesUrl,
                EventUrl = eventUrl
            };

            _cache.Set(cacheKey, data, TimeSpan.FromMinutes(30));

            await RespondWithModalAsync<PostModal>($"post_event_submit_1:{cacheKey}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при открытии модального окна");

            if (!Context.Interaction.HasResponded)
            {
                await RespondAsync("Произошла ошибка при обработке команды.", ephemeral: true);
            }
        }
    }

    [ModalInteraction("post_event_submit_1:*", ignoreGroupNames: true)]
    public async Task OnOpenPostEventModal(string cacheKey, PostModal modal)
    {   
        await DeferAsync(ephemeral: true);

        if (!_cache.TryGetValue(cacheKey, out PostData? postData) ||
            postData is null)
        {
            await FollowupAsync(embed: EmbedHelper
                .CreateError("Сессия истекла, попробуйте ещё раз.").Build());
            return;
        }

        postData.Description = modal.Description;
        postData.Topic = modal.Topic;
        postData.PlaceId = modal.Place.First().Id;
        postData.StartAt = modal.StartAt;

        var builder = new ComponentBuilder()
            .WithButton("Продолжить", $"post_event_continue_btn:{cacheKey}", ButtonStyle.Primary);
    
        await ModifyOriginalResponseAsync(properties => 
        {
            properties.Embed = EmbedHelper
                .CreateBaseWithTitle("1/2", 
                    "Подготовьте ссылку на картинку.").Build();
            properties.Components = builder.Build();
        });
    }

    [ComponentInteraction("post_event_continue_btn:*", ignoreGroupNames: true)]
    public async Task OnContinueBtnClick(string cacheKey)
    {
        if (!_cache.TryGetValue(cacheKey, out PostData? _))
        {
            await RespondAsync("Ошибка: сессия потеряна.", ephemeral: true);
            return;
        }

        await Context.Interaction
            .RespondWithModalAsync<PostModal2>($"post_event_submit_2:{cacheKey}");
    }

    [ModalInteraction("post_event_submit_2:*", ignoreGroupNames: true)]
    public async Task OnOpenPostEvent2Modal(string cacheKey, PostModal2 modal)
    {
        await DeferAsync(ephemeral: true);

        if (!_cache.TryGetValue(cacheKey, out PostData? postData) ||
            postData is null)
        {
            await FollowupAsync(embed: EmbedHelper
                .CreateError("Сессия истекла, попробуйте ещё раз.").Build());
            return;
        }

        postData.ImageUrl = modal.ImageUrl;
        postData.Leaders = [.. modal.Leaders.Select(i => i.Id)];

        var eb = EmbedHelper.CreatePost(
            description: postData.Description!,
            imageUrl: postData.ImageUrl,
            placeId: postData.PlaceId!.Value,
            leaders: postData.Leaders,
            dt: postData.StartAt!,
            topic: postData.Topic!,
            rulesUrl: postData.RulesUrl,
            eventUrl: postData.EventUrl
        );

        string text = string.Empty;
        foreach (var mention in modal.Mentions)
            text += mention.Mention;

        await Context.Channel.SendMessageAsync(
            text: text, embed: eb.Build());
    
        await ModifyOriginalResponseAsync(properties => 
        {
            properties.Embed = EmbedHelper
                .CreateBaseWithTitle("2/2 Успех", 
                    "Поздравляю, вы запостили кринж!").Build();
            properties.Components = new ComponentBuilder().Build();
        });

        _cache.Remove(cacheKey);
    }
}