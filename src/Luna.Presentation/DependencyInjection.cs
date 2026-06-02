using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Luna.Presentation.Configurations;
using Luna.Presentation.Services;

namespace Luna.Presentation
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<DiscordOptions>(
                configuration.GetSection(DiscordOptions.SectionName));
            services.AddSingleton(new DiscordSocketConfig
            {
                GatewayIntents =
                    GatewayIntents.Guilds |
                    GatewayIntents.GuildMembers |
                    GatewayIntents.GuildVoiceStates 
            });
            services.AddSingleton<DiscordSocketClient>();
            services.AddSingleton(provider => new InteractionService(
                provider.GetRequiredService<DiscordSocketClient>()));
            services.AddSingleton<InteractionHandler>();
            services.AddHostedService<DiscordWorker>();
            return services;
        }
    }
}