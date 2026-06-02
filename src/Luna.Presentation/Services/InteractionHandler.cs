using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Luna.Presentation.Services;

public class InteractionHandler
{
    private const ulong TrackedVcId = 1317038114392510499;
    private const ulong TrackedGuildId = 1317038113796657223;
    
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
        #if DEBUG
            await _interactionService.RegisterCommandsToGuildAsync(TrackedGuildId);
            _logger.LogInformation($"Commands registered locally to guild: {TrackedGuildId}");
        #else
            await _interactionService.RegisterCommandsGloballyAsync();
            _logger.LogInformation("Commands registered globally.");
        #endif
    }

    private Task OnUserVoiceStateUpdatedAsync(
        SocketUser user, 
        SocketVoiceState before, 
        SocketVoiceState after)
    {
        if (user.IsBot)
            return Task.CompletedTask;

        ulong? beforeChannelId = before.VoiceChannel?.Id;
        ulong? afterChannelId = after.VoiceChannel?.Id;

        if (beforeChannelId == TrackedVcId && afterChannelId == TrackedVcId)
        {
            if (before.IsSelfDeafened != after.IsSelfDeafened)
            {
                if (after.IsSelfDeafened)
                    _logger.LogInformation("{Username}: doesn't listen now (deafened).", user.Username);
                else
                    _logger.LogInformation("{Username}: listens now (undeafened).", user.Username);
            }
            return Task.CompletedTask;
        }

        if (beforeChannelId == TrackedVcId && afterChannelId != TrackedVcId)
        {
            _logger.LogInformation("{Username}: left the tracked channel.", user.Username);
            return Task.CompletedTask;
        }

        if (beforeChannelId != TrackedVcId && afterChannelId == TrackedVcId)
        {
            _logger.LogInformation("{Username}: joined the tracked channel.", user.Username);
            return Task.CompletedTask;
        }

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