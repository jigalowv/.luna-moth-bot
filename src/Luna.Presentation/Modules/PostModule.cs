using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Luna.Application.Auth.ExecutorHasAccess;
using Luna.Domain.Enums;
using Luna.Presentation.Extensions;
using Luna.Presentation.Modules.Modals;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace Luna.Presentation.Modules;

[Group("post", "Группа для отправки сообщений.")]
public sealed class PostModule(
    ILogger<PostModule> logger,
    IMediator mediator,
    IMemoryCache cache) : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(15);

    #region Slash Commands

    [SlashCommand("raw", "Открывает окно для отправки сообщения.")]
    public async Task PostRawAsync()
    {
        if (!await EnsureCuratorAccessAsync()) return;
        await RespondWithModalAsync<PostRawModal>("post_raw_submit");
    }

    [SlashCommand("embed", "Открывает окно для отправки сообщения с embed.")]
    public async Task PostEmbedAsync()
    {
        if (!await EnsureCuratorAccessAsync()) return;
        await RespondWithModalAsync<PostEmbedModal>("post_embed_submit");
    }

    [SlashCommand("forum", "Открывает окно для отправки сообщения в форум.")]
    public async Task PostForumAsync()
    {
        if (!await EnsureCuratorAccessAsync()) return;
        await RespondWithModalAsync<PostForumModal>("post_forum_submit");
    }

    [SlashCommand("event", "Создать анонс события.")]
    public async Task PostEventAsync(
        [Summary("событие", "Ссылка на событие Discord")] string eventUrl,
        [Summary("время", "Timestamp проведения")] string time,
        [Summary("канал", "Канал для публикации")] SocketChannel? targetChannel = null)
    {
        if (!await EnsureCuratorAccessAsync()) return;

        targetChannel ??= (SocketChannel?)Context.Channel;

        if (targetChannel is not ISocketMessageChannel)
        {
            await RespondAsync(
                embed: EmbedHelper.CreateError("В указанный канал нельзя отправить сообщение.").Build(), 
                ephemeral: true);
            return;
        }

        var sessionId = Guid.NewGuid().ToString("N");
        var data = new PostEventData
        {
            TargetChannelId = targetChannel.Id,
            EventUrl = eventUrl,
            Timestamp = time
        };

        cache.Set(sessionId, data, TimeSpan.FromMinutes(30));
        await RespondWithModalAsync<PostEventModal1>($"post_event_modal_1:{sessionId}");
    }

    #endregion

    #region Raw & Embed Handlers

    [ModalInteraction("post_raw_submit", ignoreGroupNames: true)]
    public async Task PostRawSubmitAsync(PostRawModal modal)
    {
        await DeferAsync(ephemeral: true);

        if (modal.Channel is null)
        {
            modal.Channel = Context.Channel;
        }

        if (modal.Channel is not ISocketMessageChannel channel)
        {
            await FollowupAsync(embed: EmbedHelper.CreateError("Выберите корректный канал для отправки.").Build(), ephemeral: true);
            return;
        }

        await channel.SendMessageAsync(modal.Content);
        await FollowupAsync(embed: EmbedHelper.CreateBaseWithTitle("Успех", "Вы запостили кринж!").Build());
    }

    [ModalInteraction("post_embed_submit", ignoreGroupNames: true)]
    public async Task PostEmbedSubmitAsync(PostEmbedModal modal)
    {
        await DeferAsync(ephemeral: true);

        if (modal.Channel is null)
        {
            modal.Channel = Context.Channel;
        }

        if (modal.Channel is not ISocketMessageChannel channel)
        {
            await FollowupAsync(embed: EmbedHelper.CreateError("Выберите корректный канал для отправки.").Build(), ephemeral: true);
            return;
        }

        var embed = EmbedHelper
            .CreateBase(modal.Content, Color.Magenta)
            .WithImageUrl(modal.ImageUrl)
            .Build();

        await channel.SendMessageAsync(embed: embed);
        await FollowupAsync(embed: EmbedHelper.CreateBaseWithTitle("Успех", "Вы запостили кринж!").Build());
    }

    #endregion

    #region Forum Handlers

    [ModalInteraction("post_forum_submit", ignoreGroupNames: true)]
    public async Task PostForumSubmitAsync(PostForumModal modal)
    {
        await DeferAsync(ephemeral: true);

        var forum = modal.Forums.FirstOrDefault();
        if (forum is null)
        {
            await FollowupAsync("Вы не выбрали форум!", ephemeral: true);
            return;
        }

        if (forum.Tags.Count == 0)
        {
            await CreatePostAndRespondAsync(forum, modal.ForumTitle, modal.Description, modal.ImageUrl, []);
            return;
        }

        var sessionId = Guid.NewGuid().ToString("N");
        var cacheKey = $"pending_post:{sessionId}";
        var dto = new PendingForumPostDto(Context.User.Id, forum.Id, modal.ForumTitle, modal.Description, modal.ImageUrl);

        cache.Set(cacheKey, dto, CacheTtl);

        var selectMenu = new SelectMenuBuilder()
            .WithCustomId($"select_forum_tags:{sessionId}")
            .WithPlaceholder("Выберите тег(и) для поста...")
            .WithMinValues(1)
            .WithMaxValues(Math.Min(forum.Tags.Count, 25));

        foreach (var tag in forum.Tags.Take(25))
        {
            selectMenu.AddOption(tag.Name, tag.Id.ToString(), emote: tag.Emoji);
        }

        var components = new ComponentBuilder().WithSelectMenu(selectMenu).Build();
        var embed = EmbedHelper.CreateBaseWithTitle(
            "Шаг 2 из 2: Выбор тегов", 
            $"Заголовок: **{modal.ForumTitle}**\nВыберите подходящие теги из списка ниже."
        ).Build();

        await FollowupAsync(embed: embed, components: components, ephemeral: true);
    }

    [ComponentInteraction("select_forum_tags:*", ignoreGroupNames: true)]
    public async Task HandleTagSelectionAsync(string sessionId, string[] selectedTagIds)
    {
        await DeferAsync(ephemeral: true);

        var cacheKey = $"pending_post:{sessionId}";
        if (!cache.TryGetValue(cacheKey, out PendingForumPostDto? dto) || dto is null)
        {
            await FollowupAsync("Время ожидания выбора тегов истекло.", ephemeral: true);
            return;
        }

        if (dto.UserId != Context.User.Id)
        {
            await FollowupAsync("Вы не можете завершить создание чужого поста.", ephemeral: true);
            return;
        }

        var forum = Context.Guild.GetForumChannel(dto.ForumChannelId);
        if (forum is null)
        {
            await FollowupAsync("Канал-форум больше не доступен.", ephemeral: true);
            return;
        }

        var tagIds = Array.ConvertAll(selectedTagIds, ulong.Parse);
        await CreatePostAndRespondAsync(forum, dto.Title, dto.Description, dto.ImageUrl, tagIds);

        cache.Remove(cacheKey);
    }

    #endregion

    #region Event Handlers

    [ModalInteraction("post_event_modal_1:*", ignoreGroupNames: true)]
    public async Task OnOpenPostEventModal(string sessionId, PostEventModal1 modal)
    {   
        await DeferAsync(ephemeral: true);

        if (!cache.TryGetValue(sessionId, out PostEventData? postData) || postData is null)
        {
            await FollowupAsync(embed: EmbedHelper.CreateError("Сессия истекла, попробуйте ещё раз.").Build());
            return;
        }

        postData.Description = modal.Description;
        postData.Topic = modal.Topic;
        postData.ImageUrl = modal.ImageUrl;
        postData.RulesUrl = modal.RulesUrl;

        var components = new ComponentBuilder()
            .WithButton("Продолжить", $"post_event_continue_btn:{sessionId}", ButtonStyle.Primary)
            .Build();

        await ModifyOriginalResponseAsync(props => 
        {
            props.Embed = EmbedHelper.CreateBaseWithTitle("1/2", "Вспомните кого пинговать.").Build();
            props.Components = components;
        });
    }

    [ComponentInteraction("post_event_continue_btn:*", ignoreGroupNames: true)]
    public async Task OnContinueBtnClick(string sessionId)
    {
        if (!cache.TryGetValue(sessionId, out _))
        {
            await RespondAsync("Ошибка: сессия потеряна.", ephemeral: true);
            return;
        }

        await Context.Interaction.RespondWithModalAsync<PostEventModal2>($"post_event_submit_2:{sessionId}");
    }

    [ModalInteraction("post_event_submit_2:*", ignoreGroupNames: true)]
    public async Task OnOpenPostEvent2Modal(string sessionId, PostEventModal2 modal)
    {
        await DeferAsync(ephemeral: true);

        if (!cache.TryGetValue(sessionId, out PostEventData? postData) || postData is null)
        {
            await FollowupAsync(embed: EmbedHelper.CreateError("Сессия истекла, попробуйте ещё раз.").Build());
            return;
        }

        postData.LeaderIds = modal.Leaders.Select(i => i.Id).ToList();
        postData.PlaceChannelId = modal.Place.Id;
        postData.MentionsText = string.Join(" ", modal.Mentionables.Select(m => m.Mention));

        var embed = EmbedHelper.CreatePost(
            description: postData.Description,
            imageUrl: postData.ImageUrl,
            placeId: postData.PlaceChannelId,
            leaders: postData.LeaderIds.ToArray(),
            dt: postData.Timestamp,
            topic: postData.Topic,
            rulesUrl: postData.RulesUrl,
            eventUrl: postData.EventUrl
        ).Build();

        if (Context.Guild.GetChannel(postData.TargetChannelId) is not ISocketMessageChannel channel)
        {
            await FollowupAsync(embed: EmbedHelper.CreateError("Целевой канал не найден или недоступен.").Build(), ephemeral: true);
            return;
        }

        await channel.SendMessageAsync(text: postData.MentionsText, embed: embed);

        await ModifyOriginalResponseAsync(props => 
        {
            props.Embed = EmbedHelper.CreateBaseWithTitle("2/2 Успех", "Поздравляю, вы запостили кринж!").Build();
            props.Components = new ComponentBuilder().Build();
        });

        cache.Remove(sessionId);
    }

    #endregion

    #region Helpers

    private async Task<bool> EnsureCuratorAccessAsync()
    {
        try
        {
            var request = new AuthExecutorHasAccessRequest(Context.User.Id, ExecutorRole.Curator);
            var response = await mediator.Send(request);

            if (response.IsError)
            {
                var errorMsg = response.Errors.FirstOrDefault().Description ?? "Доступ запрещен.";
                await RespondAsync(embed: EmbedHelper.CreateError(errorMsg).Build(), ephemeral: true);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при проверке прав пользователя {UserId}", Context.User.Id);

            if (!Context.Interaction.HasResponded)
            {
                await RespondAsync("Произошла ошибка при обработке команды.", ephemeral: true);
            }

            return false;
        }
    }

    private async Task CreatePostAndRespondAsync(
        IForumChannel forum, 
        string title, 
        string description, 
        string imageUrl, 
        ulong[] tagIds)
    {
        var embedBuilder = EmbedHelper.CreateBase(description, Color.Magenta);

        if (Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
        {
            embedBuilder.WithImageUrl(imageUrl);
        }

        var tags = forum.Tags.Where(t => tagIds.Contains(t.Id)).ToArray();

        var post = await forum.CreatePostAsync(
            title: title,
            embed: embedBuilder.Build(),
            tags: tags
        );

        var successEmbed = EmbedHelper.CreateBaseWithTitle("Успех", $"Пост успешно опубликован: {post.Mention}").Build();
        await FollowupAsync(embed: successEmbed, ephemeral: true);
    }

    #endregion
}