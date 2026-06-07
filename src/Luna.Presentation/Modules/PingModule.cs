using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Luna.Presentation.Modules;

[Group("example", "example description")]
public class PingModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<PingModule> _logger;

    public PingModule(ILogger<PingModule> logger)
    {
        _logger = logger;
    }

    [SlashCommand("ping", "Проверить задержку бота")]
    public async Task HandlePingCommand()
    {
        await RespondAsync("pong", ephemeral: true);
    }

    [SlashCommand("example", "Test command")]
    public async Task Example()
    {
        _logger.LogInformation("Вызвана слэш-команда 'example'.");
        await DeferAsync(ephemeral: true);

        try
        {
            var menu = new SelectMenuBuilder("example_id")
                .AddOption("Kick", "0")
                .AddOption("Dick", "1")
                .AddOption("Cock", "2");

            var components = new ComponentBuilderV2()
                .WithActionRow([menu])
                .Build();

            await FollowupAsync(components: components, ephemeral: true);
            
            _logger.LogDebug("Меню выбора успешно отправлено пользователю.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выполнении слэш-команды 'example'.");
        }
    }

    [ComponentInteraction("example_id", true)]
    public async Task HandleRecord(string[] values)
    {
        // 1. Мгновенно говорим Дискорду: "Я принял, подожди"
        // DeferAsync() для компонентов сообщает, что мы будем обновлять исходное сообщение
        await DeferAsync(); 

        var value = values[0];

        try 
        {
            // 3. Теперь безопасно обновляем
            await ModifyOriginalResponseAsync(msg =>
            {
                msg.Content = $"Выбран элемент: {value}";
                msg.Components = new ComponentBuilder().Build(); 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке");
        }
    }

    [SlashCommand("report", "Quickly report an issue")]
    public async Task OpenReportModalAsync()
    {
        // 1. Create the modal builder and set a Custom ID
        var modal = new ModalBuilder()
            .WithTitle("Quick Report")
            .WithCustomId("quick_report_form")
            
            // 2. Add input fields directly to it
            .AddTextInput(
                label: "What is the issue?", 
                customId: "issue_title", 
                style: TextInputStyle.Short, 
                placeholder: "e.g., Audio is lagging", 
                required: true)
                
            .AddTextInput(
                label: "Steps to reproduce", 
                customId: "issue_details", 
                style: TextInputStyle.Paragraph, 
                placeholder: "1. Join voice channel...\n2. ...", 
                required: false);

        // 3. Respond with the built modal
        await RespondWithModalAsync(modal.Build());
    }

    // The Custom ID here must match the ModalBuilder's Custom ID
    [ModalInteraction("quick_report_form")]
    public async Task HandleReportSubmitAsync(string issue_title, string issue_details)
    {
        // Acknowledge the interaction
        await DeferAsync(ephemeral: true);

        // 'issue_title' and 'issue_details' automatically map to the 
        // customId of the components we created in the builder.
        
        // Handle empty optional fields safely
        string details = string.IsNullOrWhiteSpace(issue_details) 
            ? "No details provided." 
            : issue_details;

        await FollowupAsync(
            text: $"Report submitted successfully!\n" +
                  $"**Issue:** {issue_title}\n" +
                  $"**Details:** {details}", 
            ephemeral: true
        );
    }
}