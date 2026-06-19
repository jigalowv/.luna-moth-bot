using Discord;

namespace Luna.Presentation.Extensions;

public static class EmbedHelper
{
    private const string FooterImageUrl = "https://raw.githubusercontent.com/jigalowv/.luna-moth-bot/main/bot_footer_img.jpg";
    private const string FooterText = "mryak";

    public static EmbedBuilder CreateBaseWithTitle(string title, string description, Color? color = null)
    {
        if (color is null)
            color = Color.Blue;

        return CreateBase(description, color)
            .WithTitle(title);
    }

    public static EmbedBuilder CreateBase(string description, Color? color = null)
    {
        if (color is null)
            color = Color.Blue;

        return new EmbedBuilder()
            .WithDescription(description)
            .WithCurrentTimestamp()
            .WithColor(color.Value)
            .WithFooter(new EmbedFooterBuilder()
                .WithIconUrl(FooterImageUrl)
                .WithText(FooterText));
    }

    public static EmbedBuilder CreatePost(
        string description,
        string imageUrl,
        ulong placeId,
        ulong[] leaders,
        string dt,
        string topic,
        string? rulesUrl,
        string? eventUrl
    )
    {
        var embed = CreateBase(description, Color.Magenta);
        
        string leadersStr = string.Empty;
        foreach (var leader in leaders)
            leadersStr += $"<@{leader}> ";

        embed.WithImageUrl(imageUrl)
             .AddField("Тема", topic, true)
             .AddField("Начало", dt, true)
             .AddField("Место", $"<#{placeId}>", true)
             .AddField("Ведущие", leadersStr, true);
            
        if (rulesUrl is not null)
            embed.AddField("Правила", rulesUrl, true);
        
        if (eventUrl is not null)
            embed.AddField("\u200B", $"[Ссылка на событие]({eventUrl})");

        return embed;
    }

    public static EmbedBuilder CreateError(string error)
    {
        return CreateBaseWithTitle(
            title: "Ошибка", 
            description: error,
            color: Color.Red);
    }

    public static EmbedBuilder CreateUpdate(string description)
    {
        return CreateBaseWithTitle(
            title: "Обновлено",
            description: description,
            color: Color.Green
        );
    }
}