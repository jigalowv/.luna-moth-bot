using ErrorOr;
using MediatR;

namespace Luna.Application.Record.Events.UserJoinChannel;

public record UserJoinChannelRequest(
    ulong VoiceChannelId,
    ulong DiscordUserId,
    bool IsDeafened
) : IRequest<ErrorOr<bool>>;