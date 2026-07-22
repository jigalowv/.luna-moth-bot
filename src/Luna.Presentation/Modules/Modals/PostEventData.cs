namespace Luna.Presentation.Modules;

public record PostEventData
{
    public ulong TargetChannelId { get; init; }
    public string EventUrl { get; init; } = string.Empty;
    public string Timestamp { get; init; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string MentionsText { get; set; } = string.Empty;
    public ulong PlaceChannelId { get; set; }
    public List<ulong> LeaderIds { get; set; } = [];
    public string RulesUrl { get; set; } = string.Empty;
}