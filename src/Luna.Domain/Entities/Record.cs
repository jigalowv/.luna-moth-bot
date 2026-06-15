namespace Luna.Domain.Entities;

public sealed class Record
{
    private Record() { }

    public int Id { get; set; }
    public ulong ChannelId { get; set; }
    public int ExecutorId { get; set; }
    public DateTime? StartAt { get; set; }
    public Executor Executor { get; set; } = null!;

    public ICollection<RecordAttendance> Attendances { get; private set; } = [];

    public static Record Create(
        ulong channelId,
        int executorId)
    {
        return new Record
        {
            ChannelId = channelId,
            ExecutorId = executorId
        };
    }
}