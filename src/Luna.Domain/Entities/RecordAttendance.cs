namespace Luna.Domain.Entities;

public sealed class RecordAttendance
{
    private RecordAttendance() { }

    public int Id { get; set; }
    public int RecordId { get; set; }
    public ulong DiscordUserId { get; set; }
    public bool IsDeafened { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public Record Record { get; set; } = null!;

    public static RecordAttendance Create(
        int recordId,
        ulong discordUserId,
        bool isDeafened)
    {
        return new RecordAttendance
        {
            RecordId = recordId,
            DiscordUserId = discordUserId,
            IsDeafened = isDeafened
        };
    }
}