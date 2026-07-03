using Discord.WebSocket;

namespace Luna.Presentation.Modules;

public class PostEventData
{
    public ulong? PlaceId { get; set; }
    public string? ImageUrl { get; set; }
    public string? RulesUrl { get; set; }
    public string? EventUrl { get; set; }
    public string? StartAt { get; set; }
    public string? Topic { get; set; }
    public string? Description { get; set; }
    public ulong[] Leaders { get; set; } = [];
    public SocketChannel? Channel { get; set; }
}