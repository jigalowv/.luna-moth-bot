using ErrorOr;
using MediatR;

namespace Luna.Application.EventTypes.Queries.EventTypeSearch;

public record EventTypeSearchRequest(
    ulong ExecutorDiscordId, 
    string SearchTerm
) : IRequest<ErrorOr<EventTypeSearchResponse>>;