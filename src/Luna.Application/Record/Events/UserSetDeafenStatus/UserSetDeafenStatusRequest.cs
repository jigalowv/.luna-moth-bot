using ErrorOr;
using MediatR;

namespace Luna.Application.Record.Events.UserSetDeafenStatus;

public record UserSetDeafenStatusRequest(
    ulong ChannelId,
    ulong DiscordUserId,
    bool IsDeafened
) : IRequest<ErrorOr<bool>>;