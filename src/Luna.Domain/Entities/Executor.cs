using System.ComponentModel.DataAnnotations;
using Luna.Domain.Enums;

namespace Luna.Domain.Entities;

public class Executor
{
    public int UserId { get; set; }
    public ExecutorRole Role { get; set; }
    
    [MaxLength(20)]
    public string Name { get; set; } = null!;
    public string? ImageUrl { get; set; }

    public User User { get; set; } = null!;
    public ICollection<Event> Events { get; set; } = [];
    public ICollection<EventEditExecutor> EventEditExecutors { get; set; } = [];
    public ICollection<Record> Records { get; set; } = [];

    public bool CanAssignRole(ExecutorRole targetRole) =>
        this.Role >= ExecutorRole.Moderator && this.Role > targetRole;

    public bool CanReassignRole(ExecutorRole beforeRole, ExecutorRole afterRole) =>
        CanAssignRole(afterRole) && Role > beforeRole;
}