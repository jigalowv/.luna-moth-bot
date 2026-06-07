using ErrorOr;
using MediatR;

namespace Luna.Application.EventTypes.Commands.SetTitle;

public record EventTypeSetTitleRequest(
    ulong ExecutorDiscordId,
    string OldTitle,
    string NewTitle
) : IRequest<ErrorOr<bool>>;