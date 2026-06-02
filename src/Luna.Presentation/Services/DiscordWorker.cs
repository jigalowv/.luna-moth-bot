using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Luna.Presentation.Configurations;

namespace Luna.Presentation.Services;

public class DiscordWorker : BackgroundService
{
    private readonly DiscordSocketClient _client;
    private readonly IOptions<DiscordOptions> _options;
    private readonly InteractionHandler _handler;

    public DiscordWorker(
        DiscordSocketClient client,
        IOptions<DiscordOptions> options,
        InteractionHandler handler)
    {
        _handler = handler;
        _options = options;
        _client = client;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _handler.InitializeAsync();

        await _client.LoginAsync(TokenType.Bot, _options.Value.Token);
        await _client.StartAsync();
        
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            await _client.LogoutAsync();
            await _client.StopAsync();
        }
    }
}