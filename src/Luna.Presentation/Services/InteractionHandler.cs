using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Luna.Application.Record.Events.UserJoinChannel;
using Luna.Application.Record.Events.UserLeftChannel;
using Luna.Application.Record.Events.UserSetDeafenStatus;
using Luna.Presentation.Modules;
using MediatR;

namespace Luna.Presentation.Services;

public sealed class InteractionHandler
{   
    private readonly ILogger<InteractionHandler> _logger;
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _services;
    private readonly DiscordSocketClient _client;

    public InteractionHandler(
        ILogger<InteractionHandler> logger,
        IServiceProvider services,
        DiscordSocketClient client,
        InteractionService interactionService)
    {
        _logger = logger;
        _services = services;
        _client = client;
        _interactionService = interactionService;
    }
    
    public async Task InitializeAsync()
    {
        await _interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
        
        _client.Ready += OnReadyAsync;
        _client.UserVoiceStateUpdated += OnUserVoiceStateUpdatedAsync;
        _client.InteractionCreated += OnInteractionCreatedAsync;
    }

    private async Task OnReadyAsync()
    {
        _logger.LogInformation("Bot is connected and ready.");
        
        try
        {
            await _interactionService.RegisterCommandsGloballyAsync();
            _logger.LogInformation("Commands registered globally.");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, 
                "Failed to register application commands during OnReady initialization.");
        }
    }

    private Task OnUserVoiceStateUpdatedAsync(
        SocketUser user, 
        SocketVoiceState before, 
        SocketVoiceState after)
    {
        if (user.IsBot)
            return Task.CompletedTask;

        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _services.CreateScope();
                
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                
                ulong? beforeChannelId = before.VoiceChannel?.Id;
                ulong? afterChannelId = after.VoiceChannel?.Id;

                if (beforeChannelId != afterChannelId)
                {
                    if (beforeChannelId is not null)
                    {
                        var leftRequest = new UserLeftChannelRequest(
                            beforeChannelId.Value, 
                            user.Id);

                        var result = await mediator.Send(leftRequest);
                        
                        if (result.IsError)
                            _logger.LogWarning(
                                "Left channel error: {Code}", result.Errors.First().Code);
                    }

                    if (afterChannelId is not null)
                    {
                        var joinRequest = new UserJoinChannelRequest(
                            afterChannelId.Value, 
                            user.Id, 
                            after.IsSelfDeafened);

                        var result = await mediator.Send(joinRequest);
                        
                        if (result.IsError)
                            _logger.LogWarning(
                                "Join channel error: {Code}", result.Errors.First().Code);
                    }
                }
                else if (
                    afterChannelId is not null && 
                    before.IsSelfDeafened != after.IsSelfDeafened)
                {
                    var setDeafenStatusRequest = new UserSetDeafenStatusRequest(
                        afterChannelId.Value, 
                        user.Id, 
                        after.IsSelfDeafened);
                    
                    var result = await mediator.Send(setDeafenStatusRequest);
                        
                    if (result.IsError)
                        _logger.LogWarning(
                            "Join channel error: {Code}", result.Errors.First().Code);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing voice state update for user {UserId}", user.Id);
            }
        });

        return Task.CompletedTask;
    }
    
    private async Task OnInteractionCreatedAsync(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(_client, interaction);
            
            var result = await _interactionService.ExecuteCommandAsync(context, _services);

            if (!result.IsSuccess)
            {
                _logger.LogError("Command execution failed: {ErrorReason}", result.ErrorReason);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while handling an interaction.");
            
            if (interaction.Type == InteractionType.ApplicationCommand)
            {
                await interaction.RespondAsync("⚠️ Произошла внутренняя ошибка при выполнении команды.", ephemeral: true);
            }
        }
    }
}