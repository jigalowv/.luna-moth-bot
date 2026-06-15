using Luna.Domain.Enums;

namespace Luna.Application.Executors.Commands.List;

public record ExecutorListResponse(
    ICollection<ExecutorListResponseItem> Executors
);

public record ExecutorListResponseItem(
    ulong DiscordId,
    ExecutorRole Role,
    string Name
);