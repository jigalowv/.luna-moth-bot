namespace Luna.Presentation.Modules.Modals;

public record EventEditContext(
    ulong GuildId,
    ulong ChannelId,
    ulong MessageId
);