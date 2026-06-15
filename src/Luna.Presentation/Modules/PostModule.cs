using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Luna.Application.Auth.ExecutorHasAccess;
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

    [SlashCommand("event", "Открыть окно для ввода анонса")]
    public async Task OpenModalAsync(
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

            await RespondWithModalAsync<PostModal>($"post_submit_1:{cacheKey}");
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

    [ModalInteraction("post_submit_1:*", ignoreGroupNames: true)]
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
            .WithButton("Продолжить", $"post_continue_btn:{cacheKey}", ButtonStyle.Primary);
    
        await ModifyOriginalResponseAsync(properties => 
        {
            properties.Embed = EmbedHelper
                .CreateBase("1/2", 
                    "Подготовьте ссылку на картинку.").Build();
            properties.Components = builder.Build();
        });
    }

    [ComponentInteraction("post_continue_btn:*", ignoreGroupNames: true)]
    public async Task OnContinueBtnClick(string cacheKey)
    {
        if (!_cache.TryGetValue(cacheKey, out PostData? _))
        {
            await RespondAsync("Ошибка: сессия потеряна.", ephemeral: true);
            return;
        }

        await Context.Interaction
            .RespondWithModalAsync<PostModal2>($"post_submit_2:{cacheKey}");
    }

    [ModalInteraction("post_submit_2:*", ignoreGroupNames: true)]
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
                .CreateBase("2/2 Успех", 
                    "Поздравляю, вы запостили кринж!").Build();
            properties.Components = new ComponentBuilder().Build();
        });

        _cache.Remove(cacheKey);
    }
}