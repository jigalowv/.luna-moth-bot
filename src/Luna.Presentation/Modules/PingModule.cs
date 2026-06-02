using Discord.Interactions;

namespace Luna.Presentation.Modules;

public class PingModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Проверить задержку бота")]
    public async Task HandlePingCommand()
    {
        await RespondAsync("pong", ephemeral: true);
    }
}