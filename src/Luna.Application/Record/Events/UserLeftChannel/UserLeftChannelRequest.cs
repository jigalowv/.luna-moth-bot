using ErrorOr;
using MediatR;

namespace Luna.Application.Record.Events.UserLeftChannel;

public record UserLeftChannelRequest(
    ulong ChannelId,
    ulong DiscordUserId
) : IRequest<ErrorOr<bool>>;