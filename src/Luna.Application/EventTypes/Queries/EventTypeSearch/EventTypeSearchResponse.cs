namespace Luna.Application.EventTypes.Queries.EventTypeSearch;

public record EventTypeSearchResponse(
    ICollection<string> Titles
);