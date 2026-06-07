namespace Luna.Application.EventTypes.Commands.List;

public record EventTypeListResponse(
    ICollection<string> Titles
);