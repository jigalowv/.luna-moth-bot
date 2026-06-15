using Discord.Interactions;
using Luna.Presentation.Extensions;

namespace Luna.Presentation.Modules;

public class AboutModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("about", "информация о боте")]
    public async Task AboutAsync()
    {
        var eb = EmbedHelper.CreateBase(
            title: "luna moth",
            description: """
            Менеджер событий "Лампового Уголка".

            На данный момент содержит 4 модуля:
            - events edit - управление информацией о прошедших событиях.
            - record - управление записью событий в указанном голосовом канале.
            - users - управление информацией об участниках.
            - event types - управление видами событий.
            """
        )
            .AddField("Автор", "<@879434636403548210> (Мряк)", true)
            .AddField("Version", "1.0.0-alpha", true);
        await RespondAsync(embed: eb.Build());
    }
}