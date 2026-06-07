namespace Luna.Domain.Entities;

public class EventEditExecutor
{
    public int ExecutorId { get; set; }
    public int EventEditId { get; set; }
    public DateTime? CreatedAt { get; set; }    

    public User Executor { get; set; } = null!;
    public EventEdit EventEdit { get; set; } = null!;

    public static EventEditExecutor Create(
        int executorId
    )
    {
        return new EventEditExecutor
        {
            ExecutorId = executorId
        };
    }
}